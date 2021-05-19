using Jaika1.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using WerewolfDataLib;
using WerewolfDataLib.Interfaces;

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public static class NetworkingGlobal
{
    private const uint SharedSecret = 0x1A7D2F9Bu;
    private static string modsDirectory = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\My Games\Jaika1\Werewolf\Mods\";
    private static List<Assembly> modAsms = new List<Assembly>();

    private static UdpServer udpSv;
    private static UdpClient udpCl;
    private static List<NetWerewolfPlayer> players;
    private static WerewolfGameInfo gameInfo;

    public const int ClientEventGroup = 0;
    public const int ServerEventGroup = 1;

    public static NetWerewolfPlayer LocalPlayer;
    public static bool FirstLobby = true;
    public static IPAddress ClientConnectIP;
    public static int ClientConnectPort;
    public static Dictionary<string, Type> LoadedRoleTypes;// = new Dictionary<string, Type>();
    public static List<string> LoadedRoleHashes;
    public static List<string> ActiveRoleHashes = new List<string>();
    public static CancellationToken GameLoopCT;

    public static UdpServer ServerInstance => udpSv;
    public static UdpClient ClientInstance => udpCl;
    public static List<NetWerewolfPlayer> ConnectedPlayers => players;
    public static WerewolfGameInfo GameInfo => gameInfo;

    public static void LoadAllRoles()
    {
        List<Assembly> roleAsms = new List<Assembly>();
        roleAsms.Add(Assembly.GetExecutingAssembly()); // This assembly to load the internal roles.

        if (Directory.Exists(modsDirectory))
        {
            foreach (string dll in Directory.EnumerateFiles(modsDirectory, "*.dll"))
            {
                Assembly modAsm = Assembly.LoadFrom(dll);
                roleAsms.Add(modAsm);
                modAsms.Add(modAsm);
            }
        }
        else
        {
            Directory.CreateDirectory(modsDirectory);
        }

        LoadedRoleTypes = WerewolfGameInfo.LoadRolesFromAssemblies(roleAsms.ToArray());
        LoadedRoleHashes = WerewolfGameInfo.GenerateRoleHashes(LoadedRoleTypes.Values.ToArray());
    }

    public static bool Application_quitting()
    {
        CloseClientInstance();
        CloseServerInstance();
        return true;
    }

    public static bool InitializeServerInstance()
    {
        gameInfo = new WerewolfGameInfo(LoadedRoleTypes);
        players = new List<NetWerewolfPlayer>();
        udpSv = new UdpServer(SharedSecret);
        udpSv.AddNetEventsFromAssembly(Assembly.GetExecutingAssembly(), ServerEventGroup);
        udpSv.ClientConnected += ServerNetEvents.ClientConnectedEventHandler;
        udpSv.ClientDisconnected += ServerNetEvents.ClientDisconnectedEventHandler;
        return udpSv.StartServer(7235);
    }

    public static bool InitializeClientInstance(IPAddress ip, int port)
    {
        if (udpSv == null)
            players = new List<NetWerewolfPlayer>();
        udpCl = new UdpClient(SharedSecret);
        LocalPlayer = new NetWerewolfPlayer(udpCl, PlayerSettings.Instance.PlayerName);
        udpCl.AddNetEventsFromAssembly(Assembly.GetExecutingAssembly(), ClientEventGroup);
        udpCl.ClientDisconnected += ClientNetEvents.ClientDisconnectedEventHandler;
        return udpCl.VerifyAndListen(ip, port);
    }

    public static void CloseServerInstance()
    {
        try
        {
            if (udpSv != null)
            {
                udpSv.Close();
                udpSv = null;
                players = null;
            }
        }
        catch { }
    }

    public static void CloseClientInstance()
    {
        try
        {
            if (udpCl != null)
            {
                udpCl.Close();
                udpCl = null;

                if (udpSv == null)
                    players = null;
            }
        }
        catch { }
    }

    public static string GetRoleNameFromHash(string roleHash)
    {
        int roleIndex = LoadedRoleHashes.IndexOf(roleHash);

        if (roleIndex != -1)
            return LoadedRoleTypes.Keys.ElementAt(roleIndex);

        return string.Empty;
    }

    public static string GetRoleHashFromType(Type roleType)
    {
        int roleIndex = LoadedRoleTypes.Values.ToList().IndexOf(roleType);

        if (roleIndex != -1)
            return LoadedRoleHashes.ElementAt(roleIndex);

        return string.Empty;
    }

    public static Type GetRoleTypeFromHash(string roleHash)
    {
        int roleIndex = LoadedRoleHashes.IndexOf(roleHash);

        if (roleIndex != -1)
            return LoadedRoleTypes.Values.ElementAt(roleIndex);

        return null;
    }

    #region Extension Methods

    public static NetWerewolfPlayer GetPlayer(this UdpClient client)
    {
        for ( int i = 0; i < ConnectedPlayers.Count;  ++i)
        {
            NetWerewolfPlayer p = ConnectedPlayers[i];
            if (p.PlayerClient != null)
                if (p.PlayerClient.EndPoint.Equals(client.EndPoint))
                    return p;
        }
        return null;
    }

    public static UnityEngine.Color ToUnityColor(this System.Drawing.Color c)
    {
        return new UnityEngine.Color(c.R / 255.0f, c.G / 255.0f, c.B / 255.0f, c.A / 255.0f);
    }

    #endregion

    #region Game Loop

    public static GameState CurrentGameState;
    public static int CurrentDay;
    public static int StateTime;
    public static bool StateChanged;
    public static NetWerewolfPlayer PlayerOnTrial;
    public static IRoleAlignment[] WinningAlignments = null;

    public static async Task NetGameLoop()
    {
        CurrentGameState = GameState.Discussion;
        CurrentDay = 0;
        StateTime = 25;
        StateChanged = true;
        PlayerOnTrial = null;

        while (true)
        {
            if (StateChanged)
            {
                foreach (NetWerewolfPlayer p in ConnectedPlayers)
                {
                    p.TrialTargetPID = 0u;
                    p.TrialVotes = 0;
                    p.VotedForKill = false;
                }

                switch (CurrentGameState)
                {
                    case GameState.Discussion:
                        ServerInstance.SendF(5,PacketFlags.Reliable, 0u, "Discussion has begun.");
                        break;

                    case GameState.Night:
                        ServerInstance.SendF(5, PacketFlags.Reliable, 0u, "The sun has retreated as the moon rises...");
                        break;

                    case GameState.Dawn:
                        List<NetWerewolfPlayer> affectedPlayers = GameInfo.ResolveNightEvents().ConvertAll(p => p as NetWerewolfPlayer);
                        affectedPlayers.ForEach(p =>
                        {
                            ServerInstance.SendF(50, PacketFlags.Reliable, p.PlayerID, p.Status); // UpdatePlayerStatus(UdpClient, uint, PlayerStatus)
                            ServerInstance.SendF(5, PacketFlags.Reliable, 0u, $"{p.Name} is {p.Status}!{(p.Status == PlayerStatus.Dead ? $" Their role was {p.Role.Name}" : "")}");
                        });
                        break;

                    case GameState.Trial:
                        if (PlayerOnTrial != null)
                        {
                            ServerInstance.SendF(5, PacketFlags.Reliable, 0u, $"The town has decided to trial {PlayerOnTrial.Name}! Select their name to vote them guilty.");
                        }
                        else StateTime = 0;
                        break;

                    case GameState.End:
                        ServerInstance.SendF(5, PacketFlags.Reliable, 0u, $"The following players have won: {string.Join(", ", ConnectedPlayers.Where(p => WinningAlignments.Contains(p.Role.Alignment)).Select(p => p.Name))}");
                        break;
                }
            }

            StateChanged = false;

            await Task.Delay(1000);
            StateTime--;

            if (StateTime < 0)
            {
                switch (CurrentGameState)
                {
                    case GameState.Discussion:
                        CurrentGameState = GameState.Night;
                        StateTime = 15;
                        break;

                    case GameState.Night:
                        CurrentGameState = GameState.Dawn;
                        StateTime = 10;
                        CurrentDay++;
                        break;

                    case GameState.Dawn:
                        (bool End, IRoleAlignment[] WinList) result = GameInfo.CheckIfWinConditionMet();

                        if (result.End)
                        {
                            WinningAlignments = result.WinList;
                            CurrentGameState = GameState.End;
                            StateTime = 20;
                            break;
                        }

                        CurrentGameState = GameState.Discussion;
                        StateTime = 120;
                        break;


                    case GameState.Trial:
                        // TODO: Straight to night if voted out
                        if (PlayerOnTrial != null && PlayerOnTrial.Status != PlayerStatus.Dead)
                        {
                            int votes = players.Count(p => p.VotedForKill);
                            int voters = players.Count(p => p.PlayerID != PlayerOnTrial.PlayerID && p.Status == PlayerStatus.Alive);

                            if (votes >= Mathf.CeilToInt(voters / 2.0f))
                            {
                                PlayerOnTrial.Status = PlayerStatus.Dead;
                                ServerInstance.SendF(50, PacketFlags.Reliable, PlayerOnTrial.PlayerID, PlayerOnTrial.Status); // UpdatePlayerStatus(uint, PlayerStatus)
                                ServerInstance.SendF(5, PacketFlags.Reliable, 0u, $"The town has decided to execute {PlayerOnTrial.Name}! They were a {PlayerOnTrial.Role.Name}!");

                                (bool End, IRoleAlignment[] WinList) trialResult = GameInfo.CheckIfWinConditionMet();

                                if (trialResult.End)
                                {
                                    WinningAlignments = trialResult.WinList;
                                    CurrentGameState = GameState.End;
                                    StateTime = 20;
                                    break;
                                }

                                CurrentGameState = GameState.Night;
                                StateTime = 15;

                                break;
                            }
                        }

                        CurrentGameState = GameState.Discussion;
                        StateTime = 50;
                        break;

                    case GameState.End:
                        // TODO: Reset the lobby

                        ConnectedPlayers.ForEach(p => {
                            p.Status = PlayerStatus.Spectating;
                            p.Role = null;
                        });

                        ActiveRoleHashes = new List<string>();
                        PlayerOnTrial = null;

                        ServerInstance.SendF(192, PacketFlags.Reliable); // InvokeSceneFlip();
                        return;
                }
                StateChanged = true;
                PlayerOnTrial = null;
            }

            ServerInstance.Send(6, StateTime); // SetTimerValue(int);
        }
    }

    public enum GameState : byte
    {
        Discussion,
        Night,
        Dawn,
        Trial,
        End
    }

    #endregion
}

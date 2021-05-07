using NetworkingLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using WerewolfDataLib;

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public static class NetworkingGlobal
{
    private const uint SharedSecret = 0x1A7D2F9Bu;

    private static UdpServer udpSv;
    private static UdpClient udpCl;
    private static List<NetWerewolfPlayer> players;
    private static WerewolfGameInfo gameInfo;

    public const int ClientEventGroup = 0;
    public const int ServerEventGroup = 1;

    public static string PlayerName = $"Player{UnityEngine.Random.Range(1000,9999)}";
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


    static NetworkingGlobal()
    {
        // We must shut down the server and/or client when the editor stops, since that doesn't happen automatically for us.
        Application.wantsToQuit += Application_quitting;
        LoadedRoleTypes = WerewolfGameInfo.LoadRolesFromAssemblies(Assembly.GetExecutingAssembly());
        LoadedRoleHashes = WerewolfGameInfo.GenerateRoleHashes(LoadedRoleTypes.Values.ToArray());
    }

    private static bool Application_quitting()
    {
        CloseClientInstance();
        CloseServerInstance();
        return true;
    }

    public static void InitializeServerInstance()
    {
        gameInfo = new WerewolfGameInfo(LoadedRoleTypes);
        players = new List<NetWerewolfPlayer>();
        udpSv = new UdpServer(SharedSecret);
        udpSv.AddNetEventsFromAssembly(Assembly.GetExecutingAssembly(), ServerEventGroup);
        udpSv.ClientConnected += ServerNetEvents.ClientConnectedEventHandler;
        udpSv.ClientDisconnected += ServerNetEvents.ClientDisconnectedEventHandler;
        udpSv.StartServer(7235);
    }

    public static void InitializeClientInstance(IPAddress ip, int port)
    {
        if (udpSv == null)
            players = new List<NetWerewolfPlayer>();
        udpCl = new UdpClient(SharedSecret);
        LocalPlayer = new NetWerewolfPlayer(udpCl, PlayerName);
        udpCl.AddNetEventsFromAssembly(Assembly.GetExecutingAssembly(), ClientEventGroup);
        udpCl.ClientDisconnected += ClientNetEvents.ClientDisconnectedEventHandler;
        bool success = udpCl.VerifyAndListen(ip, port);
    }

    public static void CloseServerInstance()
    {
        try
        {
            if (udpSv != null)
            {
                udpSv.CloseServer();
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
                udpCl.Disconnect();
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

    public static async Task NetGameLoop()
    {
        CurrentGameState = GameState.Discussion;
        CurrentDay = 0;
        StateTime = 25;
        StateChanged = true;

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
                        ServerInstance.Send(5, 0u, "Discussion has begun.");
                        break;

                    case GameState.Night:
                        ServerInstance.Send(5, 0u, "The sun has retreated as the moon rises...");
                        break;

                    case GameState.Dawn:
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
                        StateChanged = true;
                        StateTime = 30;
                        break;

                    case GameState.Night:
                        CurrentGameState = GameState.Dawn;
                        StateChanged = true;
                        StateTime = 10;
                        CurrentDay++;
                        break;

                    case GameState.Dawn:
                        CurrentGameState = GameState.Discussion;
                        StateChanged = true;
                        StateTime = 130;
                        break;


                    case GameState.Trial:
                        // TODO: Straight to night if voted out
                        CurrentGameState = GameState.Discussion;
                        StateChanged = true;
                        StateTime = 130;
                        break;

                    case GameState.End:
                        // TODO: Reset the lobby
                        StateTime = 999;
                        break;
                }
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

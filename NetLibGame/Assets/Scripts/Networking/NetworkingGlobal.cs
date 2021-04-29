using NetworkingLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
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

    public static NetWerewolfPlayer LocalPlayer;
    public static bool FirstLobby = true;
    public static IPAddress ClientConnectIP;
    public static int ClientConnectPort;
    public static Dictionary<string, Type> LoadedRoleTypes;// = new Dictionary<string, Type>();
    public static List<string> LoadedRoleHashes;

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
        udpCl.AddNetEventsFromAssembly(Assembly.GetExecutingAssembly(), ClientEventGroup);
        udpCl.ClientDisconnected += ClientNetEvents.ClientDisconnectedEventHandler;
        bool success = udpCl.VerifyAndListen(ip, port);
        LocalPlayer = new NetWerewolfPlayer(udpCl);
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

    #endregion
}

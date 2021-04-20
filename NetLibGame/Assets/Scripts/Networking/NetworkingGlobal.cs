using NetworkingLibrary;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class NetworkingGlobal
{
    private const uint SharedSecret = 0x1A7D2F9Bu;

    private static UdpServer udpSv;
    private static UdpClient udpCl;
    private static List<WerewolfPlayer> players;

    public const int ClientEventGroup = 0;
    public const int ServerEventGroup = 1;

    public static WerewolfPlayer LocalPlayer;
    public static bool FirstLobby;

    public static UdpServer ServerInstance => udpSv;
    public static UdpClient ClientInstance => udpCl;
    public static List<WerewolfPlayer> ConnectedPlayers => players;


    static NetworkingGlobal()
    {
#if UNITY_EDITOR
        // We must shut down the server when the editor stops, since that doesn't happen automatically for us.
        EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
#else
        // Similar scenario for a compiled executable to be safe.
        Application.quitting += Application_quitting;
#endif
    }

    private static void EditorApplication_playModeStateChanged(PlayModeStateChange obj)
    {
        if (obj == PlayModeStateChange.ExitingPlayMode)
        {
            CloseClientInstance();
            CloseServerInstance();
        }
    }

    private static void Application_quitting()
    {
        CloseClientInstance();
        CloseServerInstance();
    }


    public static void InitializeServerInstance()
    {
        players = new List<WerewolfPlayer>();
        udpSv = new UdpServer(SharedSecret);
        udpSv.AddNetEventsFromAssembly(Assembly.GetExecutingAssembly(), ServerEventGroup);
        udpSv.ClientConnected += ServerNetEvents.ClientConnectedEventHandler;
        udpSv.ClientDisconnected += ServerNetEvents.ClientDisconnectedEventHandler;
        udpSv.StartServer(7235);
    }

    public static void InitializeClientInstance(IPAddress ip, int port)
    {
        FirstLobby = true;
        players = new List<WerewolfPlayer>();
        udpCl = new UdpClient(SharedSecret);
        udpCl.AddNetEventsFromAssembly(Assembly.GetExecutingAssembly(), ClientEventGroup);
        udpCl.ClientDisconnected += ClientNetEvents.ClientDisconnectedEventHandler;
        udpCl.VerifyAndListen(ip, port);
        LocalPlayer = new WerewolfPlayer(udpCl);
    }

    public static void CloseServerInstance()
    {
        if (udpSv != null)
        {
            udpSv.CloseServer();
            udpSv = null;
            players = null;
        }
    }

    public static void CloseClientInstance()
    {
        if (udpCl != null)
        {
            udpCl.Disconnect();
            udpCl = null;

            if (udpSv == null)
                players = null;
        }
    }

    #region Extension Methods

    public static WerewolfPlayer GetPlayer(this UdpClient client)
    {
        return ConnectedPlayers.Find(p => p.PlayerClient.EndPoint.Equals(client.EndPoint));
    }

    #endregion
}

using NetworkingLibrary;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using UnityEngine;

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

    #region Extension Methods

    public static WerewolfPlayer GetPlayer(this UdpClient client)
    {
        return ConnectedPlayers.Find(p => p.PlayerClient.EndPoint.Equals(client.EndPoint));
    }

    #endregion
}

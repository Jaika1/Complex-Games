using NetworkingLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using static NetworkingGlobal;

public sealed class ClientNetEvents
{
    #region Event handlers and events

    public static void ClientDisconnectedEventHandler(UdpClient client)
    {
        ConnectedPlayers.Remove(client.GetPlayer());
    }

    //public static event Action<NetWerewolfPlayer, bool> NetPlayerConnected;
    //public static event Action<NetWerewolfPlayer, string> ChatMessageReceived;
    public static UnityEvent<NetWerewolfPlayer, bool> NetPlayerConnected = new UnityEvent<NetWerewolfPlayer, bool>();
    public static UnityEvent<NetWerewolfPlayer> NetPlayerDisconnected = new UnityEvent<NetWerewolfPlayer>();

    #endregion


    [NetDataEvent(0, ClientEventGroup)]
    static void CreateRoleHashesAndVerify(UdpClient client)
    {
        // TODO

        client.Send(0, "Player", new ulong[0]); // VerifyRoleHashesAndSendClientList(ulong[])
    }

    [NetDataEvent(1, ClientEventGroup)]
    static void UpdateClientInfo(UdpClient client, uint pid, string name)
    {
        NetWerewolfPlayer player = ConnectedPlayers.FirstOrDefault(p => p.PlayerID == pid);

        if (player.PlayerID != LocalPlayer.PlayerID && (player == null || ServerInstance != null))
        {
            player = new NetWerewolfPlayer(pid, name);
            if (ServerInstance == null)
                ConnectedPlayers.Add(player);
            NetPlayerConnected.Invoke(player, false);
        }
        else
        {
            player.Name = name;
        }
    }

    [NetDataEvent(2, ClientEventGroup)]
    static void RemoteClientDisconnected(UdpClient client, uint pid)
    {
        if (LocalPlayer.PlayerID == pid)
        {
            CloseClientInstance();
        }

        if (ServerInstance == null)
        {
            NetWerewolfPlayer player = ConnectedPlayers.Find(p => p.PlayerID == pid);
            ConnectedPlayers.Remove(player);

            // TODO: GameInfo removal (Maybe make dead if alive, and then remove once spectator?)
            
            NetPlayerDisconnected.Invoke(player);
        }
    }

    [NetDataEvent(200, ClientEventGroup)]
    static void ReceivePlayerList(UdpClient client, uint myPID, uint[] playerIDs, string[] playerNames)
    {
        LocalPlayer.PlayerID = myPID;

        for (int i = 0; i < playerIDs.Length; ++i)
        {
            bool isLocalPlayer = LocalPlayer.PlayerID == playerIDs[i];
            NetWerewolfPlayer netPlayer = isLocalPlayer ? LocalPlayer : new NetWerewolfPlayer(playerIDs[i], playerNames[i]);

            if (isLocalPlayer)
                LocalPlayer.Name = playerNames[i];

            if (ServerInstance == null)
                ConnectedPlayers.Add(netPlayer);

            NetPlayerConnected.Invoke(netPlayer, LocalPlayer.PlayerID == playerIDs[i]);
        }
    }
}

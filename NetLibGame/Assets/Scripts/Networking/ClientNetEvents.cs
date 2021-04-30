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
        //ConnectedPlayers.Remove(client.GetPlayer());
    }

    public static UnityEvent<NetWerewolfPlayer, bool> NetPlayerConnected = new UnityEvent<NetWerewolfPlayer, bool>();
    public static UnityEvent<uint> NetPlayerDisconnected = new UnityEvent<uint>();
    public static UnityEvent<NetWerewolfPlayer, string> ChatMessageReceived = new UnityEvent<NetWerewolfPlayer, string>();
    public static UnityEvent<bool> UpdateHostBox = new UnityEvent<bool>();
    public static UnityEvent<string, bool> UpdateActiveRoleList = new UnityEvent<string, bool>();

    #endregion


    [NetDataEvent(0, ClientEventGroup)]
    static void CreateRoleHashesAndVerify(UdpClient client)
    {
        client.Send(0, LocalPlayer.Name, LoadedRoleHashes.ToArray()); // VerifyRoleHashesAndSendClientList(string, string[])
    }

    [NetDataEvent(1, ClientEventGroup)]
    static void UpdateClientInfo(UdpClient client, uint pid, string name)
    {
        NetWerewolfPlayer player = ConnectedPlayers.FirstOrDefault(p => p.PlayerID == pid);

        if ((player == null) || (player.PlayerID != LocalPlayer.PlayerID))
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
            ConnectedPlayers.Remove(ConnectedPlayers.Find(p => p.PlayerID == pid));

        // TODO: GameInfo removal (Maybe make dead if alive, and then remove once spectator?)
            
        NetPlayerDisconnected.Invoke(pid);
    }

    [NetDataEvent(5, ClientEventGroup)]
    static void ReceivedChatMessage(UdpClient client, uint pid, string message)
    {
        ChatMessageReceived.Invoke(ConnectedPlayers.Find(p => p.PlayerID == pid), message);
    }

    [NetDataEvent(190, ClientEventGroup)]
    static void ModifyActiveRoleList(UdpClient sender, string roleHash, bool remove)
    {
        UpdateActiveRoleList.Invoke(roleHash, remove);
    }

    [NetDataEvent(199, ClientEventGroup)]
    static void SetHost(UdpClient client, bool isHost)
    {
        LocalPlayer.IsHost = isHost;
        UpdateHostBox.Invoke(isHost);
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

using NetworkingLibrary;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static NetworkingGlobal;

public sealed class ServerNetEvents
{
    #region Event handlers

    public static void ClientConnectedEventHandler(UdpClient client)
    {
        ConnectedPlayers.Add(new NetWerewolfPlayer(client));
        client.Send(0); // CreateRoleHashesAndVerify()
    }

    public static void ClientDisconnectedEventHandler(UdpClient client)
    {
        NetWerewolfPlayer player = client.GetPlayer();
        ServerInstance.Send(2, player.PlayerID); // RemoteClientDisconnected(uint)
        ConnectedPlayers.Remove(player);
    }

    #endregion


    [NetDataEvent(0, ServerEventGroup)]
    static void VerifyRoleHashesAndSendClientList(UdpClient sender, string playerName, ulong[] roleHashes)
    {
        // TODO: verify the role hashes


        NetWerewolfPlayer playerRef = sender.GetPlayer();
        playerRef.Name = playerName;
        playerRef.RoleListVerified = true;

        GameInfo.AddPlayerAndAssignId(playerRef);

        sender.Send(200, playerRef.PlayerID, ConnectedPlayers.Select(p => p.PlayerID).ToArray(), ConnectedPlayers.Select(p => p.Name).ToArray()); // ReceivePlayerList(uint, uint[], string[]);

        ServerInstance.Send(1, playerRef.PlayerID, playerRef.Name); // UpdateClientInfo(uint, string)
    }

    [NetDataEvent(5, ServerEventGroup)]
    static void BroadcastChatMessage(UdpClient sender, string message)
    {
        NetWerewolfPlayer p = sender.GetPlayer();
        if (p != null)
            ServerInstance.Send(5, p.PlayerID, message); // ReceivedChatMessage(uint, string)
    }
}

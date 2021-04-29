using NetworkingLibrary;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        try
        {
            ConnectedPlayers.Remove(player);
            GameInfo.Players.Remove(player);
        }
        catch { }

        NetWerewolfPlayer nextHost = ConnectedPlayers.First();
        nextHost?.PlayerClient.Send(199, true);
    }

    #endregion


    [NetDataEvent(0, ServerEventGroup)]
    static void VerifyRoleHashesAndSendClientList(UdpClient sender, string playerName, string[] roleHashes)
    {
        // TODO: verify the role hashes

        if (roleHashes.Length < LoadedRoleHashes.Count)
        {
            NetBase.WriteDebug($"Received role hash list from {sender.EndPoint as IPEndPoint} \"{playerName}\" has less roles than the server, impossible to verify roles.");
            ConnectedPlayers.Remove(sender.GetPlayer());
            sender.Disconnect();
            return;
        }

        for(int i = 0; i < LoadedRoleHashes.Count; ++i)
        {
            string hash = LoadedRoleHashes[i];
            if (!roleHashes.Contains(hash))
            {
                NetBase.WriteDebug($"Client {sender.EndPoint as IPEndPoint} \"{playerName}\" missing hash {hash} corresponding to role {LoadedRoleTypes.Values.ElementAt(i).AssemblyQualifiedName}!");
                ConnectedPlayers.Remove(sender.GetPlayer());
                sender.Disconnect();
                return;
            }
            else
            {
                NetBase.WriteDebug($"{sender.EndPoint as IPEndPoint}: {hash} {LoadedRoleTypes.Values.ElementAt(i).AssemblyQualifiedName} success!");
            }
        }

        NetWerewolfPlayer playerRef = sender.GetPlayer();
        playerRef.Name = playerName;
        playerRef.RoleListVerified = true;

        GameInfo.AddPlayerAndAssignId(playerRef);

        if (GameInfo.Players.Count == 1)
        {
            sender.Send(199, true); //SetHost(UdpClient, bool)
        }

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

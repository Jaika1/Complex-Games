using NetworkingLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using static NetworkingGlobal;

public sealed class ServerNetEvents
{
    static System.Random rand = new System.Random();
    const int MIN_PLAYERS = 2;

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
        ServerInstance.Send(5, 0u, $"{player.Name} has fallen...");

        try
        {
            ConnectedPlayers.Remove(player);
            GameInfo.Players.Remove(player);
        }
        catch { }

        NetWerewolfPlayer nextHost = ConnectedPlayers.First();
        if (nextHost != null && !nextHost.IsHost)
        {
            nextHost.IsHost = true;
            nextHost.PlayerClient.Send(199, true);
            ServerInstance.Send(5, 0, $"{nextHost.Name} is now the game master.");
        }
    }

    #endregion

    static string GenRandomJoinMessage(string playerName)
    {
        switch (rand.Next(5))
        {
            case 0:
                return $"{playerName} has entered the realm.";
            case 1:
                return $"{playerName} faces the oubliette of suffering.";
            case 2:
                return $"{playerName}'s body is ready.";
            case 3:
                return $"{playerName} brought the silver bullets.";
            case 4:
                return $"{playerName}'s revenge is upon us.";
        }
        return $"{playerName} has joined.";
    }

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

        ServerInstance.Send(5, 0u, GenRandomJoinMessage(playerName));

        if (GameInfo.Players.Count == 1)
        {
            playerRef.IsHost = true;
            sender.Send(199, true); //SetHost(UdpClient, bool)
            ServerInstance.Send(5, 0u, $"{playerRef.Name} is now the game master.");
        }

        sender.Send(200, playerRef.PlayerID, ConnectedPlayers.Select(p => p.PlayerID).ToArray(), ConnectedPlayers.Select(p => p.Name).ToArray()); // ReceivePlayerList(uint, uint[], string[]);

        foreach (string hash in ActiveRoleHashes)
            sender.Send(190, hash, false);

        ServerInstance.Send(1, playerRef.PlayerID, playerRef.Name); // UpdateClientInfo(uint, string)
    }

    [NetDataEvent(5, ServerEventGroup)]
    static void BroadcastChatMessage(UdpClient sender, string message)
    {
        NetWerewolfPlayer p = sender.GetPlayer();
        if (p != null)
        {
            double secondsSinceLast = (DateTime.Now - p.LastMessageTime).TotalSeconds;
            if (secondsSinceLast >= 3.5)
            {
                ServerInstance.Send(5, p.PlayerID, message); // ReceivedChatMessage(uint, string)
                p.LastMessageTime = DateTime.Now;
            }
            else
            {
                sender.Send(5, 0, $"Please wait another {Math.Round(3.5 - secondsSinceLast, 1)} seconds before sending another message."); // ReceivedChatMessage(uint, string)
            }
        }
    }

    [NetDataEvent(190, ServerEventGroup)]
    static void ModifyActiveRoleList(UdpClient sender, string roleHash, bool remove)
    {
        if (remove)
        {
            int activeIndex = ActiveRoleHashes.IndexOf(roleHash);

            if (activeIndex == -1)
                return;

            ActiveRoleHashes.RemoveAt(activeIndex);
        }
        else
        {
            ActiveRoleHashes.Add(roleHash);
        }

        ServerInstance.Send(190, roleHash, remove); // ModifyActiveRoleList(string, bool)
    }

    [NetDataEvent(191, ServerEventGroup)]
    static void TryStartGame(UdpClient sender)
    {
        if (GameInfo.Players.Count < MIN_PLAYERS)
        {
            sender.Send(5, 0u, $"There must be at least {MIN_PLAYERS} players to start the round.");
            return;
        }
        if (ActiveRoleHashes.Count < MIN_PLAYERS)
        {
            sender.Send(5, 0u, $"There must be enough roles in the active list for at least {MIN_PLAYERS} players.");
            return;
        }
        if (ActiveRoleHashes.Count > GameInfo.Players.Count)
        {
            sender.Send(5, 0u, $"There are more active roles than there are players! ({ActiveRoleHashes.Count}/{GameInfo.Players.Count})");
            return;
        }

        sender.Send(5, 0u, "Start conditions met, game would begin now!");

        // TODO: go to game scene

        List<Type> roleTypes = (from h in ActiveRoleHashes
                                from t in LoadedRoleTypes.Values
                                let th = GetRoleHashFromType(t)
                                where h == th
                                select t).ToList();

        GameInfo.AssignRolesAndSpectators(ActiveRoleHashes.Count, roleTypes);

        foreach (NetWerewolfPlayer p in ConnectedPlayers)
            p.PlayerClient.Send(191, GetRoleHashFromType(p.Role.GetType()));
    }
}

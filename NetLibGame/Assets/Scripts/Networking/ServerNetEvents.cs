using NetworkingLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using WerewolfDataLib;
using static NetworkingGlobal;

public sealed class ServerNetEvents
{
    static System.Random rand = new System.Random();
    const int MIN_PLAYERS = 4;

    #region Event handlers

    public static void ClientConnectedEventHandler(UdpClient client)
    {
        ConnectedPlayers.Add(new NetWerewolfPlayer(client));
        client.Send(0); // CreateRoleHashesAndVerify()
    }

    public static void ClientDisconnectedEventHandler(UdpClient client)
    {
        NetWerewolfPlayer player = client.GetPlayer();
        player.Status = PlayerStatus.Dead;
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

    [NetDataEvent(20, ServerEventGroup)]
    static void RequestActionOnPlayer(UdpClient sender, uint pid)
    {
        NetWerewolfPlayer player = sender.GetPlayer();
        NetWerewolfPlayer target = ConnectedPlayers.Find(p => p.PlayerID == pid);

        switch (player.Status)
        {
            case PlayerStatus.Dead:
                sender.Send(5, 0u, $"You are dead and cannot perform any more actions!");
                return;
            case PlayerStatus.Spectating:
                sender.Send(5, 0u, $"Spectators cannot perform in-game actions.");
                return;
        }

        if (target == null)
        {
            sender.Send(5, 0u, $"Targeted player with ID {pid} not found.");
            return;
        }

        if (CurrentDay == 0 && CurrentGameState == GameState.Discussion)
        {
            sender.Send(5, 0u, $"No votes for execution will be considered until the first night is over!");
            return;
        }

        switch (CurrentGameState)
        {
            case GameState.Dawn:
                sender.Send(5, 0u, $"Please wait until dawn is over to begin voting.");
                break;

            case GameState.Discussion:
                // TODO: Vote against player
                if (target.PlayerID == player.PlayerID)
                {
                    sender.Send(5, 0u, $"You cannot vote yourself onto trial!");
                    break;
                }

                if (player.TrialTargetPID != target.PlayerID)
                {
                    player.TrialTargetPID = target.PlayerID;
                    target.TrialVotes++;
                    ServerInstance.Send(5, 0u, $"{player.Name} has voted to trial {target.Name}! ({target.TrialVotes}/2)");

                    if (target.TrialVotes >= 2 && PlayerOnTrial == null)
                    {
                        PlayerOnTrial = target;
                        CurrentGameState = GameState.Trial;
                        StateChanged = true;
                        StateTime = 20;
                    }
                }

                else if (player.TrialTargetPID == target.PlayerID)
                {
                    player.TrialTargetPID = 0u;
                    target.TrialVotes--;
                    ServerInstance.Send(5, 0u, $"{player.Name} has revoked their vote to trial {target.Name}. ({target.TrialVotes}/2)");
                }

                break;

            case GameState.Trial:
                if (PlayerOnTrial != null && PlayerOnTrial.Status != PlayerStatus.Dead)
                {
                    if (player.PlayerID == PlayerOnTrial.PlayerID)
                    {
                        sender.Send(5, 0u, $"You are on trial and cannot vote to execute yourself!");
                        break;
                    }

                    player.VotedForKill = !player.VotedForKill;

                    if (player.VotedForKill)
                        sender.Send(5, 0u, $"You have voted to kill {PlayerOnTrial.Name}.");
                    else
                        sender.Send(5, 0u, $"You have decided to revoke your vote to kill {PlayerOnTrial.Name}.");
                }
                else
                {
                    sender.Send(5, 0u, $"The player who was on trial can no longer be voted against.");
                }
                break;

            case GameState.Night:
                // TODO: Night abilities
                if (player.Role.NightEvent == null)
                {
                    sender.Send(5, 0u, $"The {player.Role.Name} role does not have a night ability!");
                    break;
                }

                if (player.Role.NightEvent.EventTargets == 0)
                {
                    sender.Send(5, 0u, $"Your night ability is passive and does not require a target.");
                    break;
                }

                if (player.Role.NightEvent.TargetPlayers[0] == null || player.Role.NightEvent.TargetPlayers[0].PlayerID != target.PlayerID)
                {
                    player.Role.NightEvent.TargetPlayers[0] = target;
                    sender.Send(5, 0u, $"You have decided to target {target.Name}."); // TODO: custom action text
                }
                else
                {
                    player.Role.NightEvent.TargetPlayers[0] = null;
                    sender.Send(5, 0u, $"You have instead decided not to perform your night ability.");
                }

                break;

            case GameState.End:
                sender.Send(5, 0u, $"The game is already over!");
                break;
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

        List<Type> roleTypes = (from h in ActiveRoleHashes
                                from t in LoadedRoleTypes.Values
                                let th = GetRoleHashFromType(t)
                                where h == th
                                select t).ToList();

        GameInfo.AssignRolesAndSpectators(ActiveRoleHashes.Count, roleTypes);

        foreach (NetWerewolfPlayer p in ConnectedPlayers)
            p.PlayerClient.Send(191, GetRoleHashFromType(p.Role.GetType()));

        GameLoopCT = new CancellationToken(false);
        _ = Task.Run(NetGameLoop, GameLoopCT);
    }
}

using NetworkingLibrary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NetworkingGlobal;

public class ServerNetEvents
{
    #region Event handlers

    public static void ClientConnectedEventHandler(UdpClient client)
    {
        ConnectedPlayers.Add(new WerewolfPlayer(client));
        client.Send(0); // CreateRoleHashesAndVerify()
    }

    public static void ClientDisconnectedEventHandler(UdpClient client)
    {
        ConnectedPlayers.Remove(client.GetPlayer());
    }

    #endregion


    [NetDataEvent(0, ServerEventGroup)]
    static void VerifyRoleHashesAndSendClientList(UdpClient sender, ulong[] roleHashes)
    {
        WerewolfPlayer playerRef = sender.GetPlayer();
        // TODO

        playerRef.RoleListVerified = true;
    }
}

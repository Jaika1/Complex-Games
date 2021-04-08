using NetworkingLibrary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NetworkingGlobal;

public class ClientNetEvents
{
    #region Event handlers

    public static void ClientDisconnectedEventHandler(UdpClient client)
    {
        ConnectedPlayers.Remove(client.GetPlayer());
    }

    #endregion


    [NetDataEvent(0, ClientEventGroup)]
    static void CreateRoleHashesAndVerify(UdpClient client)
    {
        // TODO

        client.Send(0, new ulong[0]); // VerifyRoleHashesAndSendClientList(ulong[])
    }
}

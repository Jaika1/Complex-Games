using NetworkingLibrary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WerewolfPlayer
{
    private UdpClient clientRef;

    public bool RoleListVerified = false;


    public bool HasVerified => RoleListVerified;
    public UdpClient PlayerClient => clientRef;


    public WerewolfPlayer(UdpClient clientRef)
    {
        this.clientRef = clientRef;
    }
}

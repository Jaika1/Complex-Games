using NetworkingLibrary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WerewolfDataLib;

public class NetWerewolfPlayer : WerewolfPlayer
{
    private UdpClient clientRef;

    internal bool IsHost = false;

    public bool RoleListVerified = false;
    public string Name = "ERROR";

    public bool HasVerified => RoleListVerified;
    public UdpClient PlayerClient => clientRef;


    public NetWerewolfPlayer(UdpClient clientRef)
    {
        this.clientRef = clientRef;
    }

    public NetWerewolfPlayer(uint pid, string name)
    {
        PlayerID = pid;
        Name = name;
    }
}

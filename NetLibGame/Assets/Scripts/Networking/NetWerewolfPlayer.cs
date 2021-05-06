using NetworkingLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WerewolfDataLib;

public class NetWerewolfPlayer : WerewolfPlayer
{
    private UdpClient clientRef;

    internal bool IsHost = false;
    internal DateTime LastMessageTime = DateTime.MinValue;

    public bool RoleListVerified = false;
    public string Name;

    public bool HasVerified => RoleListVerified;
    public UdpClient PlayerClient => clientRef;


    public NetWerewolfPlayer(UdpClient clientRef, string name = "ERROR")
    {
        Name = name;
        this.clientRef = clientRef;
    }

    public NetWerewolfPlayer(uint pid, string name)
    {
        PlayerID = pid;
        Name = name;
    }
}

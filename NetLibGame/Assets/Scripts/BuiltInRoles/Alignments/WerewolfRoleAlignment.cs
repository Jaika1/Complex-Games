using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WerewolfDataLib;
using WerewolfDataLib.Interfaces;

public class WerewolfRoleAlignment : IRoleAlignment
{
    public bool CheckWinCondition(WerewolfGameInfo gameInfo, WerewolfPlayer[] players)
    {
        return true;
    }
}

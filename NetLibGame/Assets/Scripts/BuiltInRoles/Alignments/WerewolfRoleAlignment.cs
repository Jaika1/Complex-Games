using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WerewolfDataLib;
using WerewolfDataLib.Interfaces;

public class WerewolfRoleAlignment : IRoleAlignment
{
    public System.Drawing.Color GroupColour => System.Drawing.Color.FromArgb(255, 0, 40);

    public bool CheckWinCondition(WerewolfGameInfo gameInfo, WerewolfPlayer[] players)
    {
        return true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WerewolfDataLib;
using WerewolfDataLib.Interfaces;

public class VillagerRoleAlignment : IRoleAlignment
{
    public System.Drawing.Color GroupColour => System.Drawing.Color.FromArgb(0, 255, 60);

    public bool CheckWinCondition(WerewolfGameInfo gameInfo, WerewolfPlayer[] players)
    {
        return true;
    }
}

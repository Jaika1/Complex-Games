using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WerewolfDataLib;
using WerewolfDataLib.Interfaces;

public class VillagerRoleAlignment : IRoleAlignment
{
    public string GroupName => "Villagers";

    public System.Drawing.Color GroupColour => System.Drawing.Color.FromArgb(0, 255, 60);

    public bool CheckWinCondition(WerewolfGameInfo gameInfo, WerewolfPlayer[] players)
    {
        if (players.Count(p => p.Role.Alignment.GetType() == typeof(WerewolfRoleAlignment)) == 0)
            return true;

        return false;
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WerewolfDataLib;
using WerewolfDataLib.Interfaces;

public class WerewolfRoleAlignment : IRoleAlignment
{
    public string GroupName => "Werewolves";

    public System.Drawing.Color GroupColour => System.Drawing.Color.FromArgb(255, 0, 40);

    public bool CheckWinCondition(WerewolfGameInfo gameInfo, WerewolfPlayer[] players)
    {
        int werewolves = players.Count(p => p.Role.Alignment.GetType() == typeof(WerewolfRoleAlignment));
        int halfPlayers = Mathf.CeilToInt(players.Count() / 2.0f);

        if (werewolves >= halfPlayers)
            return true;

        return false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WerewolfDataLib;
using WerewolfDataLib.Interfaces;

public class Werewolf : WerewolfRole
{
    private static WerewolfRoleAlignment aInstance = new WerewolfRoleAlignment();
    public override string Name => "Werewolf";
    public override string Description => "Work with the other werewolves to slay the town and gain a player majority.";

    public override IRoleAlignment Alignment => aInstance;

    public Werewolf()
    {
        NightEvent = new WerewolfNightEvent();
    }
}

public class WerewolfNightEvent : NightEvent
{
    public override uint EventPriority => 100;

    public override byte EventTargets => 1;

    public override WerewolfPlayer[] DoNightEvent(WerewolfGameInfo gameNfo)
    {
        if (TargetPlayers[0] == null)
            return new WerewolfPlayer[0];

        TargetPlayers[0].Status = PlayerStatus.Dead;
        return TargetPlayers;
    }
}

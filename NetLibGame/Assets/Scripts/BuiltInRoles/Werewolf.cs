using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WerewolfDataLib;

public class Werewolf : WerewolfRole
{
    public override string Name => "Werewolf";
    public override string Description => "Work with the other werewolves to slay the town and gain a player majority.";

    public Werewolf()
    {
        Alignment = new WerewolfRoleAlignment();
        NightEvent = new WerewolfNightEvent();
    }
}

public class WerewolfNightEvent : NightEvent
{
    public override uint EventPriority => 100;

    public override byte EventTargets => 1;

    public override WerewolfPlayer[] DoNightEvent(WerewolfGameInfo gameNfo)
    {
        return new WerewolfPlayer[0];
    }
}

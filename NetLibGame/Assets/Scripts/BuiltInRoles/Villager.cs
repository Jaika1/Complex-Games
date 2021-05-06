using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WerewolfDataLib;

public class Villager : WerewolfRole
{
    public override string Name => "Villager";

    public override string Description => "Work with the other villagers to eliminate the werewolves and any other threats to restore peace to the town.";

    public Villager()
    {
        Alignment = new VillagerRoleAlignment();
        NightEvent = null;
    }
}
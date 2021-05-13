using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WerewolfDataLib;
using WerewolfDataLib.Interfaces;

public class Villager : WerewolfRole
{
    private static VillagerRoleAlignment aInstance = new VillagerRoleAlignment();

    public override string Name => "Villager";

    public override string Description => "Work with the other villagers to eliminate the werewolves and any other threats to restore peace to the town.";

    public override IRoleAlignment Alignment => aInstance;


    public Villager()
    {
        NightEvent = null;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WerewolfDataLib;

public class Villager : WerewolfRole
{
    public override string RoleName => "Villager";

    public Villager()
    {
        RoleAlignment = new VillagerRoleAlignment();
        NightEvent = null;
    }
}
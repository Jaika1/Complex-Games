using System;
using System.Collections.Generic;
using System.Text;
using WerewolfDataLib;
using WerewolfDataLib.Interfaces;

namespace TestWerewolfRoles
{
    public class Villager : WerewolfRole
    {
        public override string RoleName => "Villager";

        public Villager()
        {
            RoleAlignment = null;
            NightEvent = null;
        }
    }
}

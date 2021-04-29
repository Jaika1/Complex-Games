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
            RoleAlignment = new VillagerRoleAlignment();
            NightEvent = null;
        }
    }

    public class VillagerRoleAlignment : IRoleAlignment
    {
        public bool CheckWinCondition(WerewolfGameInfo gameInfo, WerewolfPlayer[] players)
        {
            throw new NotImplementedException();
        }
    }
}

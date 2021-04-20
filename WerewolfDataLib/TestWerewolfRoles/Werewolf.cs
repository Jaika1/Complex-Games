using System;
using WerewolfDataLib;
using WerewolfDataLib.Interfaces;

namespace TestWerewolfRoles
{
    public class Werewolf : WerewolfRole
    {
        public override string RoleName => "Werewolf";


        public Werewolf()
        {
            RoleAlignment = null;
            NightEvent = new WerewolfNightEvent();
        }
    }

    public class WerewolfNightEvent : NightEvent
    {
        public override uint EventPriority => 100u;
        public override byte EventTargets => 1;

        public override WerewolfPlayer[] DoNightEvent(WerewolfGameInfo gameNfo)
        {
            Array.ForEach(TargetPlayers, p => p.AttackFrom(SourcePlayer));
            return TargetPlayers;
        }
    }
}

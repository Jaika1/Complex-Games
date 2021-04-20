using System;
using System.Collections.Generic;
using System.Text;
using WerewolfDataLib.Interfaces;

namespace WerewolfDataLib
{
    public class WerewolfPlayer
    {
        public uint PlayerID;
        public WerewolfRole Role;
        public PlayerStatus Status;

        public WerewolfPlayer()
        {
            Status = PlayerStatus.Spectating;
        }


        public virtual void AttackFrom(WerewolfPlayer attacker)
        {
            Status = PlayerStatus.Dead;
        }
    }

    public enum PlayerStatus
    {
        Spectating,
        Alive,
        Dead
    }
}

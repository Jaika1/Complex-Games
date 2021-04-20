using System;
using WerewolfDataLib.Interfaces;

namespace WerewolfDataLib
{
    public abstract class WerewolfRole
    {
        public abstract string RoleName { get; }
        public IRoleAlignment RoleAlignment;
        public NightEvent NightEvent;
    }
}

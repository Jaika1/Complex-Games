using System;
using System.Security.Cryptography;
using System.Text;
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

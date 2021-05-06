using System;
using System.Security.Cryptography;
using System.Text;
using WerewolfDataLib.Interfaces;

namespace WerewolfDataLib
{
    public abstract class WerewolfRole
    {
        public abstract string Name { get; }
        public abstract string Description { get; }

        public IRoleAlignment Alignment;
        public NightEvent NightEvent;
    }
}

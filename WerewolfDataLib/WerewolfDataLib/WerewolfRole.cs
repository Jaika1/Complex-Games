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

        public abstract IRoleAlignment Alignment { get; }
        public NightEvent NightEvent = null;
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Text;
using WerewolfDataLib;
using WerewolfDataLib.Interfaces;

namespace TestWerewolfRoles
{
    public class ModdedRole : WerewolfRole
    {
        public static CoolAlignment aInst = new CoolAlignment();

        public override string Name => "Fancy Boi";

        public override string Description => "This is a test role.";

        public override IRoleAlignment Alignment => aInst;
    }

    public class CoolAlignment : IRoleAlignment
    {
        public string GroupName => "The Cool Group";

        public Color GroupColour => Color.FromArgb(0, 100, 255);

        public bool CheckWinCondition(WerewolfGameInfo gameInfo, WerewolfPlayer[] players)
        {
            return true;
        }
    }
}

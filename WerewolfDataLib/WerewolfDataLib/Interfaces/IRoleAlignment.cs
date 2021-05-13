using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace WerewolfDataLib.Interfaces
{
    public interface IRoleAlignment
    {
        string GroupName { get; }

        Color GroupColour { get; } 

        bool CheckWinCondition(WerewolfGameInfo gameInfo, WerewolfPlayer[] players);
    }
}

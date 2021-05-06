using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace WerewolfDataLib.Interfaces
{
    public interface IRoleAlignment
    {
        Color GroupColour { get; } 

        bool CheckWinCondition(WerewolfGameInfo gameInfo, WerewolfPlayer[] players);
    }
}

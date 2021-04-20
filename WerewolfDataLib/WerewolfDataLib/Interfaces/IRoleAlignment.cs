using System;
using System.Collections.Generic;
using System.Text;

namespace WerewolfDataLib.Interfaces
{
    public interface IRoleAlignment
    {
        bool CheckWinCondition(WerewolfGameInfo gameInfo, WerewolfPlayer[] players);
    }
}

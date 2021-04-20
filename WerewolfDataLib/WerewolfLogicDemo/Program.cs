using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TestWerewolfRoles;
using WerewolfDataLib;
using WerewolfDataLib.Interfaces;

namespace WerewolfLogicDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Random rand = new Random();
            Assembly roleAssembly = Assembly.LoadFrom(@".\TestWerewolfRoles.dll");
            WerewolfGameInfo gameNfo = new WerewolfGameInfo(roleAssembly);

            List<Type> roleList = Enumerable.Repeat(gameNfo.RoleTypeFromName("Villager"), 24).ToList();
            roleList.AddRange(Enumerable.Repeat(gameNfo.RoleTypeFromName("Werewolf"), 6));

            List<WerewolfPlayer> playerList = new List<WerewolfPlayer>();
            for (int i = 0; i < 30; ++i)
                gameNfo.AddPlayerAndAssignId();

            gameNfo.AssignRolesAndSpectators(30, roleList);

            foreach (WerewolfPlayer p in gameNfo.Players)
                Console.WriteLine($"[{p.PlayerID}] {p.Role?.RoleName} ({p.Status})");

            Console.ReadKey();

            List<NightEvent> nightEvents = new List<NightEvent>();

            foreach (WerewolfPlayer p in gameNfo.Players)
            {
                if (p.Role.NightEvent != null)
                {
                    WerewolfPlayer[] targets = gameNfo.Players.Where(p => p.Status == PlayerStatus.Alive && p.Role.RoleName != "Werewolf").ToArray();

                    p.Role.NightEvent.SourcePlayer = p;
                    p.Role.NightEvent.TargetPlayers = new WerewolfPlayer[] { targets[rand.Next(targets.Length)] };

                    Console.WriteLine($"{p.PlayerID} attacks {p.Role.NightEvent.TargetPlayers[0].PlayerID}");

                    nightEvents.Add(p.Role.NightEvent);
                }
            }

            gameNfo.ResolveNightEvents(nightEvents);

            foreach (WerewolfPlayer p in gameNfo.Players)
                Console.WriteLine($"[{p.PlayerID}] {p.Role?.RoleName} ({p.Status})");

            Console.ReadKey();
        }
    }
}

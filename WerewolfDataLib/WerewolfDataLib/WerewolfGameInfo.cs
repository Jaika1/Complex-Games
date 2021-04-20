using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using WerewolfDataLib.Interfaces;

namespace WerewolfDataLib
{
    public class WerewolfGameInfo
    {
        private uint nextPid = 0u;
        private Random random = new Random();
        private Dictionary<string, Type> loadedRoleTypes = new Dictionary<string, Type>();
        private List<WerewolfPlayer> playerList = new List<WerewolfPlayer>();

        public IReadOnlyDictionary<string, Type> LoadedRoleTypes => loadedRoleTypes;
        public List<WerewolfPlayer> Players => playerList;


        public WerewolfGameInfo(params Assembly[] roleAssemblies)
        {
            List<Type> roleTypes = (from a in roleAssemblies
                                    from t in a.GetTypes()
                                    where t.BaseType == typeof(WerewolfRole)
                                    select t).ToList();

            foreach (Type t in roleTypes)
                loadedRoleTypes.Add((Activator.CreateInstance(t) as WerewolfRole).RoleName, t);
        }


        public WerewolfPlayer AddPlayerAndAssignId(WerewolfPlayer instance = null)
        {
            if (instance == null)
            {
                WerewolfPlayer player = new WerewolfPlayer();
                player.PlayerID = nextPid++;
                playerList.Add(player);
                return player;
            }

            instance.PlayerID = ++nextPid;
            playerList.Add(instance);
            return instance;
        }

        public Type RoleTypeFromName(string name)
        {
            return loadedRoleTypes.ContainsKey(name) ? loadedRoleTypes[name] : null;
        }

        public void AssignRolesAndSpectators(int numPlaying, List<Type> predeterminedRoleTypes, List<WerewolfPlayer> forcedSpectators = null)
        {
            List<WerewolfPlayer> nonForcedPlayers = forcedSpectators == null ? playerList : playerList.Where(p => !forcedSpectators.Contains(p)).ToList();
            List<WerewolfPlayer> playingGroup = new List<WerewolfPlayer>();
            List <WerewolfPlayer> spectatingGroup = new List<WerewolfPlayer>();

            if (forcedSpectators != null)
                spectatingGroup.AddRange(forcedSpectators);

            for (int i = 0; i < nonForcedPlayers.Count(); ++i)
            {
                int d = nonForcedPlayers.Count() - i;
                int n = numPlaying - playingGroup.Count;

                if (random.Next(d) + 1 <= n)
                    playingGroup.Add(nonForcedPlayers[i]);
                else
                    spectatingGroup.Add(nonForcedPlayers[i]);
            }

            spectatingGroup.ForEach(p => p.Status = PlayerStatus.Spectating);
            playingGroup.ForEach(p => p.Status = PlayerStatus.Alive);

            List<Type> predeterminedTypesClone = new List<Type>(predeterminedRoleTypes);

            for (int i = 0; i < playingGroup.Count; ++i)
            {
                int roleIndex = random.Next(predeterminedTypesClone.Count);
                Type playerRoleType = predeterminedTypesClone[roleIndex];
                predeterminedTypesClone.RemoveAt(roleIndex);

                playingGroup[i].Role = (WerewolfRole)Activator.CreateInstance(playerRoleType);
            }
        }

        public List<WerewolfPlayer> ResolveNightEvents(List<NightEvent> events)
        {
            List<WerewolfPlayer> eventPlayers = new List<WerewolfPlayer>();
            events.Sort((e1, e2) => e1.EventPriority.CompareTo(e2.EventPriority));

            events.ForEach(e => e?.DoNightEvent(this));

            return eventPlayers;
        }

        public (bool, IRoleAlignment[]) CheckIfWinConditionMet()
        {
            return (false, null);
        }
    }
}

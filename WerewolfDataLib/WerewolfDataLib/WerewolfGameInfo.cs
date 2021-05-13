using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using WerewolfDataLib.Interfaces;

namespace WerewolfDataLib
{
    public class WerewolfGameInfo
    {
        private static MD5 hasher = MD5.Create();
        private uint nextPid = 0u;
        private Random random = new Random();
        private Dictionary<string, Type> loadedRoleTypes = new Dictionary<string, Type>();
        private List<WerewolfPlayer> playerList = new List<WerewolfPlayer>();

        public IReadOnlyDictionary<string, Type> LoadedRoleTypes => loadedRoleTypes;
        public List<WerewolfPlayer> Players => playerList;


        public WerewolfGameInfo(Dictionary<string, Type> roles)
        {
            loadedRoleTypes = roles;
        }

        public static Dictionary<string, Type> LoadRolesFromAssemblies(params Assembly[] roleAssemblies)
        {
            Dictionary<string, Type> dict = new Dictionary<string, Type>();

            List<Type> roleTypes = (from a in roleAssemblies
                                    from t in a.GetTypes()
                                    where t.BaseType == typeof(WerewolfRole)
                                    select t).ToList();

            foreach (Type t in roleTypes)
                dict.Add((Activator.CreateInstance(t) as WerewolfRole).Name, t);

            return dict;
        }

        public static List<string> GenerateRoleHashes(Type[] roleTypes)
        {
            return (from roleType in roleTypes
                    let utf8Bytes = Encoding.UTF8.GetBytes(roleType.AssemblyQualifiedName)
                    select string.Join(string.Empty, hasher.ComputeHash(utf8Bytes).Select(x => x.ToString("X")))).ToList();
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
                if (playingGroup[i].Role.NightEvent != null)
                    playingGroup[i].Role.NightEvent.SourcePlayer = playingGroup[i];
            }
        }

        public List<WerewolfPlayer> ResolveNightEvents(/*List<NightEvent> events*/)
        {
            List<NightEvent> events = (from p in Players
                                       where p.Status == PlayerStatus.Alive
                                       where p.Role.NightEvent != null
                                       select p.Role.NightEvent).ToList();

            List<WerewolfPlayer> eventPlayers = new List<WerewolfPlayer>();
            events.Sort((e1, e2) => e1.EventPriority.CompareTo(e2.EventPriority));

            events.ForEach(e => eventPlayers.AddRange(e?.DoNightEvent(this)));

            return eventPlayers;
        }

        public (bool, IRoleAlignment[]) CheckIfWinConditionMet()
        {
            List<IRoleAlignment> winAlignments = new List<IRoleAlignment>();

            foreach (WerewolfPlayer p in Players)
            {
                if (p.Status == PlayerStatus.Alive && !winAlignments.Contains(p.Role.Alignment))
                {
                    if (p.Role.Alignment.CheckWinCondition(this, Players.Where(wp => wp.Status == PlayerStatus.Alive).ToArray()))
                        winAlignments.Add(p.Role.Alignment);
                }
            }

            if (winAlignments.Count == 0)
                return (false, null);

            return (true, winAlignments.ToArray());
        }
    }
}

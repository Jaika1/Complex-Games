namespace WerewolfDataLib
{
    public abstract class NightEvent
    {
        public abstract uint EventPriority { get; }
        public WerewolfPlayer SourcePlayer = null;
        public abstract byte EventTargets { get; }
        public WerewolfPlayer[] TargetPlayers = new WerewolfPlayer[0];

        public abstract WerewolfPlayer[] DoNightEvent(WerewolfGameInfo gameNfo);
    }
}
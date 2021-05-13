namespace WerewolfDataLib
{
    public abstract class NightEvent
    {
        public NightEvent()
            => TargetPlayers = new WerewolfPlayer[EventTargets];

        internal WerewolfPlayer SourcePlayer = null;
        public WerewolfPlayer[] TargetPlayers;

        public abstract uint EventPriority { get; }
        public abstract byte EventTargets { get; }

        public abstract WerewolfPlayer[] DoNightEvent(WerewolfGameInfo gameNfo);
    }
}
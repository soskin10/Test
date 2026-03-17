namespace Project.Scripts.Services.EventBusSystem
{
    namespace Events
    {
        public readonly struct MatchPlayedEvent
        {
            public int CascadeIndex { get; }
            
            
            public MatchPlayedEvent(int cascadeIndex)
            {
                CascadeIndex = cascadeIndex;
            }
        }

        public readonly struct BombActivatedEvent
        {
            
        }
    }
}
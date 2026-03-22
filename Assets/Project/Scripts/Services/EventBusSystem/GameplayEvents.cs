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

        public readonly struct DamageDealtEvent
        {
            public int Total { get; }


            public DamageDealtEvent(int total)
            {
                Total = total;
            }
        }

        public readonly struct MoveUsedEvent
        {

        }

        public readonly struct MoveCountChangedEvent
        {
            public int Remaining { get; }


            public MoveCountChangedEvent(int remaining)
            {
                Remaining = remaining;
            }
        }

        public readonly struct OutOfMovesEvent
        {

        }

        public readonly struct EnemyHPChangedEvent
        {
            public int Current { get; }
            public int Max { get; }


            public EnemyHPChangedEvent(int current, int max)
            {
                Current = current;
                Max = max;
            }
        }

        public readonly struct EnemyDefeatedEvent
        {

        }
    }
}
using System.Collections.Generic;
using Project.Scripts.Shared.Tiles;

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
            public int MovesUsed { get; }


            public MoveCountChangedEvent(int movesUsed)
            {
                MovesUsed = movesUsed;
            }
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

        public readonly struct EnergyGeneratedEvent
        {
            public IReadOnlyDictionary<TileKind, int> EnergyByKind { get; }


            public EnergyGeneratedEvent(IReadOnlyDictionary<TileKind, int> energyByKind)
            {
                EnergyByKind = energyByKind;
            }
        }

        public readonly struct EnergyChangedEvent
        {
            public TileKind Kind { get; }
            public int Current { get; }
            public int Max { get; }


            public EnergyChangedEvent(TileKind kind, int current, int max)
            {
                Kind = kind;
                Current = current;
                Max = max;
            }
        }

        public readonly struct PlayerHPChangedEvent
        {
            public int Current { get; }
            public int Max { get; }


            public PlayerHPChangedEvent(int current, int max)
            {
                Current = current;
                Max = max;
            }
        }

        public readonly struct SwapRejectedEvent
        {

        }

        public readonly struct MoveBarChangedEvent
        {
            public int CurrentMoves { get; }
            public float FillProgress { get; }
            public bool IsAtMax { get; }


            public MoveBarChangedEvent(int currentMoves, float fillProgress, bool isAtMax)
            {
                CurrentMoves = currentMoves;
                FillProgress = fillProgress;
                IsAtMax = isAtMax;
            }
        }
    }
}
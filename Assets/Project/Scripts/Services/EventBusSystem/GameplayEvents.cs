using System.Collections.Generic;
using Project.Scripts.Shared.Damage;
using Project.Scripts.Shared.Heroes;
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

        public readonly struct CascadeCompletedEvent
        {
            public DamageBreakdown Breakdown { get; }
            public int Total => Breakdown.Total;


            public CascadeCompletedEvent(DamageBreakdown breakdown)
            {
                Breakdown = breakdown;
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

        public readonly struct HeroHPChangedEvent
        {
            public BattleSide Side      { get; }
            public int        SlotIndex { get; }
            public int        Current   { get; }
            public int        Max       { get; }


            public HeroHPChangedEvent(BattleSide side, int slotIndex, int current, int max)
            {
                Side      = side;
                SlotIndex = slotIndex;
                Current   = current;
                Max       = max;
            }
        }

        public readonly struct HeroDefeatedEvent
        {
            public BattleSide Side      { get; }
            public int        SlotIndex { get; }


            public HeroDefeatedEvent(BattleSide side, int slotIndex)
            {
                Side      = side;
                SlotIndex = slotIndex;
            }
        }

        public readonly struct HeroEnergyChangedEvent
        {
            public BattleSide Side { get; }
            public int SlotIndex { get; }
            public int Current { get; }
            public int Max { get; }


            public HeroEnergyChangedEvent(BattleSide side, int slotIndex, int current, int max)
            {
                Side = side;
                SlotIndex = slotIndex;
                Current = current;
                Max = max;
            }
        }

        public readonly struct HeroActivatedEvent
        {
            public BattleSide Side { get; }
            public int SlotIndex { get; }
            public HeroActionType ActionType { get; }
            public int ActionValue { get; }


            public HeroActivatedEvent(BattleSide side, int slotIndex, HeroActionType actionType, int actionValue)
            {
                Side = side;
                SlotIndex = slotIndex;
                ActionType = actionType;
                ActionValue = actionValue;
            }
        }

        public readonly struct EnemyAttackEvent
        {
            public int Damage { get; }


            public EnemyAttackEvent(int damage)
            {
                Damage = damage;
            }
        }

        public readonly struct PlayerDefeatedEvent
        {

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

        public readonly struct PlayerAvatarEnergyChangedEvent
        {
            public int Current { get; }
            public int Max { get; }


            public PlayerAvatarEnergyChangedEvent(int current, int max)
            {
                Current = current;
                Max = max;
            }
        }

        public readonly struct EnemyAvatarEnergyChangedEvent
        {
            public int Current { get; }
            public int Max { get; }


            public EnemyAvatarEnergyChangedEvent(int current, int max)
            {
                Current = current;
                Max = max;
            }
        }

        public readonly struct EnemyAvatarAttackedEvent
        {
            public int DamageAmount { get; }


            public EnemyAvatarAttackedEvent(int damageAmount)
            {
                DamageAmount = damageAmount;
            }
        }

        public readonly struct AbilityExecutedEvent
        {
            public UnitDescriptor Source { get; }
            public UnitDescriptor Target { get; }
            public HeroActionType ActionType { get; }
            public int Value { get; }


            public AbilityExecutedEvent(UnitDescriptor source, UnitDescriptor target, HeroActionType actionType, int value)
            {
                Source = source;
                Target = target;
                ActionType = actionType;
                Value = value;
            }
        }
    }
}
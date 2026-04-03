using System;
using System.Collections.Generic;
using Project.Scripts.Configs;
using Project.Scripts.Services.EventBusSystem;
using Project.Scripts.Services.EventBusSystem.Events;
using Project.Scripts.Shared.Heroes;
using Project.Scripts.Shared.Tiles;

namespace Project.Scripts.Services.Combat
{
    public class HeroService : IHeroService, IDisposable
    {
        private const int SlotCount = 4;


        private readonly EventBus _eventBus;
        private readonly IPlayerStateService _playerState;
        private readonly IEnemyStateService _enemyState;
        private readonly HeroSlotState[] _playerSlots = new HeroSlotState[SlotCount];
        private readonly HeroSlotState[] _enemySlots = new HeroSlotState[SlotCount];
        private IDisposable _subscription;


        public HeroService(EventBus eventBus, LevelConfig levelConfig, IPlayerStateService playerState, IEnemyStateService enemyState)
        {
            _eventBus = eventBus;
            _playerState = playerState;
            _enemyState = enemyState;

            InitSlots(_playerSlots, levelConfig.PlayerHeroes);
            InitSlots(_enemySlots, levelConfig.EnemyHeroes);

            PublishInitialHPEvents(_playerSlots, BattleSide.Player);
            PublishInitialHPEvents(_enemySlots, BattleSide.Enemy);

            _subscription = _eventBus.Subscribe<EnergyGeneratedEvent>(OnEnergyGenerated);
        }


        public IReadOnlyList<HeroSlotState> GetSlots(BattleSide side) =>
            side == BattleSide.Player ? _playerSlots : _enemySlots;

        public void TryActivate(int slotIndex)
        {
            TryActivate(BattleSide.Player, slotIndex);
        }

        public void TryActivate(BattleSide side, int slotIndex)
        {
            if (slotIndex is < 0 or >= SlotCount)
                return;

            if (side == BattleSide.Player)
                TryActivateSlot(ref _playerSlots[slotIndex], BattleSide.Player, slotIndex);
            else
                TryActivateSlot(ref _enemySlots[slotIndex], BattleSide.Enemy, slotIndex);
        }

        public void AssignEnemyHeroes(HeroConfig[] heroes)
        {
            InitSlots(_enemySlots, heroes);

            for (var i = 0; i < SlotCount; i++)
            {
                _eventBus.Publish(new HeroEnergyChangedEvent(
                    BattleSide.Enemy, i, _enemySlots[i].CurrentEnergy, _enemySlots[i].MaxEnergy));
                _eventBus.Publish(new HeroHPChangedEvent(
                    BattleSide.Enemy, i, _enemySlots[i].CurrentHP, _enemySlots[i].MaxHP));
            }
        }

        public void AddEnemyHeroEnergy(int slotIndex, int amount)
        {
            if (slotIndex is < 0 or >= SlotCount)
                return;

            ref var slot = ref _enemySlots[slotIndex];

            if (false == slot.IsAssigned || false == slot.IsAlive || amount <= 0)
                return;

            slot.CurrentEnergy = Math.Min(slot.MaxEnergy, slot.CurrentEnergy + amount);
            _eventBus.Publish(new HeroEnergyChangedEvent(
                BattleSide.Enemy, slotIndex, slot.CurrentEnergy, slot.MaxEnergy));
        }

        public void ApplyDamageToHero(BattleSide side, int slotIndex, int amount)
        {
            if (slotIndex is < 0 or >= SlotCount || amount <= 0)
                return;

            ref var slot = ref GetSlotRef(side, slotIndex);
            ApplyHPChange(ref slot, side, slotIndex, -amount);
        }

        public void ApplyHealToHero(BattleSide side, int slotIndex, int amount)
        {
            if (slotIndex is < 0 or >= SlotCount || amount <= 0)
                return;

            ref var slot = ref GetSlotRef(side, slotIndex);
            ApplyHPChange(ref slot, side, slotIndex, +amount);
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }


        private bool IsHpFull(BattleSide side)
        {
            if (side == BattleSide.Player)
                return _playerState.CurrentHP >= _playerState.MaxHP;

            return _enemyState.CurrentHP >= _enemyState.MaxHP;
        }

        private void TryActivateSlot(ref HeroSlotState slot, BattleSide side, int slotIndex)
        {
            if (false == slot.IsReady)
                return;

            if (slot.ActionType == HeroActionType.HealAlly && IsHpFull(side))
                return;

            _eventBus.Publish(new HeroActivatedEvent(side, slotIndex, slot.ActionType, slot.ActionValue));

            slot.CurrentEnergy = 0;
            _eventBus.Publish(new HeroEnergyChangedEvent(side, slotIndex, 0, slot.MaxEnergy));
        }

        private void ApplyHPChange(ref HeroSlotState slot, BattleSide side, int slotIndex, int delta)
        {
            if (false == slot.IsAssigned || slot.MaxHP <= 0)
                return;

            if (false == slot.IsAlive)
                return;

            slot.CurrentHP = Math.Clamp(slot.CurrentHP + delta, 0, slot.MaxHP);
            _eventBus.Publish(new HeroHPChangedEvent(side, slotIndex, slot.CurrentHP, slot.MaxHP));

            if (slot.CurrentHP > 0)
                return;

            slot.CurrentEnergy = 0;
            _eventBus.Publish(new HeroEnergyChangedEvent(side, slotIndex, 0, slot.MaxEnergy));
            _eventBus.Publish(new HeroDefeatedEvent(side, slotIndex));
        }

        private ref HeroSlotState GetSlotRef(BattleSide side, int slotIndex)
        {
            var slots = side == BattleSide.Player ? _playerSlots : _enemySlots;
            return ref slots[slotIndex];
        }

        private void OnEnergyGenerated(EnergyGeneratedEvent e)
        {
            foreach (var pair in e.EnergyByKind)
                DistributeToSlots(_playerSlots, BattleSide.Player, pair.Key, pair.Value);
        }

        private void DistributeToSlots(HeroSlotState[] slots, BattleSide side, TileKind kind, int amount)
        {
            for (var i = 0; i < slots.Length; i++)
            {
                ref var slot = ref slots[i];

                if (false == slot.IsAssigned || false == slot.IsAlive || slot.Kind != kind)
                    continue;

                slot.CurrentEnergy = Math.Min(slot.MaxEnergy, slot.CurrentEnergy + amount);
                _eventBus.Publish(new HeroEnergyChangedEvent(side, i, slot.CurrentEnergy, slot.MaxEnergy));
            }
        }

        private void PublishInitialHPEvents(HeroSlotState[] slots, BattleSide side)
        {
            for (var i = 0; i < slots.Length; i++)
            {
                if (false == slots[i].IsAssigned)
                    continue;

                _eventBus.Publish(new HeroHPChangedEvent(side, i, slots[i].CurrentHP, slots[i].MaxHP));
            }
        }

        private static void InitSlots(HeroSlotState[] slots, HeroConfig[] configs)
        {
            for (var i = 0; i < SlotCount; i++)
            {
                var config = (configs != null && i < configs.Length) ? configs[i] : null;
                if (!config)
                {
                    slots[i] = default;
                    continue;
                }

                slots[i] = new HeroSlotState
                {
                    IsAssigned = true,
                    Kind = config.Kind,
                    CurrentEnergy = 0,
                    MaxEnergy = config.MaxEnergy,
                    ActionType = config.ActionType,
                    ActionValue = config.ActionValue,
                    CurrentHP = config.MaxHP,
                    MaxHP = config.MaxHP,
                };
            }
        }
    }
}
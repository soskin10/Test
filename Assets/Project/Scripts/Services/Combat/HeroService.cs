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
        private readonly HeroSlotState[] _playerSlots = new HeroSlotState[SlotCount];
        private readonly HeroSlotState[] _enemySlots = new HeroSlotState[SlotCount];
        private IDisposable _subscription;


        public HeroService(EventBus eventBus, LevelConfig levelConfig)
        {
            _eventBus = eventBus;

            InitSlots(_playerSlots, levelConfig.PlayerHeroes);
            InitSlots(_enemySlots, levelConfig.EnemyHeroes);

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
                _eventBus.Publish(new HeroEnergyChangedEvent(
                    BattleSide.Enemy, i, _enemySlots[i].CurrentEnergy, _enemySlots[i].MaxEnergy));
        }

        public void AddEnemyHeroEnergy(int slotIndex, int amount)
        {
            if (slotIndex is < 0 or >= SlotCount)
                return;

            ref var slot = ref _enemySlots[slotIndex];

            if (false == slot.IsAssigned || amount <= 0)
                return;

            slot.CurrentEnergy = Math.Min(slot.MaxEnergy, slot.CurrentEnergy + amount);
            _eventBus.Publish(new HeroEnergyChangedEvent(
                BattleSide.Enemy, slotIndex, slot.CurrentEnergy, slot.MaxEnergy));
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }


        private void TryActivateSlot(ref HeroSlotState slot, BattleSide side, int slotIndex)
        {
            if (false == slot.IsReady)
                return;

            _eventBus.Publish(new HeroActivatedEvent(side, slotIndex, slot.ActionType, slot.ActionValue));

            slot.CurrentEnergy = 0;
            _eventBus.Publish(new HeroEnergyChangedEvent(side, slotIndex, 0, slot.MaxEnergy));
        }

        private void OnEnergyGenerated(EnergyGeneratedEvent e)
        {
            foreach (var pair in e.EnergyByKind)
                DistributeToSlots(_playerSlots, BattleSide.Player, pair.Key, pair.Value);
        }

        private void DistributeToSlots(HeroSlotState[] slots, BattleSide side, TileKind kind, int amount)
        {
            HeroEnergyDistributor.Distribute(slots, kind, amount);

            for (var i = 0; i < slots.Length; i++)
            {
                if (false == slots[i].IsAssigned || slots[i].Kind != kind)
                    continue;

                _eventBus.Publish(new HeroEnergyChangedEvent(side, i, slots[i].CurrentEnergy, slots[i].MaxEnergy));
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
                    ActionValue = config.ActionValue
                };
            }
        }
    }
}
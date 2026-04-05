using System;
using System.Collections.Generic;
using Project.Scripts.Configs;
using Project.Scripts.Configs.Battle;
using Project.Scripts.Services.EventBusSystem;
using Project.Scripts.Services.EventBusSystem.Events;
using Project.Scripts.Shared.Tiles;

namespace Project.Scripts.Services.Combat
{
    public class EnergyService : IEnergyService, IDisposable
    {
        private readonly HeroEnergyConfig _config;
        private readonly EventBus _eventBus;
        private readonly Dictionary<TileKind, int> _energy;
        private IDisposable _subscription;


        public IReadOnlyDictionary<TileKind, int> CurrentEnergy => _energy;
        public int MaxEnergy => _config.MaxEnergyPerType;


        public EnergyService(HeroEnergyConfig config, EventBus eventBus)
        {
            _config = config;
            _eventBus = eventBus;
            _energy = new Dictionary<TileKind, int>
            {
                { TileKind.Fire, 0 },
                { TileKind.Water, 0 },
                { TileKind.Nature, 0 },
                { TileKind.Light, 0 },
                { TileKind.Void, 0 }
            };

            _subscription = _eventBus.Subscribe<EnergyGeneratedEvent>(OnEnergyGenerated);
        }


        private void OnEnergyGenerated(EnergyGeneratedEvent e)
        {
            foreach (var pair in e.EnergyByKind)
            {
                if (false == _energy.TryGetValue(pair.Key, out var value))
                    continue;

                var newValue = Math.Min(value + pair.Value, _config.MaxEnergyPerType);
                _energy[pair.Key] = newValue;
                _eventBus.Publish(new EnergyChangedEvent(pair.Key, newValue, _config.MaxEnergyPerType));
            }
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }
    }
}
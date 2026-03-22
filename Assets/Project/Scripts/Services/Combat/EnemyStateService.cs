using System;
using Project.Scripts.Configs;
using Project.Scripts.Services.EventBusSystem;
using Project.Scripts.Services.EventBusSystem.Events;

namespace Project.Scripts.Services.Combat
{
    public class EnemyStateService : IEnemyStateService, IDisposable
    {
        public int CurrentHP { get; private set; }
        public int MaxHP { get; }


        private readonly EventBus _eventBus;
        private IDisposable _subscription;


        public EnemyStateService(EventBus eventBus, LevelConfig levelConfig)
        {
            _eventBus = eventBus;
            MaxHP = levelConfig.EnemyHP;
            CurrentHP = levelConfig.EnemyHP;
            _subscription = _eventBus.Subscribe<DamageDealtEvent>(OnDamageDealt);
        }


        private void OnDamageDealt(DamageDealtEvent e)
        {
            if (CurrentHP <= 0)
                return;

            CurrentHP = Math.Max(0, CurrentHP - e.Total);
            _eventBus.Publish(new EnemyHPChangedEvent(CurrentHP, MaxHP));

            if (CurrentHP == 0)
                _eventBus.Publish(new EnemyDefeatedEvent());
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }
    }
}

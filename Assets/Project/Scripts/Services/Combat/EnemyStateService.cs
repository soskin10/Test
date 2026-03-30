using System;
using Project.Scripts.Configs;
using Project.Scripts.Services.EventBusSystem;
using Project.Scripts.Services.EventBusSystem.Events;
using Project.Scripts.Shared.Heroes;
using R3;

namespace Project.Scripts.Services.Combat
{
    public class EnemyStateService : IEnemyStateService, IDisposable
    {
        public int CurrentHP { get; private set; }
        public int MaxHP { get; }


        private readonly EventBus _eventBus;
        private readonly CompositeDisposable _subscriptions = new();


        public EnemyStateService(EventBus eventBus, LevelConfig levelConfig)
        {
            _eventBus = eventBus;
            MaxHP = levelConfig.EnemyHP;
            CurrentHP = levelConfig.EnemyHP;

            _subscriptions.Add(_eventBus.Subscribe<DamageDealtEvent>(OnDamageDealt));
            _subscriptions.Add(_eventBus.Subscribe<HeroActivatedEvent>(OnHeroActivated));
        }


        public void Dispose()
        {
            _subscriptions.Dispose();
        }


        private void OnDamageDealt(DamageDealtEvent e)
        {
            ApplyDamage(e.Total);
        }

        private void OnHeroActivated(HeroActivatedEvent e)
        {
            if (e.Side == BattleSide.Player && e.ActionType == HeroActionType.DealDamage)
                ApplyDamage(e.ActionValue);
            else if (e.Side == BattleSide.Enemy && e.ActionType == HeroActionType.HealAlly)
                ApplyHeal(e.ActionValue);
        }

        private void ApplyDamage(int amount)
        {
            if (CurrentHP <= 0)
                return;

            CurrentHP = Math.Max(0, CurrentHP - amount);
            _eventBus.Publish(new EnemyHPChangedEvent(CurrentHP, MaxHP));

            if (CurrentHP == 0)
                _eventBus.Publish(new EnemyDefeatedEvent());
        }

        private void ApplyHeal(int amount)
        {
            if (CurrentHP >= MaxHP || amount <= 0)
                return;

            CurrentHP = Math.Min(MaxHP, CurrentHP + amount);
            _eventBus.Publish(new EnemyHPChangedEvent(CurrentHP, MaxHP));
        }
    }
}
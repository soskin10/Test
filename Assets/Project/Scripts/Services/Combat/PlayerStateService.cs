using System;
using Project.Scripts.Configs;
using Project.Scripts.Configs.Levels;
using Project.Scripts.Services.EventBusSystem;
using Project.Scripts.Services.EventBusSystem.Events;
using Project.Scripts.Shared.Heroes;
using R3;

namespace Project.Scripts.Services.Combat
{
    public class PlayerStateService : IPlayerStateService, IDisposable
    {
        public int CurrentHP { get; private set; }
        public int MaxHP { get; }


        private readonly EventBus _eventBus;
        private readonly CompositeDisposable _subscriptions = new();


        public PlayerStateService(EventBus eventBus, LevelConfig levelConfig)
        {
            _eventBus = eventBus;
            MaxHP = levelConfig.PlayerHP;
            CurrentHP = levelConfig.PlayerHP;

            _subscriptions.Add(_eventBus.Subscribe<EnemyAvatarAttackedEvent>(OnEnemyAvatarAttacked));
            _subscriptions.Add(_eventBus.Subscribe<HeroActivatedEvent>(OnHeroActivated));
        }


        public void Heal(int amount)
        {
            if (CurrentHP >= MaxHP || amount <= 0)
                return;

            CurrentHP = Math.Min(MaxHP, CurrentHP + amount);
            _eventBus.Publish(new PlayerHPChangedEvent(CurrentHP, MaxHP));
        }

        public void TakeDamage(int amount)
        {
            if (CurrentHP <= 0 || amount <= 0)
                return;

            CurrentHP = Math.Max(0, CurrentHP - amount);
            _eventBus.Publish(new PlayerHPChangedEvent(CurrentHP, MaxHP));

            if (CurrentHP == 0)
                _eventBus.Publish(new PlayerDefeatedEvent());
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }


        private void OnEnemyAvatarAttacked(EnemyAvatarAttackedEvent e)
        {
            TakeDamage(e.DamageAmount);
        }

        private void OnHeroActivated(HeroActivatedEvent e)
        {
            if (e.ActionType == HeroActionType.DealDamage && e.Side == BattleSide.Enemy)
                TakeDamage(e.ActionValue);
        }
    }
}
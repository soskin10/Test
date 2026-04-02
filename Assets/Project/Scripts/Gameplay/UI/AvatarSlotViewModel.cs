using System;
using Project.Scripts.Configs;
using Project.Scripts.Services.EventBusSystem;
using Project.Scripts.Services.EventBusSystem.Events;
using Project.Scripts.Shared.Heroes;
using R3;
using UnityEngine;

namespace Project.Scripts.Gameplay.UI
{
    public class AvatarSlotViewModel : IDisposable
    {
        public BattleSide Side { get; }
        public Sprite Portrait { get; }
        public BattleAnimationConfig AnimConfig { get; }
        public EventBus EventBus { get; }
        public ReactiveProperty<float> HPFill { get; }
        public Observable<int> Hit => _hit;
        public AvatarChargeBarViewModel EnergyBar { get; }


        private readonly Subject<int> _hit = new();
        private readonly CompositeDisposable _subscriptions = new();
        private int _prevHP;


        public AvatarSlotViewModel(EventBus eventBus, BattleSide side, Sprite portrait, 
            int initialHP, int maxHP, BattleAnimationConfig animConfig)
        {
            Side = side;
            Portrait = portrait;
            AnimConfig = animConfig;
            EventBus = eventBus;
            _prevHP = initialHP;
            HPFill = new ReactiveProperty<float>(maxHP > 0 ? (float)initialHP / maxHP : 1f);
            EnergyBar = new AvatarChargeBarViewModel(eventBus, side);

            if (side == BattleSide.Player)
                _subscriptions.Add(eventBus.Subscribe<PlayerHPChangedEvent>(OnPlayerHPChanged));
            else
                _subscriptions.Add(eventBus.Subscribe<EnemyHPChangedEvent>(OnEnemyHPChanged));
        }

        public void Dispose()
        {
            HPFill.Dispose();
            _hit.Dispose();
            EnergyBar.Dispose();
            _subscriptions.Dispose();
        }


        private void OnPlayerHPChanged(PlayerHPChangedEvent e) => ApplyHPChanged(e.Current, e.Max);

        private void OnEnemyHPChanged(EnemyHPChangedEvent e) => ApplyHPChanged(e.Current, e.Max);

        private void ApplyHPChanged(int current, int max)
        {
            if (current < _prevHP)
                _hit.OnNext(_prevHP - current);
            
            _prevHP = current;
            HPFill.Value = max > 0 ? (float)current / max : 0f;
        }
    }
}
using System;
using Project.Scripts.Configs;
using Project.Scripts.Services.EventBusSystem;
using Project.Scripts.Services.EventBusSystem.Events;
using Project.Scripts.Shared.Avatar;
using R3;
using UnityEngine;

namespace Project.Scripts.Services.Combat
{
    public class PlayerAvatarChargeService : IPlayerAvatarChargeService, IDisposable
    {
        public int CurrentCharge => _engine.Snapshot.CurrentCharge;
        public int MaxCharge => _engine.Snapshot.MaxCharge;
        public bool IsFull => _engine.Snapshot.IsFull;
        
        
        private readonly EventBus _eventBus;
        private readonly AvatarChargeEngine _engine = new AvatarChargeEngine();
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();


        public PlayerAvatarChargeService(EventBus eventBus, DamageConfig damageConfig)
        {
            _eventBus = eventBus;
            _engine.Initialize(damageConfig.MaxAvatarCharge);

            _subscriptions.Add(_eventBus.Subscribe<DamageDealtEvent>(OnDamageDealt));
            _subscriptions.Add(_eventBus.Subscribe<PlayerAvatarActivatedEvent>(OnPlayerAvatarActivated));
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }


        private void OnDamageDealt(DamageDealtEvent e)
        {
            var before = _engine.Snapshot;
            var added = _engine.TryAddCharge(e.Total);

            if (added <= 0)
            {
                Debug.Log($"[PlayerCharge] Bar full ({before.CurrentCharge}/{before.MaxCharge}) — {e.Total} dmg blocked until discharge");
                return;
            }

            var after = _engine.Snapshot;
            Debug.Log(e.Breakdown.ToLogString());
            Debug.Log($"[PlayerCharge] {after.CurrentCharge}/{after.MaxCharge}{(after.IsFull ? " — READY TO DISCHARGE" : string.Empty)}");
            PublishChargeChanged();
        }

        private void OnPlayerAvatarActivated(PlayerAvatarActivatedEvent _)
        {
            var discharged = _engine.TryDischarge();
            if (discharged <= 0)
                return;

            PublishChargeChanged();
            _eventBus.Publish(new PlayerDischargeEvent(discharged));
        }

        private void PublishChargeChanged()
        {
            var snap = _engine.Snapshot;
            _eventBus.Publish(new PlayerChargeChangedEvent(snap.CurrentCharge, snap.MaxCharge));
        }
    }
}
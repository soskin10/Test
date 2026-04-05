using System;
using Project.Scripts.Configs;
using Project.Scripts.Configs.Battle;
using Project.Scripts.Services.EventBusSystem;
using Project.Scripts.Services.EventBusSystem.Events;
using Project.Scripts.Shared.Avatar;
using R3;
using UnityEngine;
using VContainer.Unity;

namespace Project.Scripts.Services.Combat
{
    public class PlayerAvatarChargeService : IPlayerAvatarChargeService, IStartable, IDisposable
    {
        public int CurrentEnergy => _engine.Snapshot.CurrentEnergy;
        public int MaxEnergy => _engine.Snapshot.MaxEnergy;
        public bool IsReady => _engine.Snapshot.IsReady;


        private readonly EventBus _eventBus;
        private readonly AvatarEnergyEngine _engine = new AvatarEnergyEngine();
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();


        public PlayerAvatarChargeService(EventBus eventBus, HeroEnergyConfig heroEnergyConfig)
        {
            _eventBus = eventBus;
            _engine.Initialize(heroEnergyConfig.MaxAvatarCharge);
        }

        public void Start()
        {
            _subscriptions.Add(_eventBus.Subscribe<CascadeCompletedEvent>(OnCascadeCompleted));
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }


        public int TryRelease()
        {
            var released = _engine.TryRelease();
            if (released <= 0)
                return 0;

            PublishEnergyChanged();
            return released;
        }


        private void OnCascadeCompleted(CascadeCompletedEvent e)
        {
            var before = _engine.Snapshot;
            var added = _engine.TryAddEnergy(e.Total);
            if (added <= 0)
            {
                Debug.Log($"[PlayerAvatar] Bar full ({before.CurrentEnergy}/{before.MaxEnergy}) - {e.Total} energy blocked until release");
                return;
            }

            var after = _engine.Snapshot;
            Debug.Log(e.Breakdown.ToLogString());
            Debug.Log($"[PlayerAvatar] {after.CurrentEnergy}/{after.MaxEnergy}{(after.IsReady ? " - ready to attack" : string.Empty)}");
            PublishEnergyChanged();
        }

        private void PublishEnergyChanged()
        {
            var snap = _engine.Snapshot;
            _eventBus.Publish(new PlayerAvatarEnergyChangedEvent(snap.CurrentEnergy, snap.MaxEnergy));
        }
    }
}
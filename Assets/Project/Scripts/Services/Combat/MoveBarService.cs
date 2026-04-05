using System;
using Project.Scripts.Configs;
using Project.Scripts.Configs.UI;
using Project.Scripts.Services.EventBusSystem;
using Project.Scripts.Services.EventBusSystem.Events;
using Project.Scripts.Shared.Moves;

namespace Project.Scripts.Services.Combat
{
    public class MoveBarService : IMoveBarService, IDisposable
    {
        private readonly MoveBarConfig _config;
        private readonly EventBus _eventBus;
        private readonly MoveBarEngine _engine;


        public bool HasMoves => _engine.Snapshot.CurrentMoves > 0;


        public MoveBarService(MoveBarConfig config, EventBus eventBus)
        {
            _config = config;
            _eventBus = eventBus;
            _engine = new MoveBarEngine();
        }


        public MoveBarSnapshot GetSnapshot() => _engine.Snapshot;

        public void Initialize()
        {
            _engine.Initialize(_config.ToSettings());
            PublishSnapshot();
        }

        public void Tick(float deltaTime)
        {
            _engine.Tick(deltaTime);
            PublishSnapshot();
        }

        public bool TryConsume()
        {
            if (false == _engine.TryConsume())
                return false;

            PublishSnapshot();
            return true;
        }

        public void Dispose() { }


        private void PublishSnapshot()
        {
            var snapshot = _engine.Snapshot;
            _eventBus.Publish(new MoveBarChangedEvent(snapshot.CurrentMoves, snapshot.FillProgress, snapshot.IsAtMax));
        }
    }
}
using System;
using Project.Scripts.Configs;
using Project.Scripts.Services.EventBusSystem;
using Project.Scripts.Services.EventBusSystem.Events;

namespace Project.Scripts.Services.Combat
{
    public class MoveCounterService : IMoveCounterService, IDisposable
    {
        public int RemainingMoves { get; private set; }


        private readonly EventBus _eventBus;
        private IDisposable _subscription;


        public MoveCounterService(EventBus eventBus, LevelConfig levelConfig)
        {
            _eventBus = eventBus;
            RemainingMoves = levelConfig.MoveLimit;
            _subscription = _eventBus.Subscribe<MoveUsedEvent>(OnMoveUsed);
        }


        private void OnMoveUsed(MoveUsedEvent _)
        {
            if (RemainingMoves <= 0)
                return;

            RemainingMoves--;
            _eventBus.Publish(new MoveCountChangedEvent(RemainingMoves));

            if (RemainingMoves == 0)
                _eventBus.Publish(new OutOfMovesEvent());
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }
    }
}

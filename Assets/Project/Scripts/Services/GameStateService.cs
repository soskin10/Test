using System;
using Project.Scripts.Services.EventBusSystem;
using Project.Scripts.Services.EventBusSystem.Events;
using R3;

namespace Project.Scripts.Services
{
    public class GameStateService : IGameStateService, IDisposable
    {
        public ReadOnlyReactiveProperty<GameState> State => _state;
        public bool IsPlaying => _state.Value == GameState.Playing;


        private readonly EventBus _eventBus;
        private readonly ReactiveProperty<GameState> _state = new(GameState.Playing);
        private IDisposable _winSub;


        public GameStateService(EventBus eventBus)
        {
            _eventBus = eventBus;
            _winSub = _eventBus.Subscribe<EnemyDefeatedEvent>(OnEnemyDefeated);
        }


        public void SetState(GameState state)
        {
            _state.Value = state;
        }

        public void Dispose()
        {
            _winSub?.Dispose();
            _state.Dispose();
        }


        private void OnEnemyDefeated(EnemyDefeatedEvent _)
        {
            if (IsPlaying)
                SetState(GameState.Win);
        }
    }
}

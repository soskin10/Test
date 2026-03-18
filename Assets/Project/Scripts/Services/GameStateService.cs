using System;
using R3;

namespace Project.Scripts.Services
{
    public class GameStateService : IGameStateService, IDisposable
    {
        public ReadOnlyReactiveProperty<GameState> State => _state;
        public bool IsPlaying => _state.Value == GameState.Playing;
        
        
        private readonly ReactiveProperty<GameState> _state = new(GameState.Playing);


        public void SetState(GameState state)
        {
            _state.Value = state;
        }

        public void Dispose()
        {
            _state.Dispose();
        }
    }
}
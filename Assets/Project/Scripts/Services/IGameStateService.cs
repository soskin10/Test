using R3;

namespace Project.Scripts.Services
{
    public interface IGameStateService
    {
        ReadOnlyReactiveProperty<GameState> State { get; }
        bool IsPlaying { get; }

        void SetState(GameState state);
    }
}
using R3;

namespace Project.Scripts.Gameplay.UI
{
    public interface IReadyPulseCoordinator
    {
        Observable<float> Alpha { get; }
    }
}
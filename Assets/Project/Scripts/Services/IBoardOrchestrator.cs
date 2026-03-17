using Cysharp.Threading.Tasks;
using Project.Scripts.Services.ServiceLocatorSystem;

namespace Project.Scripts.Services
{
    public interface IBoardOrchestrator : IService
    {
        UniTask StartGame();
    }
}

using Cysharp.Threading.Tasks;
using Project.Scripts.Services.ServiceLocatorSystem;
using Project.Scripts.SpawnRules;

namespace Project.Scripts.Services
{
    public interface IGravityHandler : IService
    {
        UniTask ApplyGravity();
        UniTask SpawnNewTiles(SpawnContext context);
    }
}

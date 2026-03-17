using Cysharp.Threading.Tasks;

namespace Project.Scripts.Services.ServiceLocatorSystem
{
    public interface IService
    {
        UniTask InitAsync();
    }
}
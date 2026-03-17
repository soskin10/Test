using Project.Scripts.Services.ServiceLocatorSystem;

namespace Project.Scripts.Services
{
    public interface IMoveChecker : IService
    {
        bool HasPossibleMoves();
    }
}
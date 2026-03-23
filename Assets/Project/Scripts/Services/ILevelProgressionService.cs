namespace Project.Scripts.Services
{
    public interface ILevelProgressionService
    {
        void Advance();
        void Retry();
    }
}
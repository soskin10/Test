namespace Project.Scripts.Services.Combat
{
    public interface IEnemyStateService
    {
        int CurrentHP { get; }
        int MaxHP { get; }
    }
}
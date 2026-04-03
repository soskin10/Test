namespace Project.Scripts.Services.Combat
{
    public interface IEnemyStateService
    {
        int CurrentHP { get; }
        int MaxHP { get; }
        void ApplyDamage(int amount);
        void ApplyHeal(int amount);
    }
}
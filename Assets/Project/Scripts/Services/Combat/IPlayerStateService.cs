namespace Project.Scripts.Services.Combat
{
    public interface IPlayerStateService
    {
        int CurrentHP { get; }
        int MaxHP { get; }
        void Heal(int amount);
        void TakeDamage(int amount);
    }
}
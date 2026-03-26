namespace Project.Scripts.Services.Combat
{
    public interface IPlayerStateService
    {
        int CurrentHP { get; }
        int MaxHP { get; }
    }
}
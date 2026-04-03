namespace Project.Scripts.Services.Combat
{
    public interface IPlayerAvatarChargeService
    {
        int CurrentEnergy { get; }
        int MaxEnergy { get; }
        bool IsReady { get; }
        int TryRelease();
    }
}
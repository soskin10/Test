using Project.Scripts.Configs;

namespace Project.Scripts.Services.Combat
{
    public class PlayerStateService : IPlayerStateService
    {
        public int CurrentHP { get; private set; }
        public int MaxHP { get; }


        public PlayerStateService(LevelConfig levelConfig)
        {
            MaxHP = levelConfig.PlayerHP;
            CurrentHP = levelConfig.PlayerHP;
        }
    }
}
namespace Project.Scripts.Shared.Bot
{
    public readonly struct BotSettings
    {
        public readonly float MinAttackInterval;
        public readonly float MaxAttackInterval;
        public readonly int MinAttackDamage;
        public readonly int MaxAttackDamage;


        public BotSettings(
            float minAttackInterval,
            float maxAttackInterval,
            int minAttackDamage,
            int maxAttackDamage)
        {
            MinAttackInterval = minAttackInterval;
            MaxAttackInterval = maxAttackInterval;
            MinAttackDamage = minAttackDamage;
            MaxAttackDamage = maxAttackDamage;
        }
    }
}
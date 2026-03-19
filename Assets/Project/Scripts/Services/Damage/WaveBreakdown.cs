using System.Collections.Generic;

namespace Project.Scripts.Services.Damage
{
    public readonly struct WaveBreakdown
    {
        public readonly int CascadeLevel;
        public readonly IReadOnlyList<MatchInfo> Matches;
        public readonly int RawDamage;
        public readonly float CascadeMultiplier;
        public readonly bool HasMultiMatchBonus;
        public readonly int Total;
        public readonly int TotalEnergy;


        public WaveBreakdown(int cascadeLevel, IReadOnlyList<MatchInfo> matches,
            int rawDamage, float cascadeMultiplier, bool hasMultiMatchBonus, int total, int totalEnergy)
        {
            CascadeLevel = cascadeLevel;
            Matches = matches;
            RawDamage = rawDamage;
            CascadeMultiplier = cascadeMultiplier;
            HasMultiMatchBonus = hasMultiMatchBonus;
            Total = total;
            TotalEnergy = totalEnergy;
        }
    }
}

using System.Collections.Generic;
using System.Text;
using Project.Scripts.Shared.Grid;

namespace Project.Scripts.Shared.Damage
{
    public readonly struct DamageBreakdown
    {
        public readonly IReadOnlyList<WaveBreakdown> Waves;
        public readonly int BombDamage;
        public readonly int Total;
        public readonly int TotalEnergy;


        public DamageBreakdown(IReadOnlyList<WaveBreakdown> waves, int bombDamage)
        {
            Waves = waves;
            BombDamage = bombDamage;

            var total = bombDamage;
            var totalEnergy = 0;
            for (var i = 0; i < waves.Count; i++)
            {
                total += waves[i].Total;
                totalEnergy += waves[i].TotalEnergy;
            }

            Total = total;
            TotalEnergy = totalEnergy;
        }

        public string ToLogString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("[PlayerCharge] Charge added:");

            for (var i = 0; i < Waves.Count; i++)
            {
                var wave = Waves[i];
                var cascadeTag = wave.CascadeLevel > 1 ? $" cascade x{wave.CascadeMultiplier:F1}" : "";
                var multiTag = wave.HasMultiMatchBonus ? " +multi" : "";
                sb.AppendLine($" Wave {wave.CascadeLevel}{cascadeTag}{multiTag}:");

                for (var j = 0; j < wave.Matches.Count; j++)
                {
                    var m = wave.Matches[j];
                    var shapeTag = m.Shape switch
                    {
                        MatchShape.Horizontal => "→",
                        MatchShape.Vertical => "↑",
                        MatchShape.LShape => "L",
                        MatchShape.TShape => "T",
                        _ => "?"
                    };
                    sb.AppendLine($"   Match-{m.MaxLineLength} {m.TileKind} [{shapeTag}] {m.TileCount}t → {m.Damage} dmg");
                }

                sb.AppendLine($"   Subtotal: {wave.Total} dmg");
            }

            if (BombDamage > 0)
                sb.AppendLine($" Bomb: {BombDamage} dmg");

            sb.Append($" TOTAL: {Total} dmg");
            return sb.ToString();
        }
    }
}
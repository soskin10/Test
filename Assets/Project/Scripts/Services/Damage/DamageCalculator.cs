using System.Collections.Generic;
using Project.Scripts.Configs;
using Project.Scripts.Configs.Battle;
using Project.Scripts.Shared.Damage;
using Project.Scripts.Shared.Grid;
using UnityEngine;

namespace Project.Scripts.Services.Damage
{
    public class DamageCalculator : IDamageCalculator
    {
        private readonly MatchDamageConfig _config;


        public DamageCalculator(MatchDamageConfig config)
        {
            _config = config;
        }

        public WaveBreakdown CalculateWave(List<MatchResult> matches, int cascadeLevel)
        {
            var matchInfos = new List<MatchInfo>(matches.Count);
            var rawDamage = 0;
            var totalEnergy = 0;

            for (var i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                var damage = GetMatchBaseDamage(match.MaxLineLength) + GetShapeBonus(match.Shape);
                var energy = GetEnergyGenerated(match.MaxLineLength);
                rawDamage += damage;
                totalEnergy += energy;

                matchInfos.Add(new MatchInfo(
                    match.Shape,
                    match.TileKind,
                    match.MaxLineLength,
                    match.Positions.Count,
                    match.Center,
                    damage,
                    energy));
            }

            var multiplier = 1f + _config.CascadeMultiplierStep * (cascadeLevel - 1);
            var multiMatch = matches.Count > 1;

            var totalF = rawDamage * multiplier;
            if (multiMatch)
                totalF *= 1f + _config.MultiMatchBonus;

            return new WaveBreakdown(cascadeLevel, matchInfos, rawDamage, multiplier, multiMatch,
                Mathf.RoundToInt(totalF), totalEnergy);
        }

        public int CalculateBombDamage(int tilesDestroyed)
        {
            return tilesDestroyed * _config.BombDamagePerTile;
        }

        private int GetMatchBaseDamage(int matchSize)
        {
            if (matchSize >= 5)
                return _config.Match5PlusDamage + (matchSize - 5) * _config.ExtraTileDamage;
            if (matchSize == 4)
                return _config.Match4Damage;
            
            return _config.Match3Damage;
        }

        private int GetShapeBonus(MatchShape shape)
        {
            return shape switch
            {
                MatchShape.LShape => _config.LShapeBonus,
                MatchShape.TShape => _config.TShapeBonus,
                _ => 0
            };
        }

        private int GetEnergyGenerated(int matchLength)
        {
            if (matchLength >= 5) 
                return _config.Match5PlusEnergy;
            if (matchLength == 4) 
                return _config.Match4Energy;
            
            return _config.Match3Energy;
        }
    }
}
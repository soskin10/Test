using System;
using System.Collections.Generic;
using Project.Scripts.Configs;
using R3;
using UnityEngine;

namespace Project.Scripts.Services
{
    public class ScoreService : IScoreService, IDisposable
    {
        private readonly ReactiveProperty<int> _score = new(0);
        private readonly ScoreConfig _config;


        public ReadOnlyReactiveProperty<int> Score => _score;


        public ScoreService(ScoreConfig config)
        {
            _config = config;
        }

        public void AddMatchScore(List<MatchResult> matches, int cascadeLevel)
        {
            float multiplier = 1f + _config.CascadeMultiplierStep * (cascadeLevel - 1);
            int total = 0;

            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                int tileCount = match.Positions.Count;
                int length = match.MaxLineLength;

                var bonusTable = _config.BonusPerTileByLength;
                int bonusIndex = Mathf.Min(length, bonusTable.Length - 1);
                int bonusPerTile = bonusIndex >= 0 ? bonusTable[bonusIndex] : 0;

                int matchScore = (_config.PointsPerTile + bonusPerTile) * tileCount;

                if (match.IsComplex)
                    matchScore += _config.ComplexMatchBonus;

                total += matchScore;
            }

            _score.Value += Mathf.RoundToInt(total * multiplier);
        }

        public void AddBombScore(int tilesDestroyed)
        {
            _score.Value += tilesDestroyed * _config.BombKillPointsPerTile;
        }

        public void Reset()
        {
            _score.Value = 0;
        }

        public void Dispose()
        {
            _score.Dispose();
        }
    }
}
using Project.Scripts.Services.Grid;
using Project.Scripts.Tiles;
using UnityEngine;

namespace Project.Scripts.Services.Damage
{
    public readonly struct MatchInfo
    {
        public readonly MatchShape Shape;
        public readonly TileType TileType;
        public readonly int MaxLineLength;
        public readonly int TileCount;
        public readonly Vector2Int Center;
        public readonly int Damage;
        public readonly int EnergyGenerated;


        public MatchInfo(MatchShape shape, TileType tileType, int maxLineLength,
            int tileCount, Vector2Int center, int damage, int energyGenerated)
        {
            Shape = shape;
            TileType = tileType;
            MaxLineLength = maxLineLength;
            TileCount = tileCount;
            Center = center;
            Damage = damage;
            EnergyGenerated = energyGenerated;
        }
    }
}
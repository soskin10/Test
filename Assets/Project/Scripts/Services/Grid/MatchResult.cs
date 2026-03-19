using System.Collections.Generic;
using Project.Scripts.Tiles;
using UnityEngine;

namespace Project.Scripts.Services.Grid
{
    public class MatchResult
    {
        public List<Vector2Int> Positions;
        public int MaxLineLength;
        public bool IsComplex;
        public TileType TileType;
        public MatchShape Shape;
        public Vector2Int Center;
    }
}
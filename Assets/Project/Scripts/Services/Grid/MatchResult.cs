using System.Collections.Generic;
using Project.Scripts.Shared;
using Project.Scripts.Tiles;

namespace Project.Scripts.Services.Grid
{
    public class MatchResult
    {
        public List<GridPoint> Positions;
        public int MaxLineLength;
        public bool IsComplex;
        public TileKind TileKind;
        public MatchShape Shape;
        public GridPoint Center;
    }
}
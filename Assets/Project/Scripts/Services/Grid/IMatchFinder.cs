using System.Collections.Generic;
using Project.Scripts.Tiles;

namespace Project.Scripts.Services.Grid
{
    public interface IMatchFinder
    {
        List<MatchResult> FindMatches(TileType[,] grid);
    }
}
using System.Collections.Generic;
using Project.Scripts.Services.ServiceLocatorSystem;
using Project.Scripts.Tiles;

namespace Project.Scripts.Services
{
    public interface IMatchFinder : IService
    {
        List<MatchResult> FindMatches(TileType[,] grid);
    }
}
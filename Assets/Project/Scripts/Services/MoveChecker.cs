using Cysharp.Threading.Tasks;
using Project.Scripts.Configs;
using Project.Scripts.Tiles;
using UnityEngine;

namespace Project.Scripts.Services
{
    public class MoveChecker : IMoveChecker
    {
        private readonly IGridManager _grid;
        private readonly IMatchFinder _matchFinder;
        private readonly BoardConfig _config;


        public MoveChecker(IGridManager grid, IMatchFinder matchFinder, BoardConfig config)
        {
            _grid = grid;
            _matchFinder = matchFinder;
            _config = config;
        }

        public UniTask InitAsync() => UniTask.CompletedTask;

        public bool HasPossibleMoves()
        {
            var state = _grid.GetGridState();
            for (int x = 0; x < _config.Width; x++)
                for (int y = 0; y < _config.Height; y++)
                {
                    var tileA = _grid.GetTile(new Vector2Int(x, y));
                    if (!tileA) continue;

                    if (x + 1 < _config.Width)
                    {
                        var tileB = _grid.GetTile(new Vector2Int(x + 1, y));
                        if (tileB && IsValidSwap(state, tileA, tileB, x, y, x + 1, y))
                            return true;
                    }

                    if (y + 1 < _config.Height)
                    {
                        var tileB = _grid.GetTile(new Vector2Int(x, y + 1));
                        if (tileB && IsValidSwap(state, tileA, tileB, x, y, x, y + 1))
                            return true;
                    }
                }

            return false;
        }


        private bool IsValidSwap(TileType[,] state, Tile tileA, Tile tileB, int x1, int y1, int x2, int y2)
        {
            if (tileA.Config.Behaviour.IsActivatedBySwap || tileB.Config.Behaviour.IsActivatedBySwap)
                return true;

            (state[x1, y1], state[x2, y2]) = (state[x2, y2], state[x1, y1]);
            var hasMatch = _matchFinder.FindMatches(state).Count > 0;
            (state[x1, y1], state[x2, y2]) = (state[x2, y2], state[x1, y1]);
            
            return hasMatch;
        }
    }
}
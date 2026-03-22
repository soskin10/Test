using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Scripts.Configs;
using UnityEngine;

namespace Project.Scripts.Services.Grid
{
    public class GravityHandler : IGravityHandler
    {
        private readonly IGridManager _grid;
        private readonly TilePool _pool;
        private readonly LevelConfig _config;


        public GravityHandler(IGridManager grid, TilePool pool, LevelConfig config)
        {
            _grid = grid;
            _pool = pool;
            _config = config;
        }


        public async UniTask ApplyGravity()
        {
            var tasks = new List<UniTask>();
            for (var x = 0; x < _config.Width; x++)
            {
                var writeY = 0;
                for (var readY = 0; readY < _config.Height; readY++)
                {
                    var tile = _grid.GetTile(new Vector2Int(x, readY));
                    if (!tile)
                        continue;

                    if (readY != writeY)
                    {
                        var from = new Vector2Int(x, readY);
                        var to = new Vector2Int(x, writeY);
                        _grid.ClearTile(from);
                        _grid.SetTile(to, tile);
                        tile.GridPosition = to;
                        tasks.Add(tile.Animator.AnimateFallTo(_grid.GridToWorld(to)));
                    }
                    writeY++;
                }
            }

            await UniTask.WhenAll(tasks);
        }

        public async UniTask SpawnNewTiles()
        {
            var emptyPositions = new List<Vector2Int>();
            for (var x = 0; x < _config.Width; x++)
                for (var y = _config.Height - 1; y >= 0; y--)
                {
                    var pos = new Vector2Int(x, y);
                    if (!_grid.GetTile(pos))
                        emptyPositions.Add(pos);
                }

            var spawnHeights = new int[_config.Width];
            for (var x = 0; x < _config.Width; x++)
                spawnHeights[x] = _config.Height;

            var tasks = new List<UniTask>();
            for (var i = 0; i < emptyPositions.Count; i++)
            {
                var pos = emptyPositions[i];
                var tileConfig = _grid.ResolveRegularTile();
                var tile = _pool.Get();
                tile.transform.position = _grid.GridToWorld(new Vector2Int(pos.x, spawnHeights[pos.x]));
                tile.Init(tileConfig, pos);
                _grid.SetTile(pos, tile);
                tasks.Add(tile.Animator.AnimateFallTo(_grid.GridToWorld(pos)));
                spawnHeights[pos.x]++;
            }

            await UniTask.WhenAll(tasks);
        }
    }
}

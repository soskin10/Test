using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Scripts.Configs;
using Project.Scripts.SpawnRules;
using UnityEngine;

namespace Project.Scripts.Services.Grid
{
    public class GravityHandler : IGravityHandler
    {
        private readonly IGridManager _grid;
        private readonly TilePool _pool;
        private readonly BoardConfig _config;


        public GravityHandler(IGridManager grid, TilePool pool, BoardConfig config)
        {
            _grid = grid;
            _pool = pool;
            _config = config;
        }


        public UniTask InitAsync() => UniTask.CompletedTask;

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

        public async UniTask SpawnNewTiles(SpawnContext context)
        {
            var emptyPositions = new List<Vector2Int>();
            for (var x = 0; x < _config.Width; x++)
                for (var y = _config.Height - 1; y >= 0; y--)
                {
                    var pos = new Vector2Int(x, y);
                    if (!_grid.GetTile(pos))
                        emptyPositions.Add(pos);
                }

            Shuffle(emptyPositions);

            var spawnHeights = new int[_config.Width];
            for (var x = 0; x < _config.Width; x++)
                spawnHeights[x] = _config.Height;

            var tasks = new List<UniTask>();
            for (var i = 0; i < emptyPositions.Count; i++)
            {
                var pos = emptyPositions[i];
                var tileConfig = _grid.ResolveNextTile(context);
                var tile = _pool.Get();
                tile.transform.position = _grid.GridToWorld(new Vector2Int(pos.x, spawnHeights[pos.x]));
                tile.Init(tileConfig, pos);
                _grid.SetTile(pos, tile);
                tasks.Add(tile.Animator.AnimateFallTo(_grid.GridToWorld(pos)));
                spawnHeights[pos.x]++;
            }

            await UniTask.WhenAll(tasks);
        }

        private static void Shuffle(List<Vector2Int> list)
        {
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
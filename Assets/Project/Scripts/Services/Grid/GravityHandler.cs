using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Scripts.Configs;
using Project.Scripts.Configs.Levels;
using Project.Scripts.Shared;

namespace Project.Scripts.Services.Grid
{
    public class GravityHandler : IGravityHandler
    {
        private readonly IGridState _state;
        private readonly IGridView _view;
        private readonly TilePool _pool;
        private readonly LevelConfig _config;


        public GravityHandler(IGridState state, IGridView view, TilePool pool, LevelConfig config)
        {
            _state = state;
            _view = view;
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
                    var tile = _view.GetTile(new GridPoint(x, readY));
                    if (!tile)
                        continue;

                    if (readY != writeY)
                    {
                        var from = new GridPoint(x, readY);
                        var to = new GridPoint(x, writeY);
                        _view.ClearTile(from);
                        _view.SetTile(to, tile);
                        tile.GridPosition = to;
                        tasks.Add(tile.Animator.AnimateFallTo(_view.GridToWorld(to)));
                    }
                    writeY++;
                }
            }

            await UniTask.WhenAll(tasks);
        }

        public async UniTask SpawnNewTiles()
        {
            var emptyPositions = new List<GridPoint>();
            for (var x = 0; x < _config.Width; x++)
                for (var y = _config.Height - 1; y >= 0; y--)
                {
                    var pos = new GridPoint(x, y);
                    if (!_view.GetTile(pos))
                        emptyPositions.Add(pos);
                }

            var spawnHeights = new int[_config.Width];
            for (var x = 0; x < _config.Width; x++)
                spawnHeights[x] = _config.Height;

            var tasks = new List<UniTask>();
            for (var i = 0; i < emptyPositions.Count; i++)
            {
                var pos = emptyPositions[i];
                var tileConfig = _view.ResolveRegularTile();
                var tile = _pool.Get();
                tile.transform.position = _view.GridToWorld(new GridPoint(pos.X, spawnHeights[pos.X]));
                tile.Init(tileConfig, pos);
                _view.SetTile(pos, tile);
                tasks.Add(tile.Animator.AnimateFallTo(_view.GridToWorld(pos)));
                spawnHeights[pos.X]++;
            }

            await UniTask.WhenAll(tasks);
        }
    }
}
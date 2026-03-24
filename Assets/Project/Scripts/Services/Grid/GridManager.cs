using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Scripts.Configs;
using Project.Scripts.Shared;
using Project.Scripts.Tiles;
using UnityEngine;

namespace Project.Scripts.Services.Grid
{
    public class GridManager : IGridManager
    {
        private readonly LevelConfig _levelConfig;
        private readonly AnimationConfig _animConfig;
        private readonly TilePool _pool;
        private readonly Tile[,] _grid;
        private readonly HashSet<GridPoint> _scheduledRemovals = new();
        private readonly float _cellSize;
        private Vector3 _origin;


        public GridManager(LevelConfig levelConfig, AnimationConfig animConfig, TilePool pool, float cellSize)
        {
            _levelConfig = levelConfig;
            _animConfig = animConfig;
            _pool = pool;
            _cellSize = cellSize;
            _grid = new Tile[levelConfig.Width, levelConfig.Height];
        }

        public void SetOrigin(Vector3 origin)
        {
            _origin = origin;
        }

        public Tile GetTile(GridPoint pos)
        {
            return _grid[pos.X, pos.Y];
        }

        public void SetTile(GridPoint pos, Tile tile)
        {
            _grid[pos.X, pos.Y] = tile;
        }

        public void ClearTile(GridPoint pos)
        {
            _grid[pos.X, pos.Y] = null;
        }

        public bool IsValidPosition(GridPoint pos)
        {
            return pos.X >= 0 && pos.X < _levelConfig.Width && pos.Y >= 0 && pos.Y < _levelConfig.Height;
        }

        public Vector3 GridToWorld(GridPoint pos)
        {
            return _origin + new Vector3(pos.X * _cellSize, pos.Y * _cellSize, 0f);
        }

        public GridPoint WorldToGrid(Vector3 worldPos)
        {
            var relative = worldPos - _origin;

            return new GridPoint(
                Mathf.RoundToInt(relative.x / _cellSize),
                Mathf.RoundToInt(relative.y / _cellSize)
            );
        }

        public TileKind[,] GetGridState()
        {
            var state = new TileKind[_levelConfig.Width, _levelConfig.Height];
            for (var x = 0; x < _levelConfig.Width; x++)
                for (var y = 0; y < _levelConfig.Height; y++)
                    state[x, y] = _grid[x, y] ? _grid[x, y].Kind : TileKind.None;

            return state;
        }

        public TileConfig ResolveRegularTile()
        {
            return _levelConfig.RegularTiles[UnityEngine.Random.Range(0, _levelConfig.RegularTiles.Length)];
        }

        public void ScheduleRemove(List<GridPoint> positions)
        {
            for (var i = 0; i < positions.Count; i++)
                _scheduledRemovals.Add(positions[i]);
        }

        public List<GridPoint> GetNeighboursInRadius(GridPoint center, int radius)
        {
            var result = new List<GridPoint>();
            for (var dx = -radius; dx <= radius; dx++)
                for (var dy = -radius; dy <= radius; dy++)
                {
                    if (dx == 0 && dy == 0)
                        continue;

                    var pos = new GridPoint(center.X + dx, center.Y + dy);
                    if (IsValidPosition(pos))
                        result.Add(pos);
                }

            return result;
        }

        public List<GridPoint> GetAllInRow(int y)
        {
            var result = new List<GridPoint>(_levelConfig.Width);
            for (var x = 0; x < _levelConfig.Width; x++)
                result.Add(new GridPoint(x, y));

            return result;
        }

        public List<GridPoint> GetAllInColumn(int x)
        {
            var result = new List<GridPoint>(_levelConfig.Height);
            for (var y = 0; y < _levelConfig.Height; y++)
                result.Add(new GridPoint(x, y));

            return result;
        }

        public List<GridPoint> GetAllOfKind(TileKind kind)
        {
            var result = new List<GridPoint>();
            for (var x = 0; x < _levelConfig.Width; x++)
                for (var y = 0; y < _levelConfig.Height; y++)
                    if (_grid[x, y] && _grid[x, y].Kind == kind)
                        result.Add(new GridPoint(x, y));

            return result;
        }

        public List<GridPoint> GetAllOccupied()
        {
            var result = new List<GridPoint>();
            for (var x = 0; x < _levelConfig.Width; x++)
                for (var y = 0; y < _levelConfig.Height; y++)
                    if (_grid[x, y])
                        result.Add(new GridPoint(x, y));

            return result;
        }

        public TileKind GetMostCommonColor()
        {
            var counts = new Dictionary<TileKind, int>();
            for (var x = 0; x < _levelConfig.Width; x++)
                for (var y = 0; y < _levelConfig.Height; y++)
                {
                    var tile = _grid[x, y];
                    if (false == tile)
                        continue;

                    var kind = tile.Kind;
                    if (false == kind.IsColor())
                        continue;

                    counts.TryGetValue(kind, out var count);
                    counts[kind] = count + 1;
                }

            var bestKind = TileKind.None;
            var bestCount = 0;
            foreach (var kvp in counts)
            {
                if (kvp.Value > bestCount)
                {
                    bestCount = kvp.Value;
                    bestKind = kvp.Key;
                }
            }

            return bestKind;
        }

        public async UniTask PopulateGrid()
        {
            var kindCache = new TileKind[_levelConfig.Width, _levelConfig.Height];
            var tasks = new UniTask[_levelConfig.Width * _levelConfig.Height];
            var idx = 0;
            for (var x = 0; x < _levelConfig.Width; x++)
                for (var y = 0; y < _levelConfig.Height; y++)
                {
                    var pos = new GridPoint(x, y);
                    var tileConfig = GetNoMatchConfig(x, y, kindCache);
                    kindCache[x, y] = tileConfig.Kind;
                    var tile = _pool.Get();
                    tile.transform.position = GridToWorld(pos);
                    tile.Init(tileConfig, pos);
                    _grid[x, y] = tile;
                    tasks[idx++] = tile.Animator.AnimateSpawn();
                }

            await UniTask.WhenAll(tasks);
        }

        public async UniTask RemoveMatches(List<MatchResult> matches, Dictionary<GridPoint, SpecialTileSpawnData> specialPlacements)
        {
            var posSet = new HashSet<GridPoint>();
            for (var i = 0; i < matches.Count; i++)
                for (var j = 0; j < matches[i].Positions.Count; j++)
                    posSet.Add(matches[i].Positions[j]);

            await ProcessRemovals(new List<GridPoint>(posSet), specialPlacements);
            await ProcessScheduledRemovals();
        }

        public async UniTask ActivateBySwap(GridPoint pos)
        {
            await ProcessRemovals(new List<GridPoint> { pos }, null);
            await ProcessScheduledRemovals();
        }

        public async UniTask ActivateTiles(List<GridPoint> positions)
        {
            if (positions.Count == 0)
                return;

            await ProcessRemovals(positions, null);
            await ProcessScheduledRemovals();
        }

        public async UniTask ConsumeTile(GridPoint pos)
        {
            if (false == IsValidPosition(pos))
                return;

            var tile = _grid[pos.X, pos.Y];
            if (false == tile)
                return;

            await tile.Animator.AnimateDestroy();
            _grid[pos.X, pos.Y] = null;
            _pool.Release(tile);
        }

        public async UniTask SwapTiles(GridPoint from, GridPoint to)
        {
            var tileA = _grid[from.X, from.Y];
            var tileB = _grid[to.X, to.Y];

            _grid[from.X, from.Y] = tileB;
            _grid[to.X, to.Y] = tileA;

            if (tileA)
                tileA.GridPosition = to;
            if (tileB)
                tileB.GridPosition = from;

            await UniTask.WhenAll(
                tileA ? tileA.Animator.AnimateSwapTo(GridToWorld(to)) : UniTask.CompletedTask,
                tileB ? tileB.Animator.AnimateSwapTo(GridToWorld(from)) : UniTask.CompletedTask
            );
        }

        public async UniTask ShuffleGrid()
        {
            var positions = new List<GridPoint>();
            var configs = new List<TileConfig>();
            for (var x = 0; x < _levelConfig.Width; x++)
                for (var y = 0; y < _levelConfig.Height; y++)
                {
                    var tile = _grid[x, y];
                    if (tile)
                    {
                        positions.Add(new GridPoint(x, y));
                        configs.Add(tile.Config);
                    }
                }

            for (var i = configs.Count - 1; i > 0; i--)
            {
                var j = UnityEngine.Random.Range(0, i + 1);
                (configs[i], configs[j]) = (configs[j], configs[i]);
            }

            var assignedKinds = new TileKind[_levelConfig.Width, _levelConfig.Height];
            for (var i = 0; i < positions.Count; i++)
            {
                var pos = positions[i];
                for (var j = i; j < configs.Count; j++)
                {
                    if (false == WouldCreateMatch(pos.X, pos.Y, configs[j].Kind, assignedKinds))
                    {
                        if (j != i) (configs[i], configs[j]) = (configs[j], configs[i]);
                        break;
                    }
                }
                assignedKinds[pos.X, pos.Y] = configs[i].Kind;
            }

            var tasks = new List<UniTask>();
            for (var i = 0; i < positions.Count; i++)
            {
                var pos = positions[i];
                _grid[pos.X, pos.Y].Init(configs[i], pos);
                tasks.Add(_grid[pos.X, pos.Y].Animator.AnimateSpawn());
            }

            await UniTask.WhenAll(tasks);
        }

        public void ForceInjectMove()
        {
            if (_levelConfig.RegularTiles.Length < 2 || _levelConfig.Width < 4)
                return;

            var configT = _levelConfig.RegularTiles[0];
            var configX = _levelConfig.RegularTiles[1];

            ReInitTileAt(0, 0, configT);
            ReInitTileAt(1, 0, configX);
            ReInitTileAt(2, 0, configT);
            ReInitTileAt(3, 0, configT);
        }

#if UNITY_EDITOR
        public void ReplaceForEdit(GridPoint pos, TileKind kind)
        {
            var tile = _grid[pos.X, pos.Y];
            if (false == tile)
                return;

            var config = FindConfigForKind(kind);
            if (!config)
                return;

            tile.Init(config, pos);
        }

        private TileConfig FindConfigForKind(TileKind kind)
        {
            for (var i = 0; i < _levelConfig.RegularTiles.Length; i++)
                if (_levelConfig.RegularTiles[i].Kind == kind)
                    return _levelConfig.RegularTiles[i];

            if (_levelConfig.SpecialTiles != null)
                for (var i = 0; i < _levelConfig.SpecialTiles.Length; i++)
                    if (_levelConfig.SpecialTiles[i].Kind == kind)
                        return _levelConfig.SpecialTiles[i];

            return null;
        }
#endif

        private async UniTask ProcessScheduledRemovals()
        {
            while (_scheduledRemovals.Count > 0)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_animConfig.ChainReactionWaveDelay));
                var batch = new List<GridPoint>(_scheduledRemovals);
                _scheduledRemovals.Clear();
                await ProcessRemovals(batch, null);
            }
        }

        private async UniTask ProcessRemovals(List<GridPoint> positions, Dictionary<GridPoint, SpecialTileSpawnData> specialPlacements)
        {
            var toDestroy = new List<(GridPoint pos, Tile tile)>(positions.Count);
            for (var i = 0; i < positions.Count; i++)
            {
                var pos = positions[i];
                if (false == IsValidPosition(pos))
                    continue;

                var tile = _grid[pos.X, pos.Y];
                if (false == tile)
                    continue;

                toDestroy.Add((pos, tile));
            }

            if (toDestroy.Count == 0)
                return;

            var destroyTasks = new UniTask[toDestroy.Count];
            for (var i = 0; i < toDestroy.Count; i++)
                destroyTasks[i] = toDestroy[i].tile.Animator.AnimateDestroy();

            await UniTask.WhenAll(destroyTasks);

            for (var i = 0; i < toDestroy.Count; i++)
            {
                var (pos, tile) = toDestroy[i];
                tile.Config.Behaviour.OnTileDestroyed(pos, this);
                _grid[pos.X, pos.Y] = null;
                _pool.Release(tile);
            }

            if (null == specialPlacements || specialPlacements.Count == 0)
                return;

            var spawnTasks = new List<UniTask>(specialPlacements.Count);
            foreach (var kvp in specialPlacements)
            {
                var pos = kvp.Key;
                if (false == IsValidPosition(pos))
                    continue;

                if (_grid[pos.X, pos.Y])
                    continue;

                var data = kvp.Value;
                var specialTile = _pool.Get();
                specialTile.transform.position = GridToWorld(pos);
                specialTile.Init(data.Config, pos, data.PayloadKind);
                _grid[pos.X, pos.Y] = specialTile;
                spawnTasks.Add(specialTile.Animator.AnimateSpawn());
            }

            if (spawnTasks.Count > 0)
                await UniTask.WhenAll(spawnTasks);
        }

        private void ReInitTileAt(int x, int y, TileConfig config)
        {
            var tile = _grid[x, y];
            if (tile)
                tile.Init(config, new GridPoint(x, y));
        }

        private bool WouldCreateMatch(int x, int y, TileKind kind, TileKind[,] assigned)
        {
            if (false == kind.IsColor())
                return false;

            if (x >= 2 && assigned[x - 1, y] == kind && assigned[x - 2, y] == kind)
                return true;

            if (y >= 2 && assigned[x, y - 1] == kind && assigned[x, y - 2] == kind)
                return true;

            return false;
        }

        private TileConfig GetNoMatchConfig(int x, int y, TileKind[,] kinds)
        {
            var forbidden = new HashSet<TileKind>();
            if (x >= 2 && kinds[x - 1, y] == kinds[x - 2, y] && kinds[x - 1, y].IsColor())
                forbidden.Add(kinds[x - 1, y]);
            if (y >= 2 && kinds[x, y - 1] == kinds[x, y - 2] && kinds[x, y - 1].IsColor())
                forbidden.Add(kinds[x, y - 1]);

            for (var attempt = 0; attempt < 10; attempt++)
            {
                var config = _levelConfig.RegularTiles[UnityEngine.Random.Range(0, _levelConfig.RegularTiles.Length)];
                if (false == forbidden.Contains(config.Kind))
                    return config;
            }

            return _levelConfig.RegularTiles[0];
        }
    }
}
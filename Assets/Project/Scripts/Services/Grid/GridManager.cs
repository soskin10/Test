using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Scripts.Configs;
using Project.Scripts.Tiles;
using UnityEngine;

namespace Project.Scripts.Services.Grid
{
    public class GridManager : IGridManager
    {
        private readonly BoardConfig _boardConfig;
        private readonly AnimationConfig _animConfig;
        private readonly TilePool _pool;
        private readonly Tile[,] _grid;
        private readonly HashSet<Vector2Int> _scheduledRemovals = new();
        private readonly float _cellSize;
        private Vector3 _origin;


        public GridManager(BoardConfig boardConfig, AnimationConfig animConfig, TilePool pool, float cellSize)
        {
            _boardConfig = boardConfig;
            _animConfig = animConfig;
            _pool = pool;
            _cellSize = cellSize;
            _grid = new Tile[boardConfig.Width, boardConfig.Height];
        }

        public void SetOrigin(Vector3 origin)
        {
            _origin = origin;
        }

        public Tile GetTile(Vector2Int pos)
        {
            return _grid[pos.x, pos.y];
        }

        public void SetTile(Vector2Int pos, Tile tile)
        {
            _grid[pos.x, pos.y] = tile;
        }

        public void ClearTile(Vector2Int pos)
        {
            _grid[pos.x, pos.y] = null;
        }

        public bool IsValidPosition(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < _boardConfig.Width && pos.y >= 0 && pos.y < _boardConfig.Height;
        }

        public Vector3 GridToWorld(Vector2Int pos)
        {
            return _origin + new Vector3(pos.x * _cellSize, pos.y * _cellSize, 0f);
        }

        public Vector2Int WorldToGrid(Vector3 worldPos)
        {
            var relative = worldPos - _origin;

            return new Vector2Int(
                Mathf.RoundToInt(relative.x / _cellSize),
                Mathf.RoundToInt(relative.y / _cellSize)
            );
        }

        public TileType[,] GetGridState()
        {
            var state = new TileType[_boardConfig.Width, _boardConfig.Height];
            for (var x = 0; x < _boardConfig.Width; x++)
                for (var y = 0; y < _boardConfig.Height; y++)
                    state[x, y] = _grid[x, y] ? _grid[x, y].Type : TileType.None;

            return state;
        }

        public TileConfig ResolveRegularTile()
        {
            return _boardConfig.RegularTiles[UnityEngine.Random.Range(0, _boardConfig.RegularTiles.Length)];
        }

        public void ScheduleRemove(List<Vector2Int> positions)
        {
            for (var i = 0; i < positions.Count; i++)
                _scheduledRemovals.Add(positions[i]);
        }

        public List<Vector2Int> GetNeighboursInRadius(Vector2Int center, int radius)
        {
            var result = new List<Vector2Int>();
            for (var dx = -radius; dx <= radius; dx++)
                for (var dy = -radius; dy <= radius; dy++)
                {
                    if (dx == 0 && dy == 0)
                        continue;

                    var pos = new Vector2Int(center.x + dx, center.y + dy);
                    if (IsValidPosition(pos))
                        result.Add(pos);
                }

            return result;
        }

        public List<Vector2Int> GetAllInRow(int y)
        {
            var result = new List<Vector2Int>(_boardConfig.Width);
            for (var x = 0; x < _boardConfig.Width; x++)
                result.Add(new Vector2Int(x, y));

            return result;
        }

        public List<Vector2Int> GetAllInColumn(int x)
        {
            var result = new List<Vector2Int>(_boardConfig.Height);
            for (var y = 0; y < _boardConfig.Height; y++)
                result.Add(new Vector2Int(x, y));

            return result;
        }

        public List<Vector2Int> GetAllOfType(TileType type)
        {
            var result = new List<Vector2Int>();
            for (var x = 0; x < _boardConfig.Width; x++)
                for (var y = 0; y < _boardConfig.Height; y++)
                    if (_grid[x, y] && _grid[x, y].Type == type)
                        result.Add(new Vector2Int(x, y));

            return result;
        }

        public List<Vector2Int> GetAllSpecialsOfKind(SpecialTileKind kind)
        {
            var result = new List<Vector2Int>();
            for (var x = 0; x < _boardConfig.Width; x++)
                for (var y = 0; y < _boardConfig.Height; y++)
                {
                    var tile = _grid[x, y];
                    if (tile && tile.Config.Behaviour.SpecialKind == kind)
                        result.Add(new Vector2Int(x, y));
                }

            return result;
        }

        public List<Vector2Int> GetAllOccupied()
        {
            var result = new List<Vector2Int>();
            for (var x = 0; x < _boardConfig.Width; x++)
                for (var y = 0; y < _boardConfig.Height; y++)
                    if (_grid[x, y])
                        result.Add(new Vector2Int(x, y));

            return result;
        }

        public TileType GetMostCommonRegularType()
        {
            var counts = new Dictionary<TileType, int>();
            for (var x = 0; x < _boardConfig.Width; x++)
                for (var y = 0; y < _boardConfig.Height; y++)
                {
                    var tile = _grid[x, y];
                    if (false == tile)
                        continue;

                    if (tile.Config.Behaviour.SpecialKind != SpecialTileKind.None)
                        continue;

                    var type = tile.Type;
                    if (type == TileType.None)
                        continue;

                    counts.TryGetValue(type, out var count);
                    counts[type] = count + 1;
                }

            var bestType = TileType.None;
            var bestCount = 0;
            foreach (var kvp in counts)
            {
                if (kvp.Value > bestCount)
                {
                    bestCount = kvp.Value;
                    bestType = kvp.Key;
                }
            }

            return bestType;
        }

        public async UniTask PopulateGrid()
        {
            var typeCache = new TileType[_boardConfig.Width, _boardConfig.Height];
            var tasks = new UniTask[_boardConfig.Width * _boardConfig.Height];
            var idx = 0;
            for (var x = 0; x < _boardConfig.Width; x++)
                for (var y = 0; y < _boardConfig.Height; y++)
                {
                    var pos = new Vector2Int(x, y);
                    var tileConfig = GetNoMatchConfig(x, y, typeCache);
                    typeCache[x, y] = tileConfig.Type;
                    var tile = _pool.Get();
                    tile.transform.position = GridToWorld(pos);
                    tile.Init(tileConfig, pos);
                    _grid[x, y] = tile;
                    tasks[idx++] = tile.Animator.AnimateSpawn();
                }

            await UniTask.WhenAll(tasks);
        }

        public async UniTask RemoveMatches(List<MatchResult> matches, Dictionary<Vector2Int, SpecialTileSpawnData> specialPlacements)
        {
            var posSet = new HashSet<Vector2Int>();
            for (var i = 0; i < matches.Count; i++)
                for (var j = 0; j < matches[i].Positions.Count; j++)
                    posSet.Add(matches[i].Positions[j]);

            await ProcessRemovals(new List<Vector2Int>(posSet), specialPlacements);
            await ProcessScheduledRemovals();
        }

        public async UniTask ActivateBySwap(Vector2Int pos)
        {
            await ProcessRemovals(new List<Vector2Int> { pos }, null);
            await ProcessScheduledRemovals();
        }

        public async UniTask ActivateTiles(List<Vector2Int> positions)
        {
            if (positions.Count == 0)
                return;

            await ProcessRemovals(positions, null);
            await ProcessScheduledRemovals();
        }

        public async UniTask ConsumeTile(Vector2Int pos)
        {
            if (false == IsValidPosition(pos))
                return;

            var tile = _grid[pos.x, pos.y];
            if (false == tile)
                return;

            await tile.Animator.AnimateDestroy();
            _grid[pos.x, pos.y] = null;
            _pool.Release(tile);
        }

        public async UniTask SwapTiles(Vector2Int from, Vector2Int to)
        {
            var tileA = _grid[from.x, from.y];
            var tileB = _grid[to.x, to.y];

            _grid[from.x, from.y] = tileB;
            _grid[to.x, to.y] = tileA;

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
            var positions = new List<Vector2Int>();
            var configs = new List<TileConfig>();
            for (var x = 0; x < _boardConfig.Width; x++)
                for (var y = 0; y < _boardConfig.Height; y++)
                {
                    var tile = _grid[x, y];
                    if (tile)
                    {
                        positions.Add(new Vector2Int(x, y));
                        configs.Add(tile.Config);
                    }
                }

            for (var i = configs.Count - 1; i > 0; i--)
            {
                var j = UnityEngine.Random.Range(0, i + 1);
                (configs[i], configs[j]) = (configs[j], configs[i]);
            }

            var assignedTypes = new TileType[_boardConfig.Width, _boardConfig.Height];
            for (var i = 0; i < positions.Count; i++)
            {
                var pos = positions[i];
                for (var j = i; j < configs.Count; j++)
                {
                    if (false == WouldCreateMatch(pos.x, pos.y, configs[j].Type, assignedTypes))
                    {
                        if (j != i) (configs[i], configs[j]) = (configs[j], configs[i]);
                        break;
                    }
                }
                assignedTypes[pos.x, pos.y] = configs[i].Type;
            }

            var tasks = new List<UniTask>();
            for (var i = 0; i < positions.Count; i++)
            {
                var pos = positions[i];
                _grid[pos.x, pos.y].Init(configs[i], pos);
                tasks.Add(_grid[pos.x, pos.y].Animator.AnimateSpawn());
            }

            await UniTask.WhenAll(tasks);
        }

        public void ForceInjectMove()
        {
            if (_boardConfig.RegularTiles.Length < 2 || _boardConfig.Width < 4)
                return;

            var configT = _boardConfig.RegularTiles[0];
            var configX = _boardConfig.RegularTiles[1];

            ReInitTileAt(0, 0, configT);
            ReInitTileAt(1, 0, configX);
            ReInitTileAt(2, 0, configT);
            ReInitTileAt(3, 0, configT);
        }

        private async UniTask ProcessScheduledRemovals()
        {
            while (_scheduledRemovals.Count > 0)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_animConfig.ChainReactionWaveDelay));
                var batch = new List<Vector2Int>(_scheduledRemovals);
                _scheduledRemovals.Clear();
                await ProcessRemovals(batch, null);
            }
        }

        private async UniTask ProcessRemovals(List<Vector2Int> positions, Dictionary<Vector2Int, SpecialTileSpawnData> specialPlacements)
        {
            var toDestroy = new List<(Vector2Int pos, Tile tile)>(positions.Count);
            for (var i = 0; i < positions.Count; i++)
            {
                var pos = positions[i];
                if (false == IsValidPosition(pos))
                    continue;

                var tile = _grid[pos.x, pos.y];
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
                _grid[pos.x, pos.y] = null;
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

                if (_grid[pos.x, pos.y])
                    continue;

                var data = kvp.Value;
                var specialTile = _pool.Get();
                specialTile.transform.position = GridToWorld(pos);
                specialTile.Init(data.Config, pos, data.PayloadType);
                _grid[pos.x, pos.y] = specialTile;
                spawnTasks.Add(specialTile.Animator.AnimateSpawn());
            }

            if (spawnTasks.Count > 0)
                await UniTask.WhenAll(spawnTasks);
        }

        private void ReInitTileAt(int x, int y, TileConfig config)
        {
            var tile = _grid[x, y];
            if (tile)
                tile.Init(config, new Vector2Int(x, y));
        }

        private bool WouldCreateMatch(int x, int y, TileType type, TileType[,] assigned)
        {
            if (type == TileType.None)
                return false;

            if (x >= 2 && assigned[x - 1, y] == type && assigned[x - 2, y] == type)
                return true;

            if (y >= 2 && assigned[x, y - 1] == type && assigned[x, y - 2] == type)
                return true;

            return false;
        }

        private TileConfig GetNoMatchConfig(int x, int y, TileType[,] types)
        {
            var forbidden = new HashSet<TileType>();
            if (x >= 2 && types[x - 1, y] == types[x - 2, y] && types[x - 1, y] != TileType.None)
                forbidden.Add(types[x - 1, y]);
            if (y >= 2 && types[x, y - 1] == types[x, y - 2] && types[x, y - 1] != TileType.None)
                forbidden.Add(types[x, y - 1]);

            for (var attempt = 0; attempt < 10; attempt++)
            {
                var config = _boardConfig.RegularTiles[UnityEngine.Random.Range(0, _boardConfig.RegularTiles.Length)];
                if (false == forbidden.Contains(config.Type))
                    return config;
            }

            return _boardConfig.RegularTiles[0];
        }
    }
}
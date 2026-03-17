using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Scripts.Configs;
using Project.Scripts.SpawnRules;
using Project.Scripts.Tiles;
using UnityEngine;

namespace Project.Scripts.Services
{
    public class GridManager : IGridManager
    {
        private readonly BoardConfig _config;
        private readonly TilePool _pool;
        private readonly Tile[,] _grid;
        private readonly List<Vector2Int> _scheduledRemovals = new();
        private Vector3 _origin;


        public GridManager(BoardConfig config, TilePool pool)
        {
            _config = config;
            _pool = pool;
            _grid = new Tile[config.Width, config.Height];
        }


        public UniTask InitAsync() => UniTask.CompletedTask;

        public void SetOrigin(Vector3 origin) => _origin = origin;

        public Tile GetTile(Vector2Int pos) => _grid[pos.x, pos.y];

        public void SetTile(Vector2Int pos, Tile tile) => _grid[pos.x, pos.y] = tile;

        public void ClearTile(Vector2Int pos) => _grid[pos.x, pos.y] = null;

        public bool IsValidPosition(Vector2Int pos) =>
            pos.x >= 0 && pos.x < _config.Width && pos.y >= 0 && pos.y < _config.Height;

        public Vector3 GridToWorld(Vector2Int pos) =>
            _origin + new Vector3(pos.x * _config.CellSize, pos.y * _config.CellSize, 0f);

        public Vector2Int WorldToGrid(Vector3 worldPos)
        {
            var relative = worldPos - _origin;
            
            return new Vector2Int(
                Mathf.RoundToInt(relative.x / _config.CellSize),
                Mathf.RoundToInt(relative.y / _config.CellSize)
            );
        }

        public TileType[,] GetGridState()
        {
            var state = new TileType[_config.Width, _config.Height];
            for (int x = 0; x < _config.Width; x++)
                for (int y = 0; y < _config.Height; y++)
                    state[x, y] = _grid[x, y] ? _grid[x, y].Type : TileType.None;
            
            return state;
        }

        public void ScheduleRemove(List<Vector2Int> positions)
        {
            for (int i = 0; i < positions.Count; i++)
                _scheduledRemovals.Add(positions[i]);
        }

        public TileConfig ResolveNextTile(SpawnContext context)
        {
            if (null != _config.SpawnRules)
                for (int i = 0; i < _config.SpawnRules.Length; i++)
                {
                    var result = _config.SpawnRules[i].TryGetSpecialTile(context);
                    if (result)
                        return result;
                }
            
            return _config.RegularTiles[Random.Range(0, _config.RegularTiles.Length)];
        }

        public List<Vector2Int> GetNeighboursInRadius(Vector2Int center, int radius)
        {
            var result = new List<Vector2Int>();
            for (int dx = -radius; dx <= radius; dx++)
                for (int dy = -radius; dy <= radius; dy++)
                {
                    if (dx == 0 && dy == 0) 
                        continue;
                    
                    var pos = new Vector2Int(center.x + dx, center.y + dy);
                    if (IsValidPosition(pos))
                        result.Add(pos);
                }
            
            return result;
        }

        public async UniTask PopulateGrid()
        {
            var typeCache = new TileType[_config.Width, _config.Height];
            var tasks = new UniTask[_config.Width * _config.Height];
            var idx = 0;
            for (int x = 0; x < _config.Width; x++)
                for (int y = 0; y < _config.Height; y++)
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

        public async UniTask RemoveMatches(List<MatchResult> matches)
        {
            var posSet = new HashSet<Vector2Int>();
            for (int i = 0; i < matches.Count; i++)
                for (int j = 0; j < matches[i].Positions.Count; j++)
                    posSet.Add(matches[i].Positions[j]);

            await ProcessRemovals(new List<Vector2Int>(posSet));

            while (_scheduledRemovals.Count > 0)
            {
                var batch = new List<Vector2Int>(_scheduledRemovals);
                _scheduledRemovals.Clear();
                await ProcessRemovals(batch);
            }
        }

        public async UniTask ActivateBySwap(Vector2Int pos)
        {
            await ProcessRemovals(new List<Vector2Int> { pos });

            while (_scheduledRemovals.Count > 0)
            {
                var batch = new List<Vector2Int>(_scheduledRemovals);
                _scheduledRemovals.Clear();
                await ProcessRemovals(batch);
            }
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
            for (int x = 0; x < _config.Width; x++)
                for (int y = 0; y < _config.Height; y++)
                {
                    var tile = _grid[x, y];
                    if (tile)
                    {
                        positions.Add(new Vector2Int(x, y));
                        configs.Add(tile.Config);
                    }
                }

            for (int i = configs.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (configs[i], configs[j]) = (configs[j], configs[i]);
            }

            var assignedTypes = new TileType[_config.Width, _config.Height];
            for (int i = 0; i < positions.Count; i++)
            {
                var pos = positions[i];
                for (int j = i; j < configs.Count; j++)
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
            for (int i = 0; i < positions.Count; i++)
            {
                var pos = positions[i];
                _grid[pos.x, pos.y].Init(configs[i], pos);
                tasks.Add(_grid[pos.x, pos.y].Animator.AnimateSpawn());
            }
            await UniTask.WhenAll(tasks);
        }

        public void ForceInjectMove()
        {
            if (_config.RegularTiles.Length < 2 || _config.Width < 4)
                return;

            var configT = _config.RegularTiles[0];
            var configX = _config.RegularTiles[1];

            ReInitTileAt(0, 0, configT);
            ReInitTileAt(1, 0, configX);
            ReInitTileAt(2, 0, configT);
            ReInitTileAt(3, 0, configT);
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

            for (int attempt = 0; attempt < 10; attempt++)
            {
                var config = _config.RegularTiles[Random.Range(0, _config.RegularTiles.Length)];
                if (false == forbidden.Contains(config.Type))
                    return config;
            }
            
            return _config.RegularTiles[0];
        }

        private async UniTask ProcessRemovals(List<Vector2Int> positions)
        {
            var toDestroy = new List<(Vector2Int pos, Tile tile)>(positions.Count);
            for (int i = 0; i < positions.Count; i++)
            {
                var pos = positions[i];
                if (false == IsValidPosition(pos)) 
                    continue;
                
                var tile = _grid[pos.x, pos.y];
                if (!tile) 
                    continue;
                
                toDestroy.Add((pos, tile));
            }

            if (toDestroy.Count == 0) 
                return;

            var tasks = new UniTask[toDestroy.Count];
            for (int i = 0; i < toDestroy.Count; i++)
                tasks[i] = toDestroy[i].tile.Animator.AnimateDestroy();
            
            await UniTask.WhenAll(tasks);

            for (int i = 0; i < toDestroy.Count; i++)
            {
                var (pos, tile) = toDestroy[i];
                _grid[pos.x, pos.y] = null;
                tile.Config.Behaviour.OnTileDestroyed(pos, this);
                _pool.Release(tile);
            }
        }
    }
}
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Scripts.Configs;
using Project.Scripts.Services.ServiceLocatorSystem;
using Project.Scripts.SpawnRules;
using Project.Scripts.Tiles;
using UnityEngine;

namespace Project.Scripts.Services
{
    public interface IGridManager : IService
    {
        Tile GetTile(Vector2Int pos);
        void SetTile(Vector2Int pos, Tile tile);
        void ClearTile(Vector2Int pos);
        bool IsValidPosition(Vector2Int pos);
        Vector3 GridToWorld(Vector2Int gridPos);
        Vector2Int WorldToGrid(Vector3 worldPos);
        TileType[,] GetGridState();
        UniTask PopulateGrid();
        UniTask RemoveMatches(List<MatchResult> matches);
        UniTask SwapTiles(Vector2Int from, Vector2Int to);
        List<Vector2Int> GetNeighboursInRadius(Vector2Int center, int radius);
        void ScheduleRemove(List<Vector2Int> positions);
        void SetOrigin(Vector3 origin);
        TileConfig ResolveNextTile(SpawnContext context);
        UniTask ActivateBySwap(Vector2Int pos);
        UniTask ShuffleGrid();
        void ForceInjectMove();
    }
}
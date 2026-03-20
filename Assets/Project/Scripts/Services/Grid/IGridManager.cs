using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Scripts.Configs;
using Project.Scripts.Tiles;
using UnityEngine;

namespace Project.Scripts.Services.Grid
{
    public interface IGridManager
    {
        Tile GetTile(Vector2Int pos);
        void SetTile(Vector2Int pos, Tile tile);
        void ClearTile(Vector2Int pos);
        bool IsValidPosition(Vector2Int pos);
        Vector3 GridToWorld(Vector2Int gridPos);
        Vector2Int WorldToGrid(Vector3 worldPos);
        TileKind[,] GetGridState();
        TileConfig ResolveRegularTile();
        void SetOrigin(Vector3 origin);

        List<Vector2Int> GetNeighboursInRadius(Vector2Int center, int radius);
        List<Vector2Int> GetAllInRow(int y);
        List<Vector2Int> GetAllInColumn(int x);
        List<Vector2Int> GetAllOfKind(TileKind kind);
        List<Vector2Int> GetAllOccupied();
        TileKind GetMostCommonColor();

        void ScheduleRemove(List<Vector2Int> positions);

        UniTask PopulateGrid();
        UniTask SwapTiles(Vector2Int from, Vector2Int to);
        UniTask RemoveMatches(List<MatchResult> matches, Dictionary<Vector2Int, SpecialTileSpawnData> specialPlacements);
        UniTask ActivateBySwap(Vector2Int pos);
        UniTask ActivateTiles(List<Vector2Int> positions);
        UniTask ConsumeTile(Vector2Int pos);
        UniTask ShuffleGrid();
        void ForceInjectMove();

#if UNITY_EDITOR
        void ReplaceForEdit(Vector2Int pos, TileKind kind);
#endif
    }
}
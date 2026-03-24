using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Scripts.Configs;
using Project.Scripts.Shared;
using Project.Scripts.Tiles;
using UnityEngine;

namespace Project.Scripts.Services.Grid
{
    public interface IGridManager
    {
        Tile GetTile(GridPoint pos);
        void SetTile(GridPoint pos, Tile tile);
        void ClearTile(GridPoint pos);
        bool IsValidPosition(GridPoint pos);
        Vector3 GridToWorld(GridPoint gridPos);
        GridPoint WorldToGrid(Vector3 worldPos);
        TileKind[,] GetGridState();
        TileConfig ResolveRegularTile();
        void SetOrigin(Vector3 origin);

        List<GridPoint> GetNeighboursInRadius(GridPoint center, int radius);
        List<GridPoint> GetAllInRow(int y);
        List<GridPoint> GetAllInColumn(int x);
        List<GridPoint> GetAllOfKind(TileKind kind);
        List<GridPoint> GetAllOccupied();
        TileKind GetMostCommonColor();

        void ScheduleRemove(List<GridPoint> positions);

        UniTask PopulateGrid();
        UniTask SwapTiles(GridPoint from, GridPoint to);
        UniTask RemoveMatches(List<MatchResult> matches, Dictionary<GridPoint, SpecialTileSpawnData> specialPlacements);
        UniTask ActivateBySwap(GridPoint pos);
        UniTask ActivateTiles(List<GridPoint> positions);
        UniTask ConsumeTile(GridPoint pos);
        UniTask ShuffleGrid();
        void ForceInjectMove();

#if UNITY_EDITOR
        void ReplaceForEdit(GridPoint pos, TileKind kind);
#endif
    }
}
using Project.Scripts.Services.Grid;
using Project.Scripts.Shared;
using Project.Scripts.Tiles;
using UnityEngine;

namespace Project.Scripts.Behaviours
{
    [CreateAssetMenu(fileName = "StormTileBehaviour", menuName = "Configs/Behaviours/Storm")]
    public class StormTileBehaviour : TileBehaviour
    {
        public override bool IsActivatedBySwap => true;


        public override void OnTileDestroyed(GridPoint gridPos, IGridManager grid)
        {
            var tile = grid.GetTile(gridPos);
            if (false == tile)
                return;

            var targetKind = tile.PayloadKind.IsColor()
                ? tile.PayloadKind
                : grid.GetMostCommonColor();

            if (false == targetKind.IsColor())
                return;

            var positions = grid.GetAllOfKind(targetKind);
            grid.ScheduleRemove(positions);
        }
    }
}
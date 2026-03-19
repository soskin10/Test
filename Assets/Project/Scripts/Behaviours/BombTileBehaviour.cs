using Project.Scripts.Services.Grid;
using UnityEngine;

namespace Project.Scripts.Behaviours
{
    [CreateAssetMenu(fileName = "BombTileBehaviour", menuName = "Configs/Behaviours/Bomb")]
    public class BombTileBehaviour : TileBehaviour
    {
        [Tooltip("Grid radius of the explosion — destroys all tiles within this many cells")]
        [SerializeField] private int _radius = 1;


        public override bool IsActivatedBySwap => true;
        

        public override void OnTileDestroyed(Vector2Int gridPos, IGridManager grid)
        {
            var neighbours = grid.GetNeighboursInRadius(gridPos, _radius);
            grid.ScheduleRemove(neighbours);
        }
    }
}
using Project.Scripts.Services;
using UnityEngine;

namespace Project.Scripts.Behaviours
{
    [CreateAssetMenu(fileName = "BombTileBehaviour", menuName = "Configs/Behaviours/Bomb")]
    public class BombTileBehaviour : TileBehaviour
    {
        [SerializeField] private int _radius = 1;


        public override bool IsActivatedBySwap => true;
        

        public override void OnTileDestroyed(Vector2Int gridPos, IGridManager grid)
        {
            var neighbours = grid.GetNeighboursInRadius(gridPos, _radius);
            grid.ScheduleRemove(neighbours);
        }
    }
}
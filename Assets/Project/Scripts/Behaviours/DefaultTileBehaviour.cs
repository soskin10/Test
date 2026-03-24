using Project.Scripts.Services.Grid;
using Project.Scripts.Shared;
using UnityEngine;

namespace Project.Scripts.Behaviours
{
    [CreateAssetMenu(fileName = "DefaultTileBehaviour", menuName = "Configs/Behaviours/Default")]
    public class DefaultTileBehaviour : TileBehaviour
    {
        public override void OnTileDestroyed(GridPoint gridPos, IGridManager grid) { }
    }
}
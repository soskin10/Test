using Project.Scripts.Services;
using UnityEngine;

namespace Project.Scripts.Behaviours
{
    public abstract class TileBehaviour : ScriptableObject
    {
        public virtual bool IsActivatedBySwap => false;

        
        public abstract void OnTileDestroyed(Vector2Int gridPos, IGridManager grid);
    }
}

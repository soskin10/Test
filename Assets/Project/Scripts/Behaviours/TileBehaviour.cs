using Project.Scripts.Services.Grid;
using Project.Scripts.Shared;
using UnityEngine;

namespace Project.Scripts.Behaviours
{
    public abstract class TileBehaviour : ScriptableObject
    {
        public virtual bool IsActivatedBySwap => false;


        public abstract void OnTileDestroyed(GridPoint gridPos, IGridManager grid);
    }
}
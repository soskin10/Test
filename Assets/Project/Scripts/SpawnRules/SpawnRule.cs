using Project.Scripts.Configs;
using UnityEngine;

namespace Project.Scripts.SpawnRules
{
    public abstract class SpawnRule : ScriptableObject
    {
        public TileConfig TryGetSpecialTile(SpawnContext context)
        {
            if (context.SpecialTileAllocated)
                return null;

            var result = Evaluate(context);
            if (result)
                context.SpecialTileAllocated = true;

            return result;
        }

        protected abstract TileConfig Evaluate(SpawnContext context);
    }
}
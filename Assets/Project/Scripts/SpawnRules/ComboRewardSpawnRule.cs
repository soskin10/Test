using Project.Scripts.Configs;
using UnityEngine;

namespace Project.Scripts.SpawnRules
{
    [CreateAssetMenu(fileName = "ComboRewardSpawnRule", menuName = "Configs/SpawnRules/Combo Reward")]
    public class ComboRewardSpawnRule : SpawnRule
    {
        [SerializeField] private TileConfig _specialTile;
        [SerializeField] private int _minLineLength = 4;


        protected override TileConfig Evaluate(SpawnContext context)
        {
            if (context.MaxLineLength >= _minLineLength || context.HasComplexMatch)
                return _specialTile;

            return null;
        }
    }
}
using Project.Scripts.Configs;
using UnityEngine;

namespace Project.Scripts.SpawnRules
{
    [CreateAssetMenu(fileName = "RandomChanceSpawnRule", menuName = "Configs/SpawnRules/Random Chance")]
    public class RandomChanceSpawnRule : SpawnRule
    {
        [SerializeField] private TileConfig _specialTile;
        [SerializeField] private float _chance = 0.05f;


        protected override TileConfig Evaluate(SpawnContext context)
        {
            if (Random.value <= _chance)
                return _specialTile;

            return null;
        }
    }
}
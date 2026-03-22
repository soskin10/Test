using UnityEngine;

namespace Project.Scripts.Configs
{
    [CreateAssetMenu(fileName = "LevelDatabase", menuName = "Configs/Level Database")]
    public class LevelDatabase : ScriptableObject
    {
        [SerializeField] private LevelConfig[] _levels;


        public LevelConfig[] Levels => _levels;
    }
}

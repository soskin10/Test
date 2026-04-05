using UnityEngine;

namespace Project.Scripts.Configs.Levels
{
    [CreateAssetMenu(fileName = "LevelDatabase", menuName = "Configs/Level Database")]
    public class LevelDatabase : ScriptableObject
    {
        [SerializeField] private LevelConfig[] _levels;


        public LevelConfig[] Levels => _levels;

        
        public LevelConfig GetById(int levelId)
        {
            for (var i = 0; i < _levels.Length; i++)
            {
                var level = _levels[i];
                if (level.LevelId == levelId) return level;
            }

            return _levels[0];
        }

        public int GetNextId(int currentLevelId)
        {
            for (var i = 0; i < _levels.Length; i++)
                if (_levels[i].LevelId == currentLevelId)
                    return _levels[(i + 1) % _levels.Length].LevelId;
            
            return _levels[0].LevelId;
        }
    }
}
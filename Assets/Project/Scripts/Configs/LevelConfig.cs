using UnityEngine;

namespace Project.Scripts.Configs
{
    public enum LevelGoalType
    {
        DamageBased
    }

    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Configs/Level Config")]
    public class LevelConfig : ScriptableObject
    {
        [SerializeField] private int _levelId;

        [Header("Board")]
        [SerializeField] private int _width = 8;
        [SerializeField] private int _height = 8;
        [SerializeField] private TileConfig[] _regularTiles;
        [SerializeField] private TileConfig[] _specialTiles;
        [SerializeField] private int _minMatchLength = 3;

        [Header("Combat")]
        [SerializeField] private int _moveLimit = 30;
        [SerializeField] private int _enemyHP = 100;
        [SerializeField] private LevelGoalType _goalType = LevelGoalType.DamageBased;


        public int LevelId => _levelId;
        public int Width => _width;
        public int Height => _height;
        public TileConfig[] RegularTiles => _regularTiles;
        public TileConfig[] SpecialTiles => _specialTiles;
        public int MinMatchLength => _minMatchLength;
        public int MoveLimit => _moveLimit;
        public int EnemyHP => _enemyHP;
        public LevelGoalType GoalType => _goalType;
    }
}

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
        [Header("Level")]
        [Tooltip("Unique numeric identifier for this level, used by LevelDatabase lookups")]
        [SerializeField] private int _levelId = 1;

        [Header("Board")]
        [Tooltip("Number of tile columns on the board")]
        [SerializeField] private int _width = 8;

        [Tooltip("Number of tile rows on the board")]
        [SerializeField] private int _height = 8;

        [Tooltip("Tile types that can appear as ordinary matches on this board")]
        [SerializeField] private TileConfig[] _regularTiles;

        [Tooltip("Tile types that can appear as special (power-up) tiles on this board")]
        [SerializeField] private TileConfig[] _specialTiles;

        [Header("Combat")]
        [Tooltip("Starting HP of the player avatar for this level")]
        [SerializeField] private int _playerHP = 100;

        [Tooltip("Starting HP of the enemy avatar for this level")]
        [SerializeField] private int _enemyHP = 100;

        [Tooltip("Win condition type - DamageBased means the player wins by reducing enemy HP to zero")]
        [SerializeField] private LevelGoalType _goalType = LevelGoalType.DamageBased;

        // TODO: replace with lobby loadout when PvP is implemented
        [Header("Heroes (temporary - will be replaced by lobby loadout)")]
        [Tooltip("Four hero configs for the player side (null slot = empty)")]
        [SerializeField] private HeroConfig[] _playerHeroes = new HeroConfig[4];

        [Tooltip("Four hero configs for the enemy side; used when BotConfig.RandomHeroSelection is false")]
        [SerializeField] private HeroConfig[] _enemyHeroes = new HeroConfig[4];

        // TODO: replace with matchmaking opponent data when lobby is implemented
        [Header("Bot (temporary - will be replaced by lobby opponent)")]
        [Tooltip("Bot settings for this level; null means no bot (reserved for real PvP)")]
        [SerializeField] private BotConfig _botConfig;


        public int LevelId => _levelId;
        public int Width => _width;
        public int Height => _height;
        public TileConfig[] RegularTiles => _regularTiles;
        public TileConfig[] SpecialTiles => _specialTiles;
        public int PlayerHP => _playerHP;
        public int EnemyHP => _enemyHP;
        public LevelGoalType GoalType => _goalType;
        public HeroConfig[] PlayerHeroes => _playerHeroes;
        public HeroConfig[] EnemyHeroes => _enemyHeroes;
        public BotConfig BotConfig => _botConfig;
    }
}
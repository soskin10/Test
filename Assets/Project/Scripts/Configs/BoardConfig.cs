using Project.Scripts.SpawnRules;
using Project.Scripts.Tiles;
using UnityEngine;

namespace Project.Scripts.Configs
{
    [CreateAssetMenu(fileName = "BoardConfig", menuName = "Configs/Board Config")]
    public class BoardConfig : ScriptableObject
    {
        [Tooltip("Number of columns on the board")]
        [SerializeField] private int _width = 8;
        [Tooltip("Number of rows on the board")]
        [SerializeField] private int _height = 8;
        [Tooltip("Fraction of screen width reserved as padding on each side (0 = no padding, 0.5 = half screen)")]
        [SerializeField] [Range(0f, 0.5f)] private float _boardPaddingPercent = 0.08f;
        [Tooltip("Fraction of screen height reserved for UI (score, buttons, etc.) — board will not grow into this area")]
        [SerializeField] [Range(0f, 0.5f)] private float _uiReservedHeightPercent = 0.4f;
        [Tooltip("Visual size of each tile relative to its cell (1 = fills cell completely, <1 = gaps between tiles)")]
        [SerializeField] [Range(0.5f, 1f)] private float _tileScale = 1f;
        [Tooltip("Extra space in Unity units added around the board for the frame sprite")]
        [SerializeField] private float _framePadding = 0.1f;
        [Tooltip("How many extra tile rows the spawn mask reveals above the board (hides tiles appearing from above)")]
        [SerializeField] private float _maskTopPadding = 2f;
        [Tooltip("Prefab used to instantiate each tile")]
        [SerializeField] private Tile _tilePrefab;
        [Tooltip("Pool of regular tile configurations available for random spawning")]
        [SerializeField] private TileConfig[] _regularTiles;
        [Tooltip("Optional spawn rules that override random selection under specific conditions")]
        [SerializeField] private SpawnRule[] _spawnRules;
        [Tooltip("Minimum number of tiles in a row/column required to count as a match")]
        [SerializeField] private int _minMatchLength = 3;


        public int Width => _width;
        public int Height => _height;
        public float TileScale => _tileScale;
        public float BoardPaddingPercent => _boardPaddingPercent;
        public float UIReservedHeightPercent => _uiReservedHeightPercent;
        public float FramePadding => _framePadding;
        public float MaskTopPadding => _maskTopPadding;
        public Tile TilePrefab => _tilePrefab;
        public TileConfig[] RegularTiles => _regularTiles;
        public SpawnRule[] SpawnRules => _spawnRules;
        public int MinMatchLength => _minMatchLength;
    }
}
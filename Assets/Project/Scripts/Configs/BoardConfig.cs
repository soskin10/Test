using Project.Scripts.SpawnRules;
using Project.Scripts.Tiles;
using UnityEngine;

namespace Project.Scripts.Configs
{
    [CreateAssetMenu(fileName = "BoardConfig", menuName = "Configs/Board Config")]
    public class BoardConfig : ScriptableObject
    {
        [SerializeField] private int _width = 6;
        [SerializeField] private int _height = 6;
        [SerializeField] private float _cellSize = 1f;
        [SerializeField] private Tile _tilePrefab;
        [SerializeField] private TileConfig[] _regularTiles;
        [SerializeField] private SpawnRule[] _spawnRules;
        [SerializeField] private int _minMatchLength = 3;

        
        public int Width => _width;
        public int Height => _height;
        public float CellSize => _cellSize;
        public Tile TilePrefab => _tilePrefab;
        public TileConfig[] RegularTiles => _regularTiles;
        public SpawnRule[] SpawnRules => _spawnRules;
        public int MinMatchLength => _minMatchLength;
    }
}
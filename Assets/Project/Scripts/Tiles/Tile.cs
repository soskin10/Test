using Project.Scripts.Configs;
using UnityEngine;

namespace Project.Scripts.Tiles
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private TileAnimator _animator;
        
        
        public TileType Type { get; private set; }
        public Vector2Int GridPosition { get; set; }
        public TileConfig Config { get; private set; }
        public TileAnimator Animator => _animator;

        
        public void Init(TileConfig config, Vector2Int gridPos)
        {
            Config = config;
            Type = config.Type;
            GridPosition = gridPos;
            _spriteRenderer.sprite = config.Sprite;
        }
    }
}
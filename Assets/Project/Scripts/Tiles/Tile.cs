using Project.Scripts.Configs;
using Project.Scripts.Shared;
using UnityEngine;

namespace Project.Scripts.Tiles
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private TileAnimator _animator;


        public TileKind Kind { get; private set; }
        public GridPoint GridPosition { get; set; }
        public TileConfig Config { get; private set; }
        public TileAnimator Animator => _animator;
        public TileKind PayloadKind { get; private set; }


        public void Init(TileConfig config, GridPoint gridPos, TileKind payloadKind = TileKind.None)
        {
            Config = config;
            Kind = config.Kind;
            GridPosition = gridPos;
            PayloadKind = payloadKind;
            _spriteRenderer.sprite = config.Sprite;
        }

        public void SetPayloadKind(TileKind kind)
        {
            PayloadKind = kind;
        }
    }
}
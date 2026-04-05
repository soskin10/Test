using Project.Scripts.Behaviours;
using Project.Scripts.Shared.Tiles;
using UnityEngine;

namespace Project.Scripts.Configs.Board
{
    [CreateAssetMenu(fileName = "TileConfig", menuName = "Configs/Tile Config")]
    public class TileConfig : ScriptableObject
    {
        [Tooltip("Identity of this tile — determines both match color (Red/Blue/etc.) and special type (Bomb/Storm/etc.)")]
        [SerializeField] private TileKind _kind;

        [Tooltip("Visual sprite displayed for this tile")]
        [SerializeField] private Sprite _sprite;

        [Tooltip("Special behaviour applied when this tile is destroyed")]
        [SerializeField] private TileBehaviour _behaviour;


        public TileKind Kind => _kind;
        public Sprite Sprite => _sprite;
        public TileBehaviour Behaviour => _behaviour;
    }
}
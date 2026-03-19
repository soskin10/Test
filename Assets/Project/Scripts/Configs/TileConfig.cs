using Project.Scripts.Behaviours;
using Project.Scripts.Tiles;
using UnityEngine;

namespace Project.Scripts.Configs
{
    [CreateAssetMenu(fileName = "TileConfig", menuName = "Configs/Tile Config")]
    public class TileConfig : ScriptableObject
    {
        [Tooltip("Element type of this tile, used for matching and damage calculations")]
        [SerializeField] private TileType _type;

        [Tooltip("Visual sprite displayed for this tile")]
        [SerializeField] private Sprite _sprite;

        [Tooltip("Special behaviour applied when this tile is destroyed")]
        [SerializeField] private TileBehaviour _behaviour;

        
        public TileType Type => _type;
        public Sprite Sprite => _sprite;
        public TileBehaviour Behaviour => _behaviour;
    }
}
using Project.Scripts.Behaviours;
using Project.Scripts.Tiles;
using UnityEngine;

namespace Project.Scripts.Configs
{
    [CreateAssetMenu(fileName = "TileConfig", menuName = "Configs/Tile Config")]
    public class TileConfig : ScriptableObject
    {
        [SerializeField] private TileType _type;
        [SerializeField] private Sprite _sprite;
        [SerializeField] private TileBehaviour _behaviour;

        
        public TileType Type => _type;
        public Sprite Sprite => _sprite;
        public TileBehaviour Behaviour => _behaviour;
    }
}
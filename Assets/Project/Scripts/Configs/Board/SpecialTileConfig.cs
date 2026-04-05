using UnityEngine;

namespace Project.Scripts.Configs.Board
{
    [CreateAssetMenu(fileName = "SpecialTileConfig", menuName = "Configs/Special Tile Config")]
    public class SpecialTileConfig : ScriptableObject
    {
        [Tooltip("Rules that define which special tile is created for each match condition, checked in order — first matching rule wins")]
        [SerializeField] private SpecialTileEntry[] _rules;


        public SpecialTileEntry[] Rules => _rules;
    }
}
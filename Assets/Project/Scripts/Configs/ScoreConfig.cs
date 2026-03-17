using UnityEngine;

namespace Project.Scripts.Configs
{
    [CreateAssetMenu(fileName = "ScoreConfig", menuName = "Configs/Score Config")]
    public class ScoreConfig : ScriptableObject
    {
        [Tooltip("Base points awarded per destroyed tile in any match")]
        [SerializeField] private int _pointsPerTile = 20;

        [Tooltip("Bonus points per tile based on match length. Index = match length (index 3 = 3-match, 4 = 4-match, etc.)")]
        [SerializeField] private int[] _bonusPerTileByLength = { 0, 0, 0, 0, 20, 40, 40 };

        [Tooltip("Flat bonus added for complex (L/T shaped) matches")]
        [SerializeField] private int _complexMatchBonus = 100;

        [Tooltip("Multiplier increase per cascade level. Cascade 1 = x1.0, cascade 2 = x(1.0 + step), etc.")]
        [SerializeField] private float _cascadeMultiplierStep = 0.5f;

        [Tooltip("Points awarded per tile destroyed by a bomb")]
        [SerializeField] private int _bombKillPointsPerTile = 10;


        public int PointsPerTile => _pointsPerTile;
        public int[] BonusPerTileByLength => _bonusPerTileByLength;
        public int ComplexMatchBonus => _complexMatchBonus;
        public float CascadeMultiplierStep => _cascadeMultiplierStep;
        public int BombKillPointsPerTile => _bombKillPointsPerTile;
    }
}
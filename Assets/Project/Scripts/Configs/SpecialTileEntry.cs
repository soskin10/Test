using System;
using Project.Scripts.Tiles;
using UnityEngine;

namespace Project.Scripts.Configs
{
    [Serializable]
    public class SpecialTileEntry
    {
        [Tooltip("Match condition that triggers creation of this special tile")]
        [SerializeField] private SpecialTileCondition _condition;

        [Tooltip("Kind of special tile to spawn when the condition is met")]
        [SerializeField] private TileKind _tileKind;

        [Tooltip("Where on the board the special tile appears: at the center of the match shape, or at the swap pivot position")]
        [SerializeField] private SpecialTileSpawnPosition _spawnPosition;


        public SpecialTileCondition Condition => _condition;
        public TileKind TileKind => _tileKind;
        public SpecialTileSpawnPosition SpawnPosition => _spawnPosition;
    }
}

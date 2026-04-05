using Project.Scripts.Tiles;
using UnityEngine;

namespace Project.Scripts.Configs.Board
{
    [CreateAssetMenu(fileName = "BoardConfig", menuName = "Configs/Board Config")]
    public class BoardConfig : ScriptableObject
    {
        [Tooltip("Fraction of screen width reserved as padding on each side (0 = no padding, 0.5 = half screen)")]
        [SerializeField] [Range(0f, 0.5f)] private float _boardPaddingPercent = 0.08f;

        [Tooltip("Fraction of screen height reserved for UI (score, buttons, etc.) — board will not grow into this area")]
        [SerializeField] [Range(0f, 0.5f)] private float _uiReservedHeightPercent = 0.4f;

        [Tooltip("Visual size of each tile relative to its cell (1 = fills cell completely, <1 = gaps between tiles)")]
        [SerializeField] [Range(0.5f, 1f)] private float _tileScale = 0.85f;

        [Tooltip("Extra space in Unity units added around the board for the frame sprite")]
        [SerializeField] private float _framePadding = 0.1f;

        [Tooltip("How many extra tile rows the spawn mask reveals above the board (hides tiles appearing from above)")]
        [SerializeField] private float _maskTopPadding = 2f;

        [Tooltip("Fixed gap between the bottom of the screen and the bottom board edge, expressed in cell units (1 = one tile height). Consistent across all screen sizes.")]
        [SerializeField] [Range(0f, 5f)] private float _boardBottomPaddingCells = 0.65f;

        [Tooltip("Minimum number of tiles in a row/column to count as a match")]
        [SerializeField] [Range(2, 6)] private int _minMatchLength = 3;

        [Tooltip("Maximum width-to-height ratio for the game area. On wider screens the content is pillarboxed to this ratio (0.5 = 1:2)")]
        [SerializeField] [Range(0.3f, 1f)] private float _maxAspectRatio = 0.5f;

        [Tooltip("Prefab used to instantiate each tile")]
        [SerializeField] private Tile _tilePrefab;


        public float BoardPaddingPercent => _boardPaddingPercent;
        public float UIReservedHeightPercent => _uiReservedHeightPercent;
        public float TileScale => _tileScale;
        public float FramePadding => _framePadding;
        public float MaskTopPadding => _maskTopPadding;
        public float BoardBottomPaddingCells => _boardBottomPaddingCells;
        public int MinMatchLength => _minMatchLength;
        public float MaxAspectRatio => _maxAspectRatio;
        public Tile TilePrefab => _tilePrefab;
    }
}
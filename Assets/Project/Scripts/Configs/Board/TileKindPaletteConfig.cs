using System;
using Project.Scripts.Shared.Tiles;
using UnityEngine;

namespace Project.Scripts.Configs.Board
{
    [CreateAssetMenu(fileName = "TileKindPaletteConfig", menuName = "Configs/TileKindPalette Config")]
    public class TileKindPaletteConfig : ScriptableObject
    {
        [Tooltip("Color mapping for each tile element type")]
        [SerializeField] private TileKindColorEntry[] _entries;


        public Color GetColor(TileKind kind, Color fallback = default)
        {
            for (var i = 0; i < _entries.Length; i++)
                if (_entries[i].Kind == kind)
                    return _entries[i].Color;

            return fallback;
        }
    }
    
    
    [Serializable]
    public struct TileKindColorEntry
    {
        [Tooltip("Tile element type")]
        public TileKind Kind;

        [Tooltip("Color associated with this element type — used to tint hero slot backgrounds")]
        public Color Color;
    }
}
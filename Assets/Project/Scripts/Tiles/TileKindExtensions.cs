namespace Project.Scripts.Tiles
{
    public static class TileKindExtensions
    {
        public static bool IsColor(this TileKind kind) =>
            kind is >= TileKind.Fire and <= TileKind.Void;

        public static bool IsSpecial(this TileKind kind) =>
            kind >= TileKind.Bomb;
    }
}
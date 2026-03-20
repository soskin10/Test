#if UNITY_EDITOR
using System;
using Project.Scripts.Tiles;

namespace Project.Scripts.Services.BoardEdit
{
    public static class BoardEditMode
    {
        public static bool IsActive { get; private set; }
        public static TileKind SelectedKind { get; set; } = TileKind.Red;

        public static event Action<bool> OnToggled;


        public static void SetActive(bool active)
        {
            if (IsActive == active)
                return;

            IsActive = active;
            OnToggled?.Invoke(active);
        }
    }
}
#endif

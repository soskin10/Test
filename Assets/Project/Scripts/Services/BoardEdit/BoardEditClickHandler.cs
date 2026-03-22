#if UNITY_EDITOR
using DG.Tweening;
using Project.Scripts.Configs;
using Project.Scripts.Services.Grid;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Scripts.Services.BoardEdit
{
    public class BoardEditClickHandler : MonoBehaviour
    {
        private IGridManager _grid;
        private LevelConfig _levelConfig;
        private float _cellSize;
        private GameObject _overlayGo;
        private LineRenderer _overlay;


        public void Init(IGridManager grid, LevelConfig levelConfig, float cellSize)
        {
            _grid = grid;
            _levelConfig = levelConfig;
            _cellSize = cellSize;
            BoardEditMode.OnToggled += OnEditModeToggled;
        }


        private void OnDestroy()
        {
            BoardEditMode.OnToggled -= OnEditModeToggled;
            DestroyOverlay();
        }


        private void Update()
        {
            if (false == BoardEditMode.IsActive)
                return;

            if (null == Mouse.current)
                return;

            if (false == Mouse.current.leftButton.wasPressedThisFrame)
                return;

            if (null == _grid)
                return;

            var screenPos = Mouse.current.position.ReadValue();
            var worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(Camera.main.transform.position.z)));
            var gridPos = _grid.WorldToGrid(worldPos);

            if (false == _grid.IsValidPosition(gridPos))
                return;

            if (false == _grid.GetTile(gridPos))
                return;

            _grid.ReplaceForEdit(gridPos, BoardEditMode.SelectedKind);
        }


        private void OnEditModeToggled(bool active)
        {
            if (active)
                CreateOverlay();
            else
                DestroyOverlay();
        }

        private void CreateOverlay()
        {
            var half = _cellSize * 0.5f;
            var bl = _grid.GridToWorld(new Vector2Int(0, 0)) + new Vector3(-half, -half, 0f);
            var br = _grid.GridToWorld(new Vector2Int(_levelConfig.Width - 1, 0)) + new Vector3( half, -half, 0f);
            var tr = _grid.GridToWorld(new Vector2Int(_levelConfig.Width - 1, _levelConfig.Height - 1)) + new Vector3( half,  half, 0f);
            var tl = _grid.GridToWorld(new Vector2Int(0, _levelConfig.Height - 1)) + new Vector3(-half,  half, 0f);

            _overlayGo = new GameObject("BoardEditOverlay");
            _overlay = _overlayGo.AddComponent<LineRenderer>();
            _overlay.positionCount = 4;
            _overlay.loop = true;
            _overlay.useWorldSpace = true;
            _overlay.SetPositions(new[] { bl, br, tr, tl });
            _overlay.startWidth = _overlay.endWidth = _cellSize * 0.06f;
            _overlay.material = new Material(Shader.Find("Sprites/Default"));
            _overlay.sortingOrder = 100;

            var baseColor = new Color(0.2f, 1f, 0.4f, 0.7f);
            var fadeColor = new Color(0.2f, 1f, 0.4f, 0.25f);
            _overlay.startColor = _overlay.endColor = baseColor;

            DOTween.To(
                () => _overlay.startColor,
                c => { _overlay.startColor = c; _overlay.endColor = c; },
                fadeColor,
                0.7f
            ).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }

        private void DestroyOverlay()
        {
            if (false == _overlayGo)
                return;

            DOTween.Kill(_overlay);
            Destroy(_overlayGo);
            _overlayGo = null;
            _overlay = null;
        }
    }
}
#endif
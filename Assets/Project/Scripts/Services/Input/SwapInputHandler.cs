using System;
using Cysharp.Threading.Tasks;
using Project.Scripts.Services.Grid;
using UnityEngine;

namespace Project.Scripts.Services.Input
{
    public class SwapInputHandler : ISwapInputHandler, IDisposable
    {
        public event Action<SwapRequest> OnSwapRequested;


        private readonly IInputService _input;
        private readonly IGridManager _grid;
        private readonly float _worldThreshold;
        private Camera _camera;
        private Vector2Int _startGridPos;
        private bool _hasPendingSwap;


        public SwapInputHandler(IInputService input, IGridManager grid, float worldThreshold)
        {
            _input = input;
            _grid = grid;
            _worldThreshold = worldThreshold;
        }

        public UniTask InitAsync()
        {
            _camera = Camera.main;
            _input.OnDragStarted += HandleDragStarted;
            _input.OnDragDelta += HandleDragDelta;
            _input.OnDragCanceled += HandleDragCanceled;

            return UniTask.CompletedTask;
        }

        public void Dispose()
        {
            _input.OnDragStarted -= HandleDragStarted;
            _input.OnDragDelta -= HandleDragDelta;
            _input.OnDragCanceled -= HandleDragCanceled;
        }


        private void HandleDragStarted(Vector2 screenPos)
        {
            var worldPos = ScreenToWorld(screenPos);
            _startGridPos = _grid.WorldToGrid(worldPos);
            _hasPendingSwap = _grid.IsValidPosition(_startGridPos) && _grid.GetTile(_startGridPos) != null;
        }

        private void HandleDragDelta(Vector2 screenDelta)
        {
            if (false == _hasPendingSwap)
                return;

            var worldDelta = ScreenDeltaToWorld(screenDelta);
            if (worldDelta.magnitude < _worldThreshold)
                return;

            _hasPendingSwap = false;
            var dir = GetDirection(worldDelta);
            var target = _startGridPos + dir;
            if (false == _grid.IsValidPosition(target))
                return;

            if (false == _grid.GetTile(target))
                return;

            OnSwapRequested?.Invoke(new SwapRequest(_startGridPos, target));
        }

        private void HandleDragCanceled()
        {
            _hasPendingSwap = false;
        }

        private Vector3 ScreenToWorld(Vector2 screenPos)
        {
            var z = Mathf.Abs(_camera.transform.position.z);

            return _camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, z));
        }

        private Vector2 ScreenDeltaToWorld(Vector2 screenDelta)
        {
            var z = Mathf.Abs(_camera.transform.position.z);
            var origin = _camera.ScreenToWorldPoint(new Vector3(0, 0, z));
            var target = _camera.ScreenToWorldPoint(new Vector3(screenDelta.x, screenDelta.y, z));

            return new Vector2(target.x - origin.x, target.y - origin.y);
        }

        private static Vector2Int GetDirection(Vector2 worldDelta)
        {
            if (Mathf.Abs(worldDelta.x) >= Mathf.Abs(worldDelta.y))
                return worldDelta.x > 0 ? Vector2Int.right : Vector2Int.left;

            return worldDelta.y > 0 ? Vector2Int.up : Vector2Int.down;
        }
    }
}
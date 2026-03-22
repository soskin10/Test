using Project.Scripts.Configs;
using UnityEngine;

namespace Project.Scripts.Gameplay
{
    [ExecuteAlways]
    public class BoardPositioner : MonoBehaviour
    {
        [SerializeField] private BoardConfig _boardConfig;
        [SerializeField] private LevelConfig _levelConfig;


        private void Update()
        {
            if (Application.isPlaying)
                return;

            Apply();
        }

        public void Apply(float cellSize = -1f)
        {
            var cam = Camera.main;
            if (!cam || !_boardConfig || !_levelConfig)
                return;

            if (cellSize < 0f)
                cellSize = ComputeCellSize(cam);

            transform.position = ComputeBoardCenter(cam, cellSize);

            var boardView = GetComponent<BoardView>();
            if (boardView)
                boardView.Setup(_levelConfig.Width, _levelConfig.Height, cellSize,
                    _boardConfig.FramePadding, _boardConfig.MaskTopPadding);
        }


        private Vector3 ComputeBoardCenter(Camera cam, float cellSize)
        {
            var camHeight = cam.orthographicSize * 2f;
            var camBottomY = cam.transform.position.y - cam.orthographicSize;
            var boardHeight = _levelConfig.Height * cellSize;
            var bottomPadding = camHeight * _boardConfig.BoardBottomPaddingPercent;

            return new Vector3(
                cam.transform.position.x,
                camBottomY + bottomPadding + boardHeight * 0.5f,
                0f
            );
        }

        private float ComputeCellSize(Camera cam)
        {
            var camHeight = cam.orthographicSize * 2f;
            var camWidth = camHeight * GetAspect(cam);

            var byWidth = camWidth * (1f - _boardConfig.BoardPaddingPercent) / _levelConfig.Width;
            var byHeight = camHeight * (1f - _boardConfig.UIReservedHeightPercent) / _levelConfig.Height;

            return Mathf.Min(byWidth, byHeight);
        }

        private static float GetAspect(Camera cam)
        {
            var h = UnityEngine.Device.Screen.height;
            return h > 0
                ? (float)UnityEngine.Device.Screen.width / h
                : cam.aspect;
        }
    }
}

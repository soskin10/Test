using Cysharp.Threading.Tasks;
using Project.Scripts.Configs;
using Project.Scripts.Services;
using Project.Scripts.Services.Audio;
using Project.Scripts.Services.Audio.AudioSystem;
using Project.Scripts.Services.EventBusSystem;
using Project.Scripts.UI;
using UnityEngine;
using VContainer;

namespace Project.Scripts.Gameplay
{
    public class GameplayEntryPoint : MonoBehaviour
    {
        [Tooltip("Parent transform for all instantiated tile objects")]
        [SerializeField] private Transform _tileContainer;
        [Tooltip("HUD view that displays the current score")]
        [SerializeField] private ScoreHUDView _scoreHUDView;
        [Tooltip("View component that sizes the board frame and spawn mask at runtime")]
        [SerializeField] private BoardView _boardView;


        private EventBus _eventBus;
        private AudioService _audioService;
        private BoardConfig _boardConfig;
        private AnimationConfig _animConfig;
        private InputConfig _inputConfig;
        private ScoreConfig _scoreConfig;

        private InputService _inputService;
        private SwapInputHandler _swapHandler;
        private ScoreService _scoreService;
        private GameAudioController _gameAudioController;


        public void OnDestroy()
        {
            _swapHandler?.Dispose();
            _inputService?.Dispose();
            _scoreService?.Dispose();
        }


        private void Start()
        {
            InitAsync().Forget();
        }


        [Inject]
        public void Construct(
            EventBus eventBus,
            AudioService audioService,
            BoardConfig boardConfig,
            AnimationConfig animConfig,
            InputConfig inputConfig,
            ScoreConfig scoreConfig)
        {
            _eventBus = eventBus;
            _audioService = audioService;
            _boardConfig = boardConfig;
            _animConfig = animConfig;
            _inputConfig = inputConfig;
            _scoreConfig = scoreConfig;
        }


        private async UniTaskVoid InitAsync()
        {
            var cellSize = ComputeCellSize(_boardConfig);
            var pool = new TilePool(_boardConfig.TilePrefab, _tileContainer, _animConfig, cellSize, _boardConfig.TileScale);
            var matchFinder = new MatchFinder(_boardConfig.MinMatchLength);
            var gridManager = new GridManager(_boardConfig, pool, cellSize);
            gridManager.SetOrigin(ComputeGridOrigin(_boardConfig, cellSize));

            _boardView.Setup(_boardConfig.Width, _boardConfig.Height, cellSize,
                _boardConfig.FramePadding, _boardConfig.MaskTopPadding);

            var gravityHandler = new GravityHandler(gridManager, pool, _boardConfig);

            _inputService = new InputService(_inputConfig);
            _swapHandler = new SwapInputHandler(_inputService, gridManager, _inputConfig.WorldDragThreshold);

            var moveChecker = new MoveChecker(gridManager, matchFinder, _boardConfig);

            _scoreService = new ScoreService(_scoreConfig);
            _scoreHUDView.Bind(_scoreService);

            var orchestrator = new BoardOrchestrator(
                _eventBus,
                gridManager,
                gravityHandler,
                matchFinder,
                _swapHandler,
                moveChecker,
                _scoreService);

            _gameAudioController = new GameAudioController(_audioService, _eventBus);
            _gameAudioController.StartMusic();

            await _inputService.InitAsync();
            await _swapHandler.InitAsync();
            await orchestrator.InitAsync();
            await orchestrator.StartGame();
        }

        private float ComputeCellSize(BoardConfig config)
        {
            var cam = Camera.main;
            var camHeight = cam.orthographicSize * 2f;
            var camWidth = camHeight * cam.aspect;

            var cellSizeByWidth = camWidth * (1f - config.BoardPaddingPercent) / config.Width;
            var cellSizeByHeight = camHeight * (1f - config.UIReservedHeightPercent) / config.Height;

            return Mathf.Min(cellSizeByWidth, cellSizeByHeight);
        }

        private Vector3 ComputeGridOrigin(BoardConfig config, float cellSize)
        {
            var offsetX = -(config.Width - 1) * cellSize * 0.5f;
            var offsetY = -(config.Height - 1) * cellSize * 0.5f;

            return transform.position + new Vector3(offsetX, offsetY, 0f);
        }
    }
}
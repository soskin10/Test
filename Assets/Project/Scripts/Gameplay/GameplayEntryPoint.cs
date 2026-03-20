using Cysharp.Threading.Tasks;
using Project.Scripts.Configs;
using Project.Scripts.Services;
using Project.Scripts.Services.Audio;
using Project.Scripts.Services.Audio.AudioSystem;
using Project.Scripts.Services.Damage;
using Project.Scripts.Services.EventBusSystem;
using Project.Scripts.Services.Grid;
using Project.Scripts.Services.Input;
using UnityEngine;
using VContainer;
#if UNITY_EDITOR
using Project.Scripts.Services.BoardEdit;
#endif

namespace Project.Scripts.Gameplay
{
    public class GameplayEntryPoint : MonoBehaviour
    {
        [Tooltip("Parent transform for all instantiated tile objects")]
        [SerializeField] private Transform _tileContainer;

        [Tooltip("View component that sizes the board frame and spawn mask at runtime")]
        [SerializeField] private BoardView _boardView;


        private EventBus _eventBus;
        private AudioService _audioService;
        private BoardConfig _boardConfig;
        private AnimationConfig _animConfig;
        private InputConfig _inputConfig;
        private IDamageCalculator _damageCalculator;
        private SpecialTileConfig _specialTileConfig;
        private InputService _inputService;
        private SwapInputHandler _swapHandler;
        private GameStateService _gameStateService;
        private GameAudioController _gameAudioController;

        
        private void Start()
        {
            InitAsync().Forget();
        }
        
        private void OnDestroy()
        {
            _swapHandler?.Dispose();
            _inputService?.Dispose();
            _gameStateService?.Dispose();
        }


        [Inject]
        public void Construct(
            EventBus eventBus,
            AudioService audioService,
            BoardConfig boardConfig,
            AnimationConfig animConfig,
            InputConfig inputConfig,
            IDamageCalculator damageCalculator,
            SpecialTileConfig specialTileConfig)
        {
            _eventBus = eventBus;
            _audioService = audioService;
            _boardConfig = boardConfig;
            _animConfig = animConfig;
            _inputConfig = inputConfig;
            _damageCalculator = damageCalculator;
            _specialTileConfig = specialTileConfig;
        }


        private async UniTaskVoid InitAsync()
        {
            var cellSize = ComputeCellSize(_boardConfig);
            var pool = new TilePool(_boardConfig.TilePrefab, _tileContainer, _animConfig, cellSize, _boardConfig.TileScale);
            var matchFinder = new MatchFinder(_boardConfig.MinMatchLength);
            var gridManager = new GridManager(_boardConfig, _animConfig, pool, cellSize);
            gridManager.SetOrigin(ComputeGridOrigin(_boardConfig, cellSize));

            _boardView.Setup(_boardConfig.Width, _boardConfig.Height, cellSize,
                _boardConfig.FramePadding, _boardConfig.MaskTopPadding);

            var gravityHandler = new GravityHandler(gridManager, pool, _boardConfig);

            _inputService = new InputService(_inputConfig);
            _swapHandler = new SwapInputHandler(_inputService, gridManager, _inputConfig.WorldDragThreshold);

            var moveChecker = new MoveChecker(gridManager, matchFinder, _boardConfig);
            var specialTileResolver = new SpecialTileResolver(_specialTileConfig);
            var swapComboResolver = new SwapComboResolver();

            _gameStateService = new GameStateService();

            var orchestrator = new BoardOrchestrator(
                _eventBus,
                gridManager,
                gravityHandler,
                matchFinder,
                _swapHandler,
                moveChecker,
                _damageCalculator,
                _gameStateService,
                specialTileResolver,
                swapComboResolver);

            _gameAudioController = new GameAudioController(_audioService, _eventBus);
            _gameAudioController.StartMusic();

#if UNITY_EDITOR
            var editHandler = gameObject.AddComponent<BoardEditClickHandler>();
            editHandler.Init(gridManager, _boardConfig, cellSize);
#endif

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
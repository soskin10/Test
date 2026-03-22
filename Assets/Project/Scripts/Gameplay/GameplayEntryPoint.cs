using Cysharp.Threading.Tasks;
using Project.Scripts.Configs;
using Project.Scripts.Gameplay.UI;
using Project.Scripts.Services;
using Project.Scripts.Services.Audio;
using Project.Scripts.Services.Audio.AudioSystem;
using Project.Scripts.Services.Combat;
using Project.Scripts.Services.Damage;
using Project.Scripts.Services.EventBusSystem;
using Project.Scripts.Services.Grid;
using Project.Scripts.Services.Input;
using Project.Scripts.Services.UISystem;
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

        [Tooltip("Prefab containing the GameplayView component — instantiated into the main canvas on start")]
        [SerializeField] private GameObject _gameplayViewPrefab;


        private EventBus _eventBus;
        private AudioService _audioService;
        private BoardConfig _boardConfig;
        private LevelConfig _levelConfig;
        private AnimationConfig _animConfig;
        private InputConfig _inputConfig;
        private IDamageCalculator _damageCalculator;
        private SpecialTileConfig _specialTileConfig;
        private UIService _uiService;
        private GameplayViewModel _gameplayViewModel;
        private InputService _inputService;
        private SwapInputHandler _swapHandler;
        private GameStateService _gameStateService;
        private MoveCounterService _moveCounterService;
        private EnemyStateService _enemyStateService;
        private GameAudioController _gameAudioController;


        private void Start()
        {
            InitAsync().Forget();
        }

        private void OnDestroy()
        {
            _uiService?.Close<GameplayView>();
            _swapHandler?.Dispose();
            _inputService?.Dispose();
            _moveCounterService?.Dispose();
            _enemyStateService?.Dispose();
            _gameStateService?.Dispose();
        }


        [Inject]
        public void Construct(
            EventBus eventBus,
            AudioService audioService,
            BoardConfig boardConfig,
            LevelConfig levelConfig,
            AnimationConfig animConfig,
            InputConfig inputConfig,
            IDamageCalculator damageCalculator,
            SpecialTileConfig specialTileConfig,
            UIService uiService,
            GameplayViewModel gameplayViewModel)
        {
            _eventBus = eventBus;
            _audioService = audioService;
            _boardConfig = boardConfig;
            _levelConfig = levelConfig;
            _animConfig = animConfig;
            _inputConfig = inputConfig;
            _damageCalculator = damageCalculator;
            _specialTileConfig = specialTileConfig;
            _uiService = uiService;
            _gameplayViewModel = gameplayViewModel;
        }


        private async UniTaskVoid InitAsync()
        {
            _uiService.RegisterView<GameplayView>(_gameplayViewPrefab, UILayer.Main);
            await _uiService.Show<GameplayView, GameplayViewModel>(_gameplayViewModel);

            var cellSize = ComputeCellSize();
            var boardCenter = ComputeBoardCenter(cellSize);
            _boardView.transform.position = boardCenter;

            var pool = new TilePool(_boardConfig.TilePrefab, _tileContainer, _animConfig, cellSize, _boardConfig.TileScale);
            var matchFinder = new MatchFinder(_levelConfig.MinMatchLength);
            var gridManager = new GridManager(_levelConfig, _animConfig, pool, cellSize);
            gridManager.SetOrigin(ComputeGridOrigin(boardCenter, cellSize));

            _boardView.Setup(_levelConfig.Width, _levelConfig.Height, cellSize,
                _boardConfig.FramePadding, _boardConfig.MaskTopPadding);

            var gravityHandler = new GravityHandler(gridManager, pool, _levelConfig);

            _inputService = new InputService(_inputConfig);
            _swapHandler = new SwapInputHandler(_inputService, gridManager, _inputConfig.WorldDragThreshold);

            var moveChecker = new MoveChecker(gridManager, matchFinder, _levelConfig);
            var specialTileResolver = new SpecialTileResolver(_specialTileConfig, _levelConfig);
            var swapComboResolver = new SwapComboResolver();

            _moveCounterService = new MoveCounterService(_eventBus, _levelConfig);
            _enemyStateService = new EnemyStateService(_eventBus, _levelConfig);
            _gameStateService = new GameStateService(_eventBus);

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
            editHandler.Init(gridManager, _levelConfig, cellSize);
#endif

            await _inputService.InitAsync();
            await _swapHandler.InitAsync();
            await orchestrator.InitAsync();
            await orchestrator.StartGame();
        }

        private float ComputeCellSize()
        {
            var cam = Camera.main;
            var camHeight = cam.orthographicSize * 2f;
            var camWidth = camHeight * cam.aspect;

            var cellSizeByWidth = camWidth * (1f - _boardConfig.BoardPaddingPercent) / _levelConfig.Width;
            var cellSizeByHeight = camHeight * (1f - _boardConfig.UIReservedHeightPercent) / _levelConfig.Height;

            return Mathf.Min(cellSizeByWidth, cellSizeByHeight);
        }

        private Vector3 ComputeBoardCenter(float cellSize)
        {
            var cam = Camera.main;
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

        private Vector3 ComputeGridOrigin(Vector3 boardCenter, float cellSize)
        {
            return boardCenter + new Vector3(
                -(_levelConfig.Width - 1) * cellSize * 0.5f,
                -(_levelConfig.Height - 1) * cellSize * 0.5f,
                0f
            );
        }
    }
}
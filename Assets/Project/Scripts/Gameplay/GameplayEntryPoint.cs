using Cysharp.Threading.Tasks;
using Project.Scripts.Configs;
using Project.Scripts.Configs.Battle;
using Project.Scripts.Configs.Board;
using Project.Scripts.Configs.Levels;
using Project.Scripts.Configs.UI;
using Project.Scripts.Gameplay.UI;
using Project.Scripts.Gameplay.WorldSpace;
using Project.Scripts.Services;
using Project.Scripts.Services.Audio;
using Project.Scripts.Services.Combat;
using Project.Scripts.Shared.Rules;
using Project.Scripts.Services.Audio.AudioSystem;
using Project.Scripts.Services.Damage;
using Project.Scripts.Services.EventBusSystem;
using Project.Scripts.Services.Grid;
using Project.Scripts.Services.Input;
using Project.Scripts.Services.UISystem;
using Project.Scripts.Shared.Grid;
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
        private LevelConfig _levelConfig;
        private BoardAnimationConfig _animConfig;
        private InputConfig _inputConfig;
        private IDamageCalculator _damageCalculator;
        private SpecialTileConfig _specialTileConfig;
        private UIConfig _uiConfig;
        private BattleViewConfig _battleViewConfig;
        private UIService _uiService;
        private MoveBarViewModel _moveBarViewModel;
        private IGameStateService _gameStateService;
        private IMoveBarService _moveBarService;
        private GameResultPresenter _gameResultPresenter;
        private BattleHUDViewModel _battleHUDViewModel;
        private IBoardBoundsProvider _boardBoundsProvider;
        private WorldBattleHUDView _worldBattleHUDView;
        private InputService _inputService;
        private SwapInputHandler _swapHandler;
        private BoardOrchestrator _orchestrator;
        private GameAudioController _gameAudioController;

#if UNITY_EDITOR
        private GridManager _gridManager;
        private float _cellSize;
        private int _lastWidth;
        private int _lastHeight;
#endif


        private void Start()
        {
            InitAsync().Forget();
        }

        private void Update()
        {
            if (null == _moveBarService)
                return;

            if (false == _gameStateService.IsPlaying)
                return;

            _moveBarService.Tick(Time.deltaTime);

#if UNITY_EDITOR
            if (_gridManager == null)
                return;

            if (Screen.width == _lastWidth && Screen.height == _lastHeight)
                return;

            _lastWidth = Screen.width;
            _lastHeight = Screen.height;
            ApplyLiveResize();
#endif
        }

        private void OnDestroy()
        {
            _uiService?.Close<MoveBarView>();
            _uiService?.Close<TopBarView>();
            if (_worldBattleHUDView)
                _worldBattleHUDView.Close();
            _orchestrator?.Dispose();
            _swapHandler?.Dispose();
            _inputService?.Dispose();
        }


        [Inject]
        public void Construct(
            EventBus eventBus,
            AudioService audioService,
            BoardConfig boardConfig,
            LevelConfig levelConfig,
            BoardAnimationConfig animConfig,
            InputConfig inputConfig,
            IDamageCalculator damageCalculator,
            SpecialTileConfig specialTileConfig,
            UIConfig uiConfig,
            BattleViewConfig battleViewConfig,
            UIService uiService,
            MoveBarViewModel moveBarViewModel,
            IGameStateService gameStateService,
            IMoveBarService moveBarService,
            GameResultPresenter gameResultPresenter,
            BattleHUDViewModel battleHUDViewModel,
            IBoardBoundsProvider boardBoundsProvider)
        {
            _eventBus = eventBus;
            _audioService = audioService;
            _boardConfig = boardConfig;
            _levelConfig = levelConfig;
            _animConfig = animConfig;
            _inputConfig = inputConfig;
            _damageCalculator = damageCalculator;
            _specialTileConfig = specialTileConfig;
            _uiConfig = uiConfig;
            _battleViewConfig = battleViewConfig;
            _uiService = uiService;
            _moveBarViewModel = moveBarViewModel;
            _gameStateService = gameStateService;
            _moveBarService = moveBarService;
            _gameResultPresenter = gameResultPresenter;
            _battleHUDViewModel = battleHUDViewModel;
            _boardBoundsProvider = boardBoundsProvider;
        }


        private async UniTaskVoid InitAsync()
        {
            _moveBarService.Initialize();

            _uiService.RegisterView<MoveBarView>(_uiConfig.MoveBarViewPrefab, UILayer.MainDynamic);
            _uiService.RegisterView<TopBarView>(_uiConfig.TopBarViewPrefab, UILayer.Main);

            await _uiService.Show<MoveBarView, MoveBarViewModel>(_moveBarViewModel);

            var cellSize = ComputeCellSize();
            var boardCenter = ComputeBoardCenter(cellSize);
            _boardView.transform.position = boardCenter;

            var boardTopWorldY = boardCenter.y + _levelConfig.Height * cellSize * 0.5f;
            var boardHalfWidth = _levelConfig.Width * cellSize * 0.5f;
            _boardBoundsProvider.SetBounds(boardCenter.x, boardTopWorldY, boardHalfWidth, cellSize);

            await _uiService.Show<TopBarView, BattleHUDViewModel>(_battleHUDViewModel);

            // InputService is created here so WorldBattleHUDView can subscribe to its events.
            // The input map is enabled later via InitAsync, so no input fires until then.
            _inputService = new InputService(_inputConfig);

            var hudGo = Instantiate(_battleViewConfig.BattleHUDViewPrefab);
            _worldBattleHUDView = hudGo.GetComponent<WorldBattleHUDView>();
            _worldBattleHUDView.SetDependencies(_inputService, _battleViewConfig);
            await _worldBattleHUDView.InitializeAsync(_battleHUDViewModel);
            await _worldBattleHUDView.ShowAsync();

            var pool = new TilePool(_boardConfig.TilePrefab, _tileContainer, _animConfig, cellSize, _boardConfig.TileScale);
            var matchFinder = new MatchFinder(_boardConfig.MinMatchLength);
            var gridManager = new GridManager(_levelConfig, _animConfig, pool, cellSize);
            gridManager.SetOrigin(ComputeGridOrigin(boardCenter, cellSize));

#if UNITY_EDITOR
            _gridManager = gridManager;
            _cellSize = cellSize;
            _lastWidth = Screen.width;
            _lastHeight = Screen.height;
#endif

            _boardView.Setup(_levelConfig.Width, _levelConfig.Height, cellSize,
                _boardConfig.FramePadding, _boardConfig.MaskTopPadding);

            var gravityHandler = new GravityHandler(gridManager.State, gridManager, pool, _levelConfig);

            _swapHandler = new SwapInputHandler(_inputService, gridManager.State, gridManager, _inputConfig.WorldDragThreshold);

            var moveChecker = new MoveChecker(gridManager.State, gridManager, matchFinder, _levelConfig);
            var specialTileResolver = new SpecialTileResolver(_specialTileConfig, _levelConfig);
            var swapComboResolver = new SwapComboResolver();

            _orchestrator = new BoardOrchestrator(
                _eventBus,
                gridManager.State,
                gridManager,
                gridManager,
                gravityHandler,
                matchFinder,
                _swapHandler,
                moveChecker,
                _damageCalculator,
                _gameStateService,
                _moveBarService,
                specialTileResolver,
                swapComboResolver);

            _gameAudioController = new GameAudioController(_audioService, _eventBus, _gameStateService);
            _gameAudioController.StartMusic();

            _gameResultPresenter.Initialize();

#if UNITY_EDITOR
            var editHandler = gameObject.AddComponent<BoardEditClickHandler>();
            editHandler.Init(gridManager.State, gridManager, _levelConfig, cellSize);
#endif

            await _inputService.InitAsync();
            await _swapHandler.InitAsync();
            await _orchestrator.InitAsync();
            await _orchestrator.StartGame();
        }

#if UNITY_EDITOR
        private void ApplyLiveResize()
        {
            var boardCenter = ComputeBoardCenter(_cellSize);
            _boardView.transform.position = boardCenter;

            var newOrigin = ComputeGridOrigin(boardCenter, _cellSize);
            _gridManager.SetOrigin(newOrigin);
            _gridManager.RepositionAllTiles();

            var boardTopWorldY = boardCenter.y + _levelConfig.Height * _cellSize * 0.5f;
            var boardHalfWidth = _levelConfig.Width * _cellSize * 0.5f;
            _boardBoundsProvider.SetBounds(boardCenter.x, boardTopWorldY, boardHalfWidth, _cellSize);
        }
#endif

        private float ComputeCellSize()
        {
            var cam = Camera.main;
            var camHeight = cam.orthographicSize * 2f;
            var camWidth = camHeight * cam.aspect;
            var effectiveWidth = Mathf.Min(camWidth, camHeight * _boardConfig.MaxAspectRatio);

            var cellSizeByWidth = effectiveWidth * (1f - _boardConfig.BoardPaddingPercent) / _levelConfig.Width;
            var cellSizeByHeight = camHeight * (1f - _boardConfig.UIReservedHeightPercent) / _levelConfig.Height;

            return Mathf.Min(cellSizeByWidth, cellSizeByHeight);
        }

        private Vector3 ComputeBoardCenter(float cellSize)
        {
            var cam = Camera.main;
            var camBottomY = cam.transform.position.y - cam.orthographicSize;
            var boardHeight = _levelConfig.Height * cellSize;
            var bottomPadding = _boardConfig.BoardBottomPaddingCells * cellSize;

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
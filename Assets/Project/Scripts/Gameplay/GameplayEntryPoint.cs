using Cysharp.Threading.Tasks;
using Project.Scripts.Configs;
using Project.Scripts.Services;
using Project.Scripts.Services.Audio;
using Project.Scripts.Services.Audio.AudioSystem;
using Project.Scripts.Services.EventBusSystem;
using Project.Scripts.Services.ServiceLocatorSystem;
using Project.Scripts.UI;
using UnityEngine;

namespace Project.Scripts.Gameplay
{
    public class GameplayEntryPoint : MonoBehaviour
    {
        [SerializeField] private Transform _tileContainer;
        [SerializeField] private ScoreHUDView _scoreHUDView;


        private InputService _inputService;
        private SwapInputHandler _swapHandler;
        private ScoreService _scoreService;
        private GameAudioController _gameAudioController;


        private void Start()
        {
            InitAsync().Forget();
        }

        private async UniTaskVoid InitAsync()
        {
            var eventBus = ServiceLocator.Get<EventBus>();
            var audioService = ServiceLocator.Get<AudioService>();
            var mainConfig = ServiceLocator.Get<MainConfig>();
            var boardConfig = mainConfig.BoardConfig;
            var animConfig = mainConfig.AnimationConfig;
            var inputConfig = mainConfig.InputConfig;
            var scoreConfig = mainConfig.ScoreConfig;

            var pool = new TilePool(boardConfig.TilePrefab, _tileContainer, animConfig);
            var matchFinder = new MatchFinder(boardConfig.MinMatchLength);
            var gridManager = new GridManager(boardConfig, pool);
            gridManager.SetOrigin(ComputeGridOrigin(boardConfig));

            var gravityHandler = new GravityHandler(gridManager, pool, boardConfig);

            _inputService = new InputService(inputConfig);
            _swapHandler = new SwapInputHandler(_inputService, gridManager, inputConfig.WorldDragThreshold);

            var moveChecker = new MoveChecker(gridManager, matchFinder, boardConfig);

            _scoreService = new ScoreService(scoreConfig);
            _scoreHUDView.Bind(_scoreService);

            var orchestrator = new BoardOrchestrator(
                eventBus,
                gridManager, 
                gravityHandler, 
                matchFinder, 
                _swapHandler, 
                moveChecker, 
                _scoreService);
            
            _gameAudioController = new GameAudioController(audioService, eventBus);
            _gameAudioController.StartMusic(); 

            await _inputService.InitAsync();
            await _swapHandler.InitAsync();
            await orchestrator.InitAsync();
            await orchestrator.StartGame();
        }

        private void OnDestroy()
        {
            _swapHandler?.Dispose();
            _inputService?.Dispose();
            _scoreService?.Dispose();
        }

        private Vector3 ComputeGridOrigin(BoardConfig config)
        {
            var offsetX = -(config.Width - 1) * config.CellSize * 0.5f;
            var offsetY = -(config.Height - 1) * config.CellSize * 0.5f;

            return transform.position + new Vector3(offsetX, offsetY, 0f);
        }
    }
}
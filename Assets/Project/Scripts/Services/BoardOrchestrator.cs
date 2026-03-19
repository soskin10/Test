using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Scripts.Services.Damage;
using Project.Scripts.Services.EventBusSystem;
using Project.Scripts.Services.EventBusSystem.Events;
using Project.Scripts.Services.Grid;
using Project.Scripts.Services.Input;
using Project.Scripts.SpawnRules;
using Project.Scripts.Tiles;
using UnityEngine;

namespace Project.Scripts.Services
{
    public class BoardOrchestrator : IBoardOrchestrator
    {
        private const int ShuffleMaxAttempts = 10;


        private readonly IGridManager _grid;
        private readonly IGravityHandler _gravity;
        private readonly IMatchFinder _matchFinder;
        private readonly ISwapInputHandler _swapHandler;
        private readonly IMoveChecker _moveChecker;
        private readonly IDamageCalculator _damageCalculator;
        private readonly IGameStateService _gameStateService;
        private readonly EventBus _eventBus;
        private bool _isProcessing;


        public BoardOrchestrator(EventBus eventBus, IGridManager grid, IGravityHandler gravity,
            IMatchFinder matchFinder, ISwapInputHandler swapHandler, IMoveChecker moveChecker,
            IDamageCalculator damageCalculator, IGameStateService gameStateService)
        {
            _eventBus = eventBus;
            _grid = grid;
            _gravity = gravity;
            _matchFinder = matchFinder;
            _swapHandler = swapHandler;
            _moveChecker = moveChecker;
            _damageCalculator = damageCalculator;
            _gameStateService = gameStateService;
        }

        public UniTask InitAsync()
        {
            _swapHandler.OnSwapRequested += OnSwapRequested;

            return UniTask.CompletedTask;
        }

        public async UniTask StartGame()
        {
            await _grid.PopulateGrid();
        }


        private void OnSwapRequested(SwapRequest request)
        {
            if (_isProcessing)
                return;

            if (false == _gameStateService.IsPlaying)
                return;

            HandleSwapAsync(request).Forget();
        }

        private async UniTask HandleSwapAsync(SwapRequest request)
        {
            if (!_grid.GetTile(request.From) || !_grid.GetTile(request.To))
                return;

            _isProcessing = true;
            try
            {
                await _grid.SwapTiles(request.From, request.To);

                var tileAtTo = _grid.GetTile(request.To);
                var tileAtFrom = _grid.GetTile(request.From);

                var waves = new List<WaveBreakdown>();
                int bombDamage = 0;

                bool bombActivated = false;
                bool anyBomb = (tileAtTo && tileAtTo.Config.Behaviour.IsActivatedBySwap) ||
                               (tileAtFrom && tileAtFrom.Config.Behaviour.IsActivatedBySwap);
                int tilesBefore = anyBomb ? CountActiveTiles(_grid.GetGridState()) : 0;

                if (tileAtTo && tileAtTo.Config.Behaviour.IsActivatedBySwap)
                {
                    await _grid.ActivateBySwap(request.To);
                    bombActivated = true;
                }

                if (tileAtFrom && tileAtFrom.Config.Behaviour.IsActivatedBySwap)
                {
                    await _grid.ActivateBySwap(request.From);
                    bombActivated = true;
                }

                if (bombActivated)
                {
                    _eventBus.Publish(new BombActivatedEvent());
                    int tilesAfter = CountActiveTiles(_grid.GetGridState());
                    bombDamage = _damageCalculator.CalculateBombDamage(tilesBefore - tilesAfter);

                    await _gravity.ApplyGravity();
                    await _gravity.SpawnNewTiles(new SpawnContext());
                    var chainMatches = _matchFinder.FindMatches(_grid.GetGridState());
                    if (chainMatches.Count > 0)
                        await ProcessMatchChain(chainMatches, waves);
                    await EnsureMovesAvailable();
                }
                else
                {
                    var gridState = _grid.GetGridState();
                    var matches = _matchFinder.FindMatches(gridState);

                    if (matches.Count == 0)
                        await _grid.SwapTiles(request.To, request.From);
                    else
                        await ProcessMatchChain(matches, waves);
                }

                if (waves.Count > 0 || bombDamage > 0)
                    Debug.Log(new DamageBreakdown(waves, bombDamage).ToLogString());
            }
            finally
            {
                _isProcessing = false;
            }
        }

        private async UniTask ProcessMatchChain(List<MatchResult> matches, List<WaveBreakdown> waves)
        {
            int cascadeLevel = 1;
            while (matches.Count > 0)
            {
                _eventBus.Publish(new MatchPlayedEvent(cascadeLevel));
                waves.Add(_damageCalculator.CalculateWave(matches, cascadeLevel));
                cascadeLevel++;

                var context = BuildContext(matches);
                await _grid.RemoveMatches(matches);
                await _gravity.ApplyGravity();
                await _gravity.SpawnNewTiles(context);
                matches = _matchFinder.FindMatches(_grid.GetGridState());
            }

            await EnsureMovesAvailable();
        }

        private async UniTask EnsureMovesAvailable()
        {
            if (_moveChecker.HasPossibleMoves())
                return;

            for (int i = 0; i < ShuffleMaxAttempts; i++)
            {
                await _grid.ShuffleGrid();
                if (_moveChecker.HasPossibleMoves())
                    return;
            }

            _grid.ForceInjectMove();

            var immediateMatches = _matchFinder.FindMatches(_grid.GetGridState());
            if (immediateMatches.Count > 0)
                await ProcessMatchChain(immediateMatches, new List<WaveBreakdown>());
        }

        private static SpawnContext BuildContext(List<MatchResult> matches)
        {
            int maxLen = 0;
            int total = 0;
            bool hasComplex = false;
            for (int i = 0; i < matches.Count; i++)
            {
                if (matches[i].MaxLineLength > maxLen)
                    maxLen = matches[i].MaxLineLength;
                if (matches[i].IsComplex)
                    hasComplex = true;
                total += matches[i].Positions.Count;
            }

            return new SpawnContext
            {
                MaxLineLength = maxLen,
                HasComplexMatch = hasComplex,
                TotalDestroyed = total
            };
        }

        private static int CountActiveTiles(TileType[,] state)
        {
            int count = 0;
            foreach (var type in state)
                if (type != TileType.None) 
                    count++;

            return count;
        }
    }
}
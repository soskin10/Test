using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Scripts.Behaviours;
using Project.Scripts.Services.Damage;
using Project.Scripts.Services.EventBusSystem;
using Project.Scripts.Services.EventBusSystem.Events;
using Project.Scripts.Services.Grid;
using Project.Scripts.Services.Input;
using Project.Scripts.Shared;
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
        private readonly SpecialTileResolver _specialTileResolver;
        private readonly SwapComboResolver _swapComboResolver;
        private bool _isProcessing;


        public BoardOrchestrator(EventBus eventBus, IGridManager grid, IGravityHandler gravity,
            IMatchFinder matchFinder, ISwapInputHandler swapHandler, IMoveChecker moveChecker,
            IDamageCalculator damageCalculator, IGameStateService gameStateService,
            SpecialTileResolver specialTileResolver, SwapComboResolver swapComboResolver)
        {
            _eventBus = eventBus;
            _grid = grid;
            _gravity = gravity;
            _matchFinder = matchFinder;
            _swapHandler = swapHandler;
            _moveChecker = moveChecker;
            _damageCalculator = damageCalculator;
            _gameStateService = gameStateService;
            _specialTileResolver = specialTileResolver;
            _swapComboResolver = swapComboResolver;
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
            var fromTile = _grid.GetTile(request.From);
            var toTile = _grid.GetTile(request.To);

            if (false == fromTile || false == toTile)
                return;

            _isProcessing = true;
            try
            {
                var fromKind = fromTile.Config.Kind;
                var toKind = toTile.Config.Kind;
                var fromIsSpecial = fromKind.IsSpecial();
                var toIsSpecial = toKind.IsSpecial();

                await _grid.SwapTiles(request.From, request.To);

                var waves = new List<WaveBreakdown>();
                var energyByKind = new Dictionary<TileKind, int>();
                var bombDamage = 0;
                var moveUsed = false;

                if (fromIsSpecial && toIsSpecial)
                {
                    var stateBefore = _grid.GetGridState();
                    var tilesBefore = CountActiveTiles(stateBefore);

                    await ExecuteSwapCombo(fromKind, toKind, request.To, request.From, fromTile, toTile);

                    var stateAfter = _grid.GetGridState();
                    _eventBus.Publish(new BombActivatedEvent());
                    bombDamage = _damageCalculator.CalculateBombDamage(tilesBefore - CountActiveTiles(stateAfter));
                    AccumulateGridDiffEnergy(stateBefore, stateAfter, energyByKind);

                    await RunPostActivationFlow(waves, request.PivotPosition);
                    moveUsed = true;
                }
                else if (fromIsSpecial || toIsSpecial)
                {
                    Tile specialTile, partnerTile;
                    GridPoint specialFinalPos;

                    if (fromIsSpecial)
                    {
                        specialTile = fromTile;
                        specialFinalPos = request.To;
                        partnerTile = toTile;
                    }
                    else
                    {
                        specialTile = toTile;
                        specialFinalPos = request.From;
                        partnerTile = fromTile;
                    }

                    var stateBefore = _grid.GetGridState();
                    var tilesBefore = CountActiveTiles(stateBefore);

                    await ActivateSpecialWithPartner(specialTile, partnerTile, specialFinalPos);

                    var stateAfter = _grid.GetGridState();
                    _eventBus.Publish(new BombActivatedEvent());
                    bombDamage = _damageCalculator.CalculateBombDamage(tilesBefore - CountActiveTiles(stateAfter));
                    AccumulateGridDiffEnergy(stateBefore, stateAfter, energyByKind);

                    await RunPostActivationFlow(waves, request.PivotPosition);
                    moveUsed = true;
                }
                else
                {
                    var matches = _matchFinder.FindMatches(_grid.GetGridState());

                    if (matches.Count == 0)
                        await _grid.SwapTiles(request.To, request.From);
                    else
                    {
                        await ProcessMatchChain(matches, waves, request.PivotPosition, true);
                        moveUsed = true;
                    }
                }

                AccumulateMatchEnergy(waves, energyByKind);

                if (waves.Count > 0 || bombDamage > 0)
                {
                    var breakdown = new DamageBreakdown(waves, bombDamage);
                    _eventBus.Publish(new DamageDealtEvent(breakdown.Total));
                    Debug.Log(breakdown.ToLogString());
                }

                if (energyByKind.Count > 0)
                {
                    _eventBus.Publish(new EnergyGeneratedEvent(energyByKind));
                    Debug.Log(BuildEnergyLogString(energyByKind));
                }

                if (moveUsed)
                    _eventBus.Publish(new MoveUsedEvent());
            }
            finally
            {
                _isProcessing = false;
            }
        }

        private async UniTask ActivateSpecialWithPartner(Tile specialTile, Tile partnerTile, GridPoint specialFinalPos)
        {
            if (specialTile.Config.Kind == TileKind.Storm)
                specialTile.SetPayloadKind(partnerTile.Kind);

            await _grid.ActivateBySwap(specialFinalPos);
        }

        private async UniTask ExecuteSwapCombo(TileKind kindA, TileKind kindB,
            GridPoint posA, GridPoint posB, Tile tileA, Tile tileB)
        {
            var comboType = _swapComboResolver.Resolve(kindA, kindB);

            switch (comboType)
            {
                case SwapComboType.StormStorm:
                    await ExecuteStormStormCombo(posA, posB);
                    break;

                case SwapComboType.StormBomb:
                {
                    var stormPos = kindA == TileKind.Storm ? posA : posB;
                    await ExecuteStormBombCombo(stormPos);
                    break;
                }

                case SwapComboType.StormLine:
                {
                    var stormPos = kindA == TileKind.Storm ? posA : posB;
                    await ExecuteStormLineCombo(stormPos);
                    break;
                }

                case SwapComboType.BombBomb:
                {
                    var doubleRadius = GetBombDoubleRadius(tileA, tileB);
                    await ExecuteBombBombCombo(posA, posB, doubleRadius);
                    break;
                }

                case SwapComboType.BombLine:
                {
                    var bombPos = kindA == TileKind.Bomb ? posA : posB;
                    var bombTile = kindA == TileKind.Bomb ? tileA : tileB;
                    var radius = GetBombRadius(bombTile);
                    await ExecuteBombLineCombo(posA, posB, bombPos, radius);
                    break;
                }

                case SwapComboType.LineLine:
                    await ExecuteLineLineCombo(posA, posB);
                    break;
            }
        }

        private async UniTask ExecuteStormStormCombo(GridPoint posA, GridPoint posB)
        {
            await UniTask.WhenAll(_grid.ConsumeTile(posA), _grid.ConsumeTile(posB));
            var allPositions = _grid.GetAllOccupied();
            await _grid.ActivateTiles(allPositions);
        }

        private async UniTask ExecuteStormBombCombo(GridPoint stormPos)
        {
            await _grid.ConsumeTile(stormPos);
            var bombPositions = _grid.GetAllOfKind(TileKind.Bomb);
            if (bombPositions.Count == 0)
                return;

            await _grid.ActivateTiles(bombPositions);
        }

        private async UniTask ExecuteStormLineCombo(GridPoint stormPos)
        {
            await _grid.ConsumeTile(stormPos);
            var linePositions = _grid.GetAllOfKind(TileKind.LineRuneH);
            linePositions.AddRange(_grid.GetAllOfKind(TileKind.LineRuneV));
            if (linePositions.Count == 0)
                return;

            await _grid.ActivateTiles(linePositions);
        }

        private async UniTask ExecuteBombBombCombo(GridPoint posA, GridPoint posB, int doubleRadius)
        {
            await UniTask.WhenAll(_grid.ConsumeTile(posA), _grid.ConsumeTile(posB));

            var explosion = new HashSet<GridPoint>(_grid.GetNeighboursInRadius(posA, doubleRadius));
            var ps = _grid.GetNeighboursInRadius(posB, doubleRadius);
            for (var i = 0; i < ps.Count; i++)
                explosion.Add(ps[i]);

            await _grid.ActivateTiles(new List<GridPoint>(explosion));
        }

        private async UniTask ExecuteBombLineCombo(GridPoint posA, GridPoint posB,
            GridPoint bombPos, int bombRadius)
        {
            await UniTask.WhenAll(_grid.ConsumeTile(posA), _grid.ConsumeTile(posB));

            var area = new HashSet<GridPoint>(_grid.GetNeighboursInRadius(bombPos, bombRadius));
            var row = _grid.GetAllInRow(bombPos.Y);
            for (var i = 0; i < row.Count; i++)
                area.Add(row[i]);

            var ps = _grid.GetAllInColumn(bombPos.X);
            for (var i = 0; i < ps.Count; i++)
                area.Add(ps[i]);

            await _grid.ActivateTiles(new List<GridPoint>(area));
        }

        private async UniTask ExecuteLineLineCombo(GridPoint posA, GridPoint posB)
        {
            await _grid.ActivateTiles(new List<GridPoint> { posA, posB });
        }

        private async UniTask RunPostActivationFlow(List<WaveBreakdown> waves, GridPoint pivotPosition)
        {
            await _gravity.ApplyGravity();
            await _gravity.SpawnNewTiles();
            var chainMatches = _matchFinder.FindMatches(_grid.GetGridState());
            if (chainMatches.Count > 0)
                await ProcessMatchChain(chainMatches, waves, pivotPosition, false);

            await EnsureMovesAvailable();
        }

        private async UniTask ProcessMatchChain(List<MatchResult> matches, List<WaveBreakdown> waves, GridPoint pivotPosition, bool spawnSpecials)
        {
            while (matches.Count > 0)
            {
                var cascadeLevel = waves.Count + 1;
                _eventBus.Publish(new MatchPlayedEvent(cascadeLevel));
                waves.Add(_damageCalculator.CalculateWave(matches, cascadeLevel));

                var specialPlacements = spawnSpecials && cascadeLevel == 1
                    ? _specialTileResolver.Resolve(matches, pivotPosition) : null;
                await _grid.RemoveMatches(matches, specialPlacements);
                await _gravity.ApplyGravity();
                await _gravity.SpawnNewTiles();
                matches = _matchFinder.FindMatches(_grid.GetGridState());
            }

            await EnsureMovesAvailable();
        }

        private async UniTask EnsureMovesAvailable()
        {
            if (_moveChecker.HasPossibleMoves())
                return;

            for (var i = 0; i < ShuffleMaxAttempts; i++)
            {
                await _grid.ShuffleGrid();
                if (_moveChecker.HasPossibleMoves())
                    return;
            }

            _grid.ForceInjectMove();

            var immediateMatches = _matchFinder.FindMatches(_grid.GetGridState());
            if (immediateMatches.Count > 0)
                await ProcessMatchChain(immediateMatches, new List<WaveBreakdown>(), GridPoint.Zero, false);
        }

        private static int CountActiveTiles(TileKind[,] state)
        {
            var count = 0;
            foreach (var kind in state)
                if (kind != TileKind.None)
                    count++;

            return count;
        }

        private static int GetBombRadius(Tile tile)
        {
            return (tile.Config.Behaviour as BombTileBehaviour)?.Radius ?? 1;
        }

        private static int GetBombDoubleRadius(Tile tileA, Tile tileB)
        {
            var bomb = tileA.Config.Behaviour as BombTileBehaviour
                    ?? tileB.Config.Behaviour as BombTileBehaviour;

            return bomb?.DoubleRadius ?? 2;
        }

        private static void AccumulateMatchEnergy(List<WaveBreakdown> waves, Dictionary<TileKind, int> energy)
        {
            for (var i = 0; i < waves.Count; i++)
            {
                var matches = waves[i].Matches;
                for (var j = 0; j < matches.Count; j++)
                {
                    var match = matches[j];
                    if (false == match.TileKind.IsColor())
                        continue;

                    energy.TryGetValue(match.TileKind, out var current);
                    energy[match.TileKind] = current + match.TileCount;
                }
            }
        }

        private static string BuildEnergyLogString(Dictionary<TileKind, int> energyByKind)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("[Energy] Move result:");

            var total = 0;
            foreach (var pair in energyByKind)
            {
                if (pair.Value <= 0)
                    continue;

                sb.AppendLine($"  {pair.Key}: +{pair.Value}");
                total += pair.Value;
            }

            sb.Append($"  Total: +{total}");
            return sb.ToString();
        }

        private static void AccumulateGridDiffEnergy(TileKind[,] before, TileKind[,] after, Dictionary<TileKind, int> energy)
        {
            var width = before.GetLength(0);
            var height = before.GetLength(1);

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var kindBefore = before[x, y];
                    if (false == kindBefore.IsColor())
                        continue;

                    if (after[x, y] != kindBefore)
                    {
                        energy.TryGetValue(kindBefore, out var current);
                        energy[kindBefore] = current + 1;
                    }
                }
            }
        }
    }
}
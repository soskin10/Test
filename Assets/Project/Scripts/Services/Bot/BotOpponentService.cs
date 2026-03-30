using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Project.Scripts.Configs;
using Project.Scripts.Services.Combat;
using Project.Scripts.Services.EventBusSystem;
using Project.Scripts.Services.EventBusSystem.Events;
using Project.Scripts.Shared.Bot;
using Project.Scripts.Shared.Heroes;
using Project.Scripts.Shared.Moves;
using R3;
using UnityEngine;
using VContainer.Unity;

namespace Project.Scripts.Services.Bot
{
    public class BotOpponentService : IBotOpponentService, IStartable, IDisposable
    {
        private readonly EventBus _eventBus;
        private readonly IHeroService _heroService;
        private readonly IGameStateService _gameStateService;
        private readonly BotConfig _botConfig;
        private readonly MoveBarConfig _moveBarConfig;

        private BotDecisionEngine _engine;
        private readonly MoveBarEngine _botMoveBar = new MoveBarEngine();
        private CancellationTokenSource _cts;
        private IDisposable _stateSub;
        private readonly bool[] _heroActivationPending = new bool[4];


        public BotOpponentService(
            EventBus eventBus,
            IHeroService heroService,
            IGameStateService gameStateService,
            BotConfig botConfig,
            MoveBarConfig moveBarConfig)
        {
            _eventBus = eventBus;
            _heroService = heroService;
            _gameStateService = gameStateService;
            _botConfig = botConfig;
            _moveBarConfig = moveBarConfig;
        }


        public void Start()
        {
            if (false == _botConfig.Enabled)
                return;

            var settings = _botConfig.ToSettings();
            _engine = new BotDecisionEngine(settings, UnityEngine.Random.Range(0, int.MaxValue));

            _botMoveBar.Initialize(_moveBarConfig.ToSettings());

            if (_botConfig.RandomHeroSelection && _botConfig.HeroPool?.Length > 0)
                _heroService.AssignEnemyHeroes(PickRandomHeroes(_botConfig.HeroPool, 4));

            _cts = new CancellationTokenSource();

            _stateSub = _gameStateService.State
                .Where(s => s != GameState.Playing)
                .Take(1)
                .Subscribe(_ => StopLoops());

            RunMoveBarTickLoop(_cts.Token).Forget();
            RunAttackLoop(_cts.Token).Forget();
            RunHeroEnergyLoop(_cts.Token).Forget();
        }

        public void Dispose()
        {
            StopLoops();
            _stateSub?.Dispose();
            _stateSub = null;
        }


        private async UniTaskVoid RunMoveBarTickLoop(CancellationToken ct)
        {
            const float tickInterval = 0.25f;
            var lastTime = Time.realtimeSinceStartup;

            while (false == ct.IsCancellationRequested)
            {
                var cancelled = await UniTask
                    .Delay(TimeSpan.FromSeconds(tickInterval), cancellationToken: ct)
                    .SuppressCancellationThrow();

                if (cancelled)
                    return;

                var now = Time.realtimeSinceStartup;
                _botMoveBar.Tick(now - lastTime);
                lastTime = now;
            }
        }

        private async UniTaskVoid RunAttackLoop(CancellationToken ct)
        {
            while (false == ct.IsCancellationRequested)
            {
                var delay = _engine.NextAttackDelay();
                var cancelled = await UniTask
                    .Delay(TimeSpan.FromSeconds(delay), cancellationToken: ct)
                    .SuppressCancellationThrow();

                if (cancelled || false == _gameStateService.IsPlaying)
                    return;

                while (false == _botMoveBar.TryConsume())
                {
                    var moveCancelled = await UniTask
                        .Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: ct)
                        .SuppressCancellationThrow();

                    if (moveCancelled || false == _gameStateService.IsPlaying)
                        return;
                }

                var damage = _engine.GenerateAttackDamage();
                _eventBus.Publish(new EnemyAttackEvent(damage));
            }
        }

        private async UniTaskVoid RunHeroEnergyLoop(CancellationToken ct)
        {
            while (false == ct.IsCancellationRequested)
            {
                var cancelled = await UniTask
                    .Delay(TimeSpan.FromSeconds(_botConfig.HeroEnergyTickInterval), cancellationToken: ct)
                    .SuppressCancellationThrow();

                if (cancelled || false == _gameStateService.IsPlaying)
                    return;

                var slots = _heroService.GetSlots(BattleSide.Enemy);
                var slotIndex = _engine.PickRandomAssignedSlot(slots);

                if (slotIndex < 0)
                    continue;

                _heroService.AddEnemyHeroEnergy(slotIndex, _botConfig.HeroEnergyPerTick);
                
                if (_heroService.GetSlots(BattleSide.Enemy)[slotIndex].IsReady
                    && false == _heroActivationPending[slotIndex])
                {
                    _heroActivationPending[slotIndex] = true;
                    ActivateWithDelay(slotIndex, ct).Forget();
                }
            }
        }

        private async UniTaskVoid ActivateWithDelay(int slotIndex, CancellationToken ct)
        {
            var delay = _engine.GenerateHeroActivationDelay(
                _botConfig.MinHeroActivationDelay,
                _botConfig.MaxHeroActivationDelay);

            var cancelled = await UniTask
                .Delay(TimeSpan.FromSeconds(delay), cancellationToken: ct)
                .SuppressCancellationThrow();

            _heroActivationPending[slotIndex] = false;

            if (cancelled || false == _gameStateService.IsPlaying)
                return;

            _heroService.TryActivate(BattleSide.Enemy, slotIndex);
        }

        private void StopLoops()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private static HeroConfig[] PickRandomHeroes(HeroConfig[] pool, int count)
        {
            var result = new HeroConfig[count];
            var available = new List<HeroConfig>(pool);

            for (var i = 0; i < count && available.Count > 0; i++)
            {
                var idx = UnityEngine.Random.Range(0, available.Count);
                result[i] = available[idx];
                available.RemoveAt(idx);
            }

            return result;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Project.Scripts.Configs;
using Project.Scripts.Services.Combat;
using Project.Scripts.Services.EventBusSystem;
using Project.Scripts.Shared.Bot;
using Project.Scripts.Shared.Heroes;
using R3;
using VContainer.Unity;

namespace Project.Scripts.Services.Bot
{
    public class BotOpponentService : IBotOpponentService, IStartable, IDisposable
    {
        private readonly EventBus _eventBus;
        private readonly IHeroService _heroService;
        private readonly IGameStateService _gameStateService;
        private readonly IEnemyAvatarChargeService _enemyChargeService;
        private readonly BotConfig _botConfig;

        private BotDecisionEngine _engine;
        private CancellationTokenSource _cts;
        private IDisposable _stateSub;
        private readonly bool[] _heroActivationPending = new bool[4];
        private bool _dischargeScheduled;


        public BotOpponentService(
            EventBus eventBus,
            IHeroService heroService,
            IGameStateService gameStateService,
            IEnemyAvatarChargeService enemyChargeService,
            BotConfig botConfig)
        {
            _eventBus = eventBus;
            _heroService = heroService;
            _gameStateService = gameStateService;
            _enemyChargeService = enemyChargeService;
            _botConfig = botConfig;
        }


        public void Start()
        {
            if (false == _botConfig.Enabled)
                return;

            _engine = new BotDecisionEngine(_botConfig.ToSettings(), UnityEngine.Random.Range(0, int.MaxValue));

            if (_botConfig.RandomHeroSelection && _botConfig.HeroPool?.Length > 0)
                _heroService.AssignEnemyHeroes(PickRandomHeroes(_botConfig.HeroPool, 4));

            _cts = new CancellationTokenSource();

            _stateSub = _gameStateService.State
                .Where(s => s != GameState.Playing)
                .Take(1)
                .Subscribe(_ => StopLoops());

            RunEnemyChargeLoop(_cts.Token).Forget();
            RunHeroEnergyLoop(_cts.Token).Forget();
        }

        public void Dispose()
        {
            StopLoops();
            _stateSub?.Dispose();
            _stateSub = null;
        }


        private async UniTaskVoid RunEnemyChargeLoop(CancellationToken ct)
        {
            while (false == ct.IsCancellationRequested)
            {
                var cancelled = await UniTask
                    .Delay(TimeSpan.FromSeconds(_botConfig.EnemyChargeTickInterval), cancellationToken: ct)
                    .SuppressCancellationThrow();

                if (cancelled || false == _gameStateService.IsPlaying)
                    return;

                _enemyChargeService.AddCharge(_botConfig.EnemyChargePerTick);

                if (_enemyChargeService.IsFull && false == _dischargeScheduled)
                {
                    _dischargeScheduled = true;
                    ScheduleDischarge(ct).Forget();
                }
            }
        }

        private async UniTaskVoid ScheduleDischarge(CancellationToken ct)
        {
            var delay = _engine.GenerateDischargeDelay();

            var cancelled = await UniTask
                .Delay(TimeSpan.FromSeconds(delay), cancellationToken: ct)
                .SuppressCancellationThrow();

            _dischargeScheduled = false;

            if (cancelled || false == _gameStateService.IsPlaying)
                return;

            _enemyChargeService.TriggerDischarge();
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
                var pickedIndex = _engine.PickRandomAssignedSlot(slots);

                if (pickedIndex < 0)
                    continue;

                var kind = slots[pickedIndex].Kind;
                for (var i = 0; i < slots.Count; i++)
                {
                    if (false == slots[i].IsAssigned || slots[i].Kind != kind)
                        continue;

                    _heroService.AddEnemyHeroEnergy(i, _botConfig.HeroEnergyPerTick);

                    if (_heroService.GetSlots(BattleSide.Enemy)[i].IsReady
                        && false == _heroActivationPending[i])
                    {
                        _heroActivationPending[i] = true;
                        ActivateWithDelay(i, ct).Forget();
                    }
                }
            }
        }

        private async UniTaskVoid ActivateWithDelay(int slotIndex, CancellationToken ct)
        {
            var delay = _engine.GenerateDelay(
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
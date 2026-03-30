using System;
using Cysharp.Threading.Tasks;
using Project.Scripts.Configs;
using Project.Scripts.Services;
using Project.Scripts.Services.Combat;
using Project.Scripts.Services.EventBusSystem;
using Project.Scripts.Services.EventBusSystem.Events;
using Project.Scripts.Services.UISystem;
using Project.Scripts.Shared.Heroes;
using R3;
using UnityEngine;

namespace Project.Scripts.Gameplay.UI
{
    public class BattleHUDViewModel : BaseViewModel
    {
        private readonly EventBus _eventBus;
        private readonly IEnemyStateService _enemyState;
        private readonly IPlayerStateService _playerState;
        private readonly BattleHUDConfig _config;
        private readonly BattleAnimationConfig _battleAnimationConfig;
        private readonly IHeroService _heroService;
        private readonly TileKindPaletteConfig _palette;
        private readonly LevelConfig _levelConfig;
        private readonly IBoardBoundsProvider _boardBounds;
        private HeroSlotViewModel[] _playerHeroSlots;
        private HeroSlotViewModel[] _enemyHeroSlots;
        private readonly Subject<int> _enemyHit = new();
        private readonly Subject<int> _playerHit = new();
        private int _prevEnemyHP;
        private int _prevPlayerHP;


        public ReactiveProperty<float> EnemyHPFill { get; } = new(1f);
        public ReactiveProperty<float> PlayerHPFill { get; } = new(1f);
        public Observable<int> EnemyHit => _enemyHit;
        public Observable<int> PlayerHit => _playerHit;
        public Sprite EnemyAvatarSprite { get; private set; }
        public Sprite PlayerAvatarSprite { get; private set; }
        public BattleAnimationConfig BattleAnimConfig => _battleAnimationConfig;
        public float BoardTopWorldY => _boardBounds.BoardTopWorldY;
        public float BoardHalfWidth => _boardBounds.BoardHalfWidth;
        public float BoardCenterX => _boardBounds.BoardCenterX;
        public HeroSlotViewModel[] PlayerHeroSlots => _playerHeroSlots;
        public HeroSlotViewModel[] EnemyHeroSlots => _enemyHeroSlots;
        public string EnemyName => _levelConfig.BotConfig ? _levelConfig.BotConfig.OpponentName : string.Empty;


        public BattleHUDViewModel(
            EventBus eventBus,
            IEnemyStateService enemyState,
            IPlayerStateService playerState,
            BattleHUDConfig config,
            BattleAnimationConfig battleAnimationConfig,
            IHeroService heroService,
            TileKindPaletteConfig palette,
            LevelConfig levelConfig,
            IBoardBoundsProvider boardBounds)
        {
            _eventBus = eventBus;
            _enemyState = enemyState;
            _playerState = playerState;
            _config = config;
            _battleAnimationConfig = battleAnimationConfig;
            _heroService = heroService;
            _palette = palette;
            _levelConfig = levelConfig;
            _boardBounds = boardBounds;
        }


        protected override UniTask OnInitializeAsync()
        {
            EnemyAvatarSprite = _config.EnemyAvatarSprite;
            PlayerAvatarSprite = _config.PlayerAvatarSprite;

            _prevEnemyHP = _enemyState.CurrentHP;
            _prevPlayerHP = _playerState.CurrentHP;

            EnemyHPFill.Value = _enemyState.MaxHP > 0
                ? (float)_enemyState.CurrentHP / _enemyState.MaxHP : 1f;
            PlayerHPFill.Value = _playerState.MaxHP > 0
                ? (float)_playerState.CurrentHP / _playerState.MaxHP : 1f;

            _playerHeroSlots = CreateHeroSlotViewModels(
                BattleSide.Player,
                _heroService.GetSlots(BattleSide.Player),
                _levelConfig.PlayerHeroes,
                _heroService.TryActivate);

            _enemyHeroSlots = CreateHeroSlotViewModels(
                BattleSide.Enemy,
                _heroService.GetSlots(BattleSide.Enemy),
                _levelConfig.EnemyHeroes,
                null);

            Disposables.Add(_eventBus.Subscribe<EnemyHPChangedEvent>(OnEnemyHPChanged));
            Disposables.Add(_eventBus.Subscribe<PlayerHPChangedEvent>(OnPlayerHPChanged));
            Disposables.Add(_eventBus.Subscribe<HeroEnergyChangedEvent>(OnHeroEnergyChanged));

            return UniTask.CompletedTask;
        }

        protected override void OnCleanup()
        {
            EnemyHPFill.Dispose();
            PlayerHPFill.Dispose();
            _enemyHit.Dispose();
            _playerHit.Dispose();

            if (null != _playerHeroSlots)
                for (var i = 0; i < _playerHeroSlots.Length; i++)
                    _playerHeroSlots[i]?.Dispose();

            if (null != _enemyHeroSlots)
                for (var i = 0; i < _enemyHeroSlots.Length; i++)
                    _enemyHeroSlots[i]?.Dispose();
        }


        private void OnEnemyHPChanged(EnemyHPChangedEvent e)
        {
            if (e.Current < _prevEnemyHP)
                _enemyHit.OnNext(_prevEnemyHP - e.Current);
            _prevEnemyHP = e.Current;
            EnemyHPFill.Value = e.Max > 0 ? (float)e.Current / e.Max : 0f;
        }

        private void OnPlayerHPChanged(PlayerHPChangedEvent e)
        {
            if (e.Current < _prevPlayerHP)
                _playerHit.OnNext(_prevPlayerHP - e.Current);
            _prevPlayerHP = e.Current;
            PlayerHPFill.Value = e.Max > 0 ? (float)e.Current / e.Max : 0f;
        }

        private void OnHeroEnergyChanged(HeroEnergyChangedEvent e)
        {
            var slots = e.Side == BattleSide.Player ? _playerHeroSlots : _enemyHeroSlots;
            if (null == slots || e.SlotIndex < 0 || e.SlotIndex >= slots.Length)
                return;

            slots[e.SlotIndex]?.UpdateEnergy(e.Current, e.Max);
        }

        private HeroSlotViewModel[] CreateHeroSlotViewModels(
            BattleSide side,
            System.Collections.Generic.IReadOnlyList<HeroSlotState> states,
            HeroConfig[] configs,
            Action<int> onActivate)
        {
            var slots = new HeroSlotViewModel[4];

            for (var i = 0; i < 4; i++)
            {
                var state = states[i];
                var config = (configs != null && i < configs.Length) ? configs[i] : null;
                var color = state.IsAssigned
                    ? _palette.GetColor(state.Kind, Color.gray)
                    : Color.gray;
                var portrait = config ? config.Portrait : null;

                slots[i] = new HeroSlotViewModel(i, side, state, color, portrait, onActivate);
            }

            return slots;
        }
    }
}
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Scripts.Configs;
using Project.Scripts.Configs.Battle;
using Project.Scripts.Configs.Board;
using Project.Scripts.Configs.Levels;
using Project.Scripts.Services;
using Project.Scripts.Services.Combat;
using Project.Scripts.Services.EventBusSystem;
using Project.Scripts.Services.EventBusSystem.Events;
using Project.Scripts.Services.UISystem;
using Project.Scripts.Shared.Heroes;
using UnityEngine;

namespace Project.Scripts.Gameplay.UI
{
    public class BattleHUDViewModel : BaseViewModel
    {
        public AvatarSlotViewModel PlayerAvatar { get; private set; }
        public AvatarSlotViewModel EnemyAvatar { get; private set; }
        public HeroSlotViewModel[] PlayerHeroSlots => _playerHeroSlots;
        public HeroSlotViewModel[] EnemyHeroSlots => _enemyHeroSlots;
        public IReadyPulseCoordinator PulseCoordinator { get; }
        public IAbilityExecutionService AbilityExecution { get; }
        public string EnemyName => _levelConfig.BotConfig ? _levelConfig.BotConfig.OpponentName : string.Empty;
        public BattleAnimationConfig BattleAnimConfig => _battleAnimationConfig;
        public float BoardTopWorldY => _boardBounds.BoardTopWorldY;
        public float BoardHalfWidth => _boardBounds.BoardHalfWidth;
        public float BoardCenterX => _boardBounds.BoardCenterX;

        public float CellSize
        {
            get { return _boardBounds.CellSize; }
        }


        private readonly EventBus _eventBus;
        private readonly IEnemyStateService _enemyState;
        private readonly IPlayerStateService _playerState;
        private readonly BattleViewConfig _battleViewConfig;
        private readonly BattleAnimationConfig _battleAnimationConfig;
        private readonly IHeroService _heroService;
        private readonly TileKindPaletteConfig _palette;
        private readonly LevelConfig _levelConfig;
        private readonly IBoardBoundsProvider _boardBounds;
        private HeroSlotViewModel[] _playerHeroSlots;
        private HeroSlotViewModel[] _enemyHeroSlots;


        public BattleHUDViewModel(
            EventBus eventBus,
            IEnemyStateService enemyState,
            IPlayerStateService playerState,
            BattleViewConfig battleViewConfig,
            BattleAnimationConfig battleAnimationConfig,
            IHeroService heroService,
            TileKindPaletteConfig palette,
            LevelConfig levelConfig,
            IBoardBoundsProvider boardBounds,
            IReadyPulseCoordinator pulseCoordinator,
            IAbilityExecutionService abilityExecution)
        {
            _eventBus = eventBus;
            _enemyState = enemyState;
            _playerState = playerState;
            _battleViewConfig = battleViewConfig;
            _battleAnimationConfig = battleAnimationConfig;
            _heroService = heroService;
            _palette = palette;
            _levelConfig = levelConfig;
            _boardBounds = boardBounds;
            PulseCoordinator = pulseCoordinator;
            AbilityExecution = abilityExecution;
        }


        protected override UniTask OnInitializeAsync()
        {
            PlayerAvatar = new AvatarSlotViewModel(
                _eventBus,
                BattleSide.Player,
                _battleViewConfig.PlayerAvatarSprite,
                _playerState.CurrentHP,
                _playerState.MaxHP,
                _battleAnimationConfig);

            EnemyAvatar = new AvatarSlotViewModel(
                _eventBus,
                BattleSide.Enemy,
                _battleViewConfig.EnemyAvatarSprite,
                _enemyState.CurrentHP,
                _enemyState.MaxHP,
                _battleAnimationConfig);

            _playerHeroSlots = CreateHeroSlotViewModels(
                BattleSide.Player,
                _heroService.GetSlots(BattleSide.Player),
                _levelConfig.PlayerHeroes);

            _enemyHeroSlots = CreateHeroSlotViewModels(
                BattleSide.Enemy,
                _heroService.GetSlots(BattleSide.Enemy),
                _levelConfig.EnemyHeroes);

            Disposables.Add(_eventBus.Subscribe<HeroEnergyChangedEvent>(OnHeroEnergyChanged));
            Disposables.Add(_eventBus.Subscribe<HeroHPChangedEvent>(OnHeroHPChanged));
            Disposables.Add(_eventBus.Subscribe<HeroDefeatedEvent>(OnHeroDefeated));

            return UniTask.CompletedTask;
        }

        protected override void OnCleanup()
        {
            PlayerAvatar?.Dispose();
            EnemyAvatar?.Dispose();

            if (null != _playerHeroSlots)
                for (var i = 0; i < _playerHeroSlots.Length; i++)
                    _playerHeroSlots[i]?.Dispose();

            if (null != _enemyHeroSlots)
                for (var i = 0; i < _enemyHeroSlots.Length; i++)
                    _enemyHeroSlots[i]?.Dispose();
        }


        private void OnHeroEnergyChanged(HeroEnergyChangedEvent e)
        {
            var slots = e.Side == BattleSide.Player ? _playerHeroSlots : _enemyHeroSlots;
            if (null == slots || e.SlotIndex < 0 || e.SlotIndex >= slots.Length)
                return;

            slots[e.SlotIndex]?.UpdateEnergy(e.Current, e.Max);
        }

        private void OnHeroHPChanged(HeroHPChangedEvent e)
        {
            var slots = e.Side == BattleSide.Player ? _playerHeroSlots : _enemyHeroSlots;
            if (null == slots || e.SlotIndex < 0 || e.SlotIndex >= slots.Length)
                return;

            slots[e.SlotIndex]?.UpdateHP(e.Current, e.Max);
        }

        private void OnHeroDefeated(HeroDefeatedEvent e)
        {
            // Reserved for future side effects (sound, particles, etc.).
        }

        private HeroSlotViewModel[] CreateHeroSlotViewModels(
            BattleSide side,
            IReadOnlyList<HeroSlotState> states,
            HeroConfig[] configs)
        {
            var slots = new HeroSlotViewModel[4];

            for (var i = 0; i < 4; i++)
            {
                var state = states[i];
                var config = (null != configs && i < configs.Length) ? configs[i] : null;
                var color = state.IsAssigned
                    ? _palette.GetColor(state.Kind, Color.gray)
                    : Color.gray;
                var portrait = config ? config.Portrait : null;

                slots[i] = new HeroSlotViewModel(i, side, state, color, portrait);
            }

            return slots;
        }
    }
}
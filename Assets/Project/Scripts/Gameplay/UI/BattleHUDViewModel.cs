using Cysharp.Threading.Tasks;
using Project.Scripts.Configs;
using Project.Scripts.Services.Combat;
using Project.Scripts.Services.EventBusSystem;
using Project.Scripts.Services.EventBusSystem.Events;
using Project.Scripts.Services.UISystem;
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


        public ReactiveProperty<float> EnemyHPFill { get; } = new(1f);
        public ReactiveProperty<float> PlayerHPFill { get; } = new(1f);
        public Sprite EnemyAvatarSprite { get; private set; }
        public Sprite PlayerAvatarSprite { get; private set; }
        public Color EnemyHPBarColor { get; private set; }
        public Color PlayerHPBarColor { get; private set; }
        public float BoardTopWorldY { get; private set; }


        public BattleHUDViewModel(EventBus eventBus, IEnemyStateService enemyState,
            IPlayerStateService playerState, BattleHUDConfig config)
        {
            _eventBus = eventBus;
            _enemyState = enemyState;
            _playerState = playerState;
            _config = config;
        }


        public void SetBoardTopWorldY(float worldY) => BoardTopWorldY = worldY;


        protected override UniTask OnInitializeAsync()
        {
            EnemyAvatarSprite = _config.EnemyAvatarSprite;
            PlayerAvatarSprite = _config.PlayerAvatarSprite;
            EnemyHPBarColor = _config.EnemyHPBarColor;
            PlayerHPBarColor = _config.PlayerHPBarColor;

            EnemyHPFill.Value = _enemyState.MaxHP > 0
                ? (float)_enemyState.CurrentHP / _enemyState.MaxHP : 1f;
            PlayerHPFill.Value = _playerState.MaxHP > 0
                ? (float)_playerState.CurrentHP / _playerState.MaxHP : 1f;

            Disposables.Add(_eventBus.Subscribe<EnemyHPChangedEvent>(OnEnemyHPChanged));
            Disposables.Add(_eventBus.Subscribe<PlayerHPChangedEvent>(OnPlayerHPChanged));

            return UniTask.CompletedTask;
        }

        protected override void OnCleanup()
        {
            EnemyHPFill.Dispose();
            PlayerHPFill.Dispose();
        }


        private void OnEnemyHPChanged(EnemyHPChangedEvent e) =>
            EnemyHPFill.Value = e.Max > 0 ? (float)e.Current / e.Max : 0f;

        private void OnPlayerHPChanged(PlayerHPChangedEvent e) =>
            PlayerHPFill.Value = e.Max > 0 ? (float)e.Current / e.Max : 0f;
    }
}
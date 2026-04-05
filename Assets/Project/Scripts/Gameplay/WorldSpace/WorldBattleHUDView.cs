using Cysharp.Threading.Tasks;
using Project.Scripts.Configs;
using Project.Scripts.Configs.Battle;
using Project.Scripts.Gameplay.UI;
using Project.Scripts.Services.Input;
using Project.Scripts.Services.UISystem;
using R3;
using UnityEngine;
using UnityEngine.Pool;

namespace Project.Scripts.Gameplay.WorldSpace
{
    public class WorldBattleHUDView : BaseView<BattleHUDViewModel>
    {
        [Tooltip("Four hero slot views for the player side, ordered left to right (indices 0–3)")]
        [SerializeField] private WorldHeroSlotView[] _playerHeroSlots;

        [Tooltip("Four hero slot views for the enemy side, ordered left to right (indices 0–3)")]
        [SerializeField] private WorldHeroSlotView[] _enemyHeroSlots;

        [Tooltip("Avatar slot view for the player")]
        [SerializeField] private WorldAvatarSlotView _playerAvatarSlot;

        [Tooltip("Avatar slot view for the enemy")]
        [SerializeField] private WorldAvatarSlotView _enemyAvatarSlot;

        [Tooltip("Handles drag-to-target input gestures in world space")]
        [SerializeField] private WorldTargetingInputHandler _targetingInputHandler;

        [Tooltip("Prefab with WorldFloatingDamageNumber component — pooled at runtime")]
        [SerializeField] private WorldFloatingDamageNumber _floatingDamagePrefab;

        
        private IInputService _inputService;
        private BattleViewConfig _battleViewConfig;
        private ObjectPool<WorldFloatingDamageNumber> _floatingPool;


        protected override UniTask OnBindViewModel()
        {
            PositionHUD();
            BindSlots();
            SetupTargeting();
            SetupFloatingNumbers();

            return UniTask.CompletedTask;
        }

        protected override void OnClose()
        {
            _floatingPool?.Dispose();
            _floatingPool = null;
        }


        public void SetDependencies(IInputService inputService, BattleViewConfig battleViewConfig)
        {
            _inputService = inputService;
            _battleViewConfig = battleViewConfig;
        }


        private void PositionHUD()
        {
            transform.position = new Vector3(
                ViewModel.BoardCenterX,
                ViewModel.BoardTopWorldY + _battleViewConfig.BattleAreaTopPadding,
                0f);
        }

        private void BindSlots()
        {
            _playerAvatarSlot?.Bind(ViewModel.PlayerAvatar, ViewModel.PulseCoordinator);
            _enemyAvatarSlot?.Bind(ViewModel.EnemyAvatar, ViewModel.PulseCoordinator);

            BindHeroRow(_playerHeroSlots, ViewModel.PlayerHeroSlots);
            BindHeroRow(_enemyHeroSlots, ViewModel.EnemyHeroSlots);
        }

        private void BindHeroRow(WorldHeroSlotView[] views, HeroSlotViewModel[] viewModels)
        {
            if (null == views || null == viewModels)
                return;

            var count = Mathf.Min(views.Length, viewModels.Length);
            for (var i = 0; i < count; i++)
            {
                if (views[i])
                    views[i].Bind(viewModels[i], ViewModel.PulseCoordinator, ViewModel.BattleAnimConfig);
            }
        }

        private void SetupTargeting()
        {
            if (false == _targetingInputHandler || null == _inputService)
                return;

            var registry = new WorldTargetingRegistry();

            if (_playerAvatarSlot)
                registry.Register(_playerAvatarSlot);

            if (_enemyAvatarSlot)
                registry.Register(_enemyAvatarSlot);

            if (_playerHeroSlots != null)
                for (var i = 0; i < _playerHeroSlots.Length; i++)
                    if (_playerHeroSlots[i])
                        registry.Register(_playerHeroSlots[i]);

            if (_enemyHeroSlots != null)
                for (var i = 0; i < _enemyHeroSlots.Length; i++)
                    if (_enemyHeroSlots[i])
                        registry.Register(_enemyHeroSlots[i]);

            _targetingInputHandler.Init(_inputService, registry, ViewModel.AbilityExecution, Camera.main);
        }

        private void SetupFloatingNumbers()
        {
            if (false == _floatingDamagePrefab)
                return;

            _floatingPool = new ObjectPool<WorldFloatingDamageNumber>(
                createFunc: () => Instantiate(_floatingDamagePrefab, transform),
                actionOnGet: c => c.gameObject.SetActive(true),
                actionOnRelease: c => { c.Kill(); c.gameObject.SetActive(false); },
                actionOnDestroy: c => { if (c) Destroy(c.gameObject); },
                defaultCapacity: 4,
                maxSize: 16);

            ViewModel.EnemyAvatar.Hit
                .Subscribe(dmg => SpawnFloatingNumber(dmg, FloatingNumberType.Damage, _enemyAvatarSlot.HitAnchor))
                .AddTo(Disposables);

            ViewModel.PlayerAvatar.Hit
                .Subscribe(dmg => SpawnFloatingNumber(dmg, FloatingNumberType.Damage, _playerAvatarSlot.HitAnchor))
                .AddTo(Disposables);

            ViewModel.EnemyAvatar.Heal
                .Subscribe(amt => SpawnFloatingNumber(amt, FloatingNumberType.Heal, _enemyAvatarSlot.HitAnchor))
                .AddTo(Disposables);

            ViewModel.PlayerAvatar.Heal
                .Subscribe(amt => SpawnFloatingNumber(amt, FloatingNumberType.Heal, _playerAvatarSlot.HitAnchor))
                .AddTo(Disposables);

            BindHeroFloatingNumbers(_playerHeroSlots, ViewModel.PlayerHeroSlots);
            BindHeroFloatingNumbers(_enemyHeroSlots, ViewModel.EnemyHeroSlots);
        }

        private void BindHeroFloatingNumbers(WorldHeroSlotView[] views, HeroSlotViewModel[] viewModels)
        {
            if (null == views || null == viewModels)
                return;

            var count = Mathf.Min(views.Length, viewModels.Length);
            for (var i = 0; i < count; i++)
            {
                if (false == views[i] || false == viewModels[i].IsAssigned)
                    continue;

                var anchor = views[i].HitAnchor;
                var vm = viewModels[i];

                vm.Hit
                    .Subscribe(dmg => SpawnFloatingNumber(dmg, FloatingNumberType.Damage, anchor))
                    .AddTo(Disposables);

                vm.Heal
                    .Subscribe(amt => SpawnFloatingNumber(amt, FloatingNumberType.Heal, anchor))
                    .AddTo(Disposables);
            }
        }

        private void SpawnFloatingNumber(int value, FloatingNumberType type, Transform anchor)
        {
            if (null == _floatingPool || false == anchor)
                return;

            var item = _floatingPool.Get();
            item.Play(value, type, anchor, ViewModel.BattleAnimConfig,
                () => _floatingPool.Release(item));
        }
    }
}
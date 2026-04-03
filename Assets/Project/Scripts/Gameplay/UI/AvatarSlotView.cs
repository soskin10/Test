using Project.Scripts.Configs;
using Project.Scripts.Shared.Heroes;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.Gameplay.UI
{
    public class AvatarSlotView : MonoBehaviour, ITargetableView
    {
        [Tooltip("Avatar portrait image")]
        [SerializeField] private Image _portrait;

        [Tooltip("HP bar component — handles fill bar, lag bar, and shake")]
        [SerializeField] private HPBarComponent _hpBar;

        [Tooltip("Hit reaction component — flash and knockback on damage")]
        [SerializeField] private HitReactionComponent _hitReaction;

        [Tooltip("Energy bar displayed next to the avatar")]
        [SerializeField] private AvatarChargeBarView _energyBar;

        [Header("Targeting")]
        [Tooltip("Glow image shown when this unit is highlighted as source or target. Disabled by default.")]
        [SerializeField] private Image _glowImage;


        public RectTransform HitAnchor => _hitReaction ? (RectTransform)_hitReaction.transform : (RectTransform)transform;
        public UnitDescriptor Descriptor => UnitDescriptor.Avatar(_viewModel.Side);
        public RectTransform HitArea => (RectTransform)transform;
        public bool IsReadySource => _viewModel is { Side: BattleSide.Player } && _viewModel.EnergyBar.IsReady.CurrentValue;

        
        private AvatarSlotViewModel _viewModel;
        private BattleAnimationConfig _animConfig;
        private CompositeDisposable _disposables;
        
        
        private void OnDestroy()
        {
            _disposables?.Dispose();
        }
        
        
        public bool IsValidTarget(UnitDescriptor source)
        {
            if (_viewModel == null)
                return false;

            if (source.ActionType == HeroActionType.DealDamage && _viewModel.Side == BattleSide.Enemy)
                return true;

            if (source.ActionType == HeroActionType.HealAlly && _viewModel.Side == BattleSide.Player)
                return _viewModel.HPFill.CurrentValue < 1f;

            return false;
        }

        public void SetSourceHighlight(bool active)
        {
            if (!_glowImage)
                return;

            if (active && _animConfig)
                _glowImage.color = _animConfig.SourceHighlightColor;

            _glowImage.enabled = active;
        }

        public void SetTargetHighlight(bool active, HeroActionType actionType)
        {
            if (!_glowImage)
                return;

            if (active && _animConfig)
                _glowImage.color = actionType == HeroActionType.HealAlly
                    ? _animConfig.HealTargetColor
                    : _animConfig.AttackTargetColor;

            _glowImage.enabled = active;
        }

        public void Bind(AvatarSlotViewModel viewModel, IReadyPulseCoordinator pulseCoordinator)
        {
            _viewModel = viewModel;
            _animConfig = viewModel.AnimConfig;
            _disposables?.Dispose();
            _disposables = new CompositeDisposable();

            if (_portrait)
                _portrait.sprite = viewModel.Portrait;

            if (_hpBar)
            {
                _hpBar.Init(viewModel.AnimConfig);
                _hpBar.SetFillInstant(viewModel.HPFill.CurrentValue);

                viewModel.HPFill
                    .Skip(1)
                    .Subscribe(v => _hpBar.AnimateFill(v))
                    .AddTo(_disposables);
            }

            if (_hitReaction)
            {
                _hitReaction.Init(viewModel.AnimConfig);

                viewModel.Hit
                    .Subscribe(_ => _hitReaction.PlayHitReaction())
                    .AddTo(_disposables);
            }

            _energyBar?.Bind(viewModel.EnergyBar, pulseCoordinator, viewModel.AnimConfig);
        }
    }
}
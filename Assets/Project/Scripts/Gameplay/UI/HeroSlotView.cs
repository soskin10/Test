using DG.Tweening;
using Project.Scripts.Configs;
using Project.Scripts.Shared.Heroes;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.Gameplay.UI
{
    public class HeroSlotView : MonoBehaviour, ITargetableView
    {
        [Header("Visuals")]
        [Tooltip("Background Image")]
        [SerializeField] private Image _background;

        [Tooltip("Hero portrait Image — tinted with the hero's element color when a hero is assigned")]
        [SerializeField] private Image _portrait;

        [Tooltip("Energy bar fill Image (Type=Filled, FillMethod=Vertical, FillOrigin=Bottom)")]
        [SerializeField] private Image _energyBarFill;

        [Header("HP")]
        [Tooltip("Reusable HP bar component — wire up HPBar and LagBar images inside the prefab")]
        [SerializeField] private HPBarComponent _hpBar;

        [Header("Targeting")]
        [Tooltip("Glow image shown when this unit is highlighted as source or target. Disabled by default.")]
        [SerializeField] private Image _glowImage;

        
        public UnitDescriptor Descriptor => UnitDescriptor.Hero(_viewModel.Side, _viewModel.SlotIndex, _viewModel.ActionType);
        public RectTransform HitArea => (RectTransform)transform;
        public bool IsReadySource => _viewModel.IsAssigned && _viewModel.IsActivatable.CurrentValue;

        
        private Color _defaultEnergyBarColor;
        private CompositeDisposable _disposables = new();
        private BattleAnimationConfig _config;
        private Tweener _energyFillTween;
        private HeroSlotViewModel _viewModel;
        
        
        private void Awake()
        {
            if (_energyBarFill)
                _defaultEnergyBarColor = _energyBarFill.color;
        }

        private void OnDestroy()
        {
            _energyFillTween?.Kill();
            _disposables?.Dispose();
        }


        public void Bind(HeroSlotViewModel viewModel, IReadyPulseCoordinator pulseCoordinator, BattleAnimationConfig config)
        {
            _viewModel = viewModel;
            _config = config;

            BindPortrait(viewModel);
            BindEnergyBar(viewModel, pulseCoordinator);
            BindHPBar(viewModel);
        }
        
        public bool IsValidTarget(UnitDescriptor source)
        {
            if (_viewModel == null || !_viewModel.IsAssigned || _viewModel.IsDefeated.CurrentValue)
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

            if (active && _config)
                _glowImage.color = _config.SourceHighlightColor;

            _glowImage.enabled = active;
        }

        public void SetTargetHighlight(bool active, HeroActionType actionType)
        {
            if (!_glowImage)
                return;

            if (active && _config)
                _glowImage.color = actionType == HeroActionType.HealAlly
                    ? _config.HealTargetColor
                    : _config.AttackTargetColor;

            _glowImage.enabled = active;
        }


        private void BindPortrait(HeroSlotViewModel viewModel)
        {
            if (!_portrait)
                return;

            _portrait.enabled = viewModel.IsAssigned;

            if (!viewModel.IsAssigned)
                return;

            _portrait.color = viewModel.SlotColor;

            if (viewModel.Portrait)
                _portrait.sprite = viewModel.Portrait;

            viewModel.IsDefeated
                .Subscribe(defeated => _portrait.color = defeated ? Color.gray : viewModel.SlotColor)
                .AddTo(_disposables);
        }

        private void BindEnergyBar(HeroSlotViewModel viewModel, IReadyPulseCoordinator pulseCoordinator)
        {
            if (!_energyBarFill)
                return;

            if (viewModel.IsAssigned)
            {
                _energyBarFill.color      = viewModel.SlotColor;
                _defaultEnergyBarColor    = viewModel.SlotColor;
            }

            _energyBarFill.fillAmount = viewModel.EnergyFill.CurrentValue;

            viewModel.EnergyFill
                .Skip(1)
                .Subscribe(v =>
                {
                    _energyFillTween?.Kill();
                    if (_config)
                        _energyFillTween = _energyBarFill.DOFillAmount(v, _config.EnergyFillDuration).SetEase(_config.EnergyFillEase);
                    else
                        _energyBarFill.fillAmount = v;
                })
                .AddTo(_disposables);

            if (!viewModel.IsAssigned)
                return;

            viewModel.IsActivatable
                .Subscribe(activatable =>
                {
                    if (!activatable)
                        _energyBarFill.color = _defaultEnergyBarColor;
                })
                .AddTo(_disposables);

            pulseCoordinator.Alpha
                .Subscribe(a =>
                {
                    if (viewModel.IsActivatable.CurrentValue)
                    {
                        var c = _defaultEnergyBarColor;
                        c.a = a;
                        _energyBarFill.color = c;
                    }
                })
                .AddTo(_disposables);

            viewModel.IsDefeated
                .Subscribe(defeated => _energyBarFill.enabled = !defeated)
                .AddTo(_disposables);
        }

        private void BindHPBar(HeroSlotViewModel viewModel)
        {
            if (!_hpBar || !viewModel.IsAssigned)
                return;

            _hpBar.Init(_config);
            _hpBar.SetFillInstant(viewModel.HPFill.CurrentValue);

            viewModel.HPFill
                .Skip(1)
                .Subscribe(fill => _hpBar.AnimateFill(fill))
                .AddTo(_disposables);

            viewModel.IsDefeated
                .Subscribe(defeated => _hpBar.gameObject.SetActive(!defeated))
                .AddTo(_disposables);
        }
    }
}
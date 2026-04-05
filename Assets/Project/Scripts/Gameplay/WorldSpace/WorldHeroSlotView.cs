using DG.Tweening;
using Project.Scripts.Configs;
using Project.Scripts.Configs.Battle;
using Project.Scripts.Gameplay.UI;
using Project.Scripts.Shared.Heroes;
using R3;
using UnityEngine;

namespace Project.Scripts.Gameplay.WorldSpace
{
    public class WorldHeroSlotView : MonoBehaviour, IWorldTargetable
    {
        [Tooltip("Background SpriteRenderer — defines the slot's visual bounds and is scaled by SetSize")]
        [SerializeField] private SpriteRenderer _background;

        [Tooltip("Portrait SpriteRenderer — tinted with the hero's element color when assigned")]
        [SerializeField] private SpriteRenderer _portrait;

        [Tooltip("Glow SpriteRenderer with Additive material — shown as source or target highlight")]
        [SerializeField] private SpriteRenderer _glow;

        [Tooltip("Main HP bar — snaps instantly on damage")]
        [SerializeField] private WorldBarRenderer _hpBar;

        [Tooltip("Lag HP bar placed visually behind the main bar — drains with a delay after damage")]
        [SerializeField] private WorldBarRenderer _hpLagBar;

        [Tooltip("Energy bar — tinted with the hero's element color")]
        [SerializeField] private WorldBarRenderer _energyBar;

        [Tooltip("Spawn anchor for floating damage/heal numbers — defaults to slot centre if unassigned")]
        [SerializeField] private Transform _hitAnchor;


        public Transform HitAnchor => _hitAnchor ? _hitAnchor : transform;
        public UnitDescriptor Descriptor => UnitDescriptor.Hero(_viewModel.Side, _viewModel.SlotIndex, _viewModel.ActionType);
        public bool IsReadySource => _viewModel != null && _viewModel.IsAssigned && _viewModel.IsActivatable.CurrentValue;
        public Bounds WorldBounds => _background ? _background.bounds : new Bounds(transform.position, Vector3.one);


        private HeroSlotViewModel _viewModel;
        private BattleAnimationConfig _config;
        private CompositeDisposable _disposables;
        private Color _originalPortraitColor;
        private Vector3 _originalLocalPos;
        private Tween _hitFlashTween;
        private Tween _knockbackTween;


        private void OnDestroy()
        {
            _hitFlashTween?.Kill();
            _knockbackTween?.Kill();
            _disposables?.Dispose();
        }


        public void Bind(HeroSlotViewModel viewModel, IReadyPulseCoordinator pulseCoordinator, BattleAnimationConfig config)
        {
            _viewModel = viewModel;
            _config = config;
            _disposables?.Dispose();
            _disposables = new CompositeDisposable();
            _originalLocalPos = transform.localPosition;

            BindPortrait(viewModel);
            BindHPBars(viewModel);
            BindEnergyBar(viewModel, pulseCoordinator);
            BindHitReaction(viewModel);
        }

        public bool IsValidTarget(UnitDescriptor source)
        {
            if (_viewModel == null || false == _viewModel.IsAssigned || _viewModel.IsDefeated.CurrentValue)
                return false;

            if (source.ActionType == HeroActionType.DealDamage && _viewModel.Side == BattleSide.Enemy)
                return true;

            if (source.ActionType == HeroActionType.HealAlly && _viewModel.Side == BattleSide.Player)
                return _viewModel.HPFill.CurrentValue < 1f;

            return false;
        }

        public void SetSourceHighlight(bool active)
        {
            if (false == _glow)
                return;

            if (active && _config)
                _glow.color = _config.SourceHighlightColor;

            _glow.gameObject.SetActive(active);
        }

        public void SetTargetHighlight(bool active, HeroActionType actionType)
        {
            if (false == _glow)
                return;

            if (active && _config)
                _glow.color = actionType == HeroActionType.HealAlly
                    ? _config.HealTargetColor
                    : _config.AttackTargetColor;

            _glow.gameObject.SetActive(active);
        }


        private void BindPortrait(HeroSlotViewModel viewModel)
        {
            if (false == _portrait)
                return;

            _portrait.enabled = viewModel.IsAssigned;

            if (false == viewModel.IsAssigned)
                return;

            _portrait.color = viewModel.SlotColor;

            if (viewModel.Portrait)
                _portrait.sprite = viewModel.Portrait;

            viewModel.IsDefeated
                .Subscribe(defeated => _portrait.color = defeated ? Color.gray : viewModel.SlotColor)
                .AddTo(_disposables);
        }

        private void BindHPBars(HeroSlotViewModel viewModel)
        {
            if (false == viewModel.IsAssigned)
                return;

            if (_hpBar)
                _hpBar.SetFill(viewModel.HPFill.CurrentValue);

            if (_hpLagBar)
                _hpLagBar.SetFill(viewModel.HPFill.CurrentValue);

            viewModel.HPFill
                .Skip(1)
                .Subscribe(fill =>
                {
                    var isDamage = _hpBar != null && fill < _hpBar.CurrentNormalized;

                    if (isDamage)
                    {
                        _hpBar?.SetFill(fill);

                        if (_config)
                            _hpLagBar?.SetFillAnimated(fill, _config.HPBarLagDuration, _config.HPBarLagDelay);
                        else
                            _hpLagBar?.SetFill(fill);
                    }
                    else
                    {
                        var healDuration = _config ? _config.HPBarHealDuration : 0.4f;
                        _hpBar?.SetFillAnimated(fill, healDuration);
                        _hpLagBar?.SetFillAnimated(fill, healDuration);
                    }
                })
                .AddTo(_disposables);

            viewModel.IsDefeated
                .Subscribe(defeated =>
                {
                    if (_hpBar)
                        _hpBar.gameObject.SetActive(false == defeated);

                    if (_hpLagBar)
                        _hpLagBar.gameObject.SetActive(false == defeated);
                })
                .AddTo(_disposables);
        }

        private void BindEnergyBar(HeroSlotViewModel viewModel, IReadyPulseCoordinator pulseCoordinator)
        {
            if (false == _energyBar || false == viewModel.IsAssigned)
                return;

            _energyBar.SetFillColor(viewModel.SlotColor);
            _energyBar.SetFill(viewModel.EnergyFill.CurrentValue);

            viewModel.EnergyFill
                .Skip(1)
                .Subscribe(v =>
                {
                    var duration = _config ? _config.EnergyFillDuration : 0.35f;
                    _energyBar.SetFillAnimated(v, duration);
                })
                .AddTo(_disposables);

            viewModel.IsActivatable
                .Subscribe(activatable =>
                {
                    if (false == activatable)
                        _energyBar.SetFillAlpha(1f);
                })
                .AddTo(_disposables);

            pulseCoordinator.Alpha
                .Subscribe(a =>
                {
                    if (viewModel.IsActivatable.CurrentValue)
                        _energyBar.SetFillAlpha(a);
                })
                .AddTo(_disposables);

            viewModel.IsDefeated
                .Subscribe(defeated => _energyBar.gameObject.SetActive(false == defeated))
                .AddTo(_disposables);
        }

        private void BindHitReaction(HeroSlotViewModel viewModel)
        {
            if (false == viewModel.IsAssigned || false == _portrait || false == _config)
                return;

            _originalPortraitColor = _portrait.color;

            viewModel.IsDefeated
                .Subscribe(defeated => _originalPortraitColor = defeated ? Color.gray : viewModel.SlotColor)
                .AddTo(_disposables);

            viewModel.Hit
                .Subscribe(_ =>
                {
                    PlayHitFlash();
                    PlayKnockback();
                })
                .AddTo(_disposables);
        }

        private void PlayHitFlash()
        {
            _hitFlashTween?.Kill();
            var halfDuration = _config.HitFlashDuration * 0.5f;

            _hitFlashTween = _portrait
                .DOColor(_config.HitFlashColor, halfDuration)
                .SetEase(_config.HitFlashEase)
                .OnComplete(() =>
                {
                    _hitFlashTween = _portrait
                        .DOColor(_originalPortraitColor, halfDuration)
                        .SetEase(_config.HitFlashEase);
                });
        }

        private void PlayKnockback()
        {
            _knockbackTween?.Kill();
            transform.localPosition = _originalLocalPos;

            var direction = _viewModel.Side == BattleSide.Enemy ? 1f : -1f;
            var targetY = _originalLocalPos.y + direction * _config.KnockbackDistance;
            var halfDuration = _config.KnockbackDuration * 0.5f;

            _knockbackTween = transform
                .DOLocalMoveY(targetY, halfDuration)
                .SetEase(_config.KnockbackEase)
                .OnComplete(() =>
                {
                    _knockbackTween = transform
                        .DOLocalMoveY(_originalLocalPos.y, halfDuration)
                        .SetEase(_config.KnockbackEase);
                });
        }
    }
}
using Project.Scripts.Services.Input;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.Gameplay.UI
{
    public class AvatarSlotView : MonoBehaviour
    {
        [Tooltip("Avatar portrait image")]
        [SerializeField] private Image _portrait;

        [Tooltip("HP bar component — handles fill bar, lag bar, and shake")]
        [SerializeField] private HPBarComponent _hpBar;

        [Tooltip("Hit reaction component — flash and knockback on damage")]
        [SerializeField] private HitReactionComponent _hitReaction;

        [Tooltip("Energy bar displayed next to the avatar")]
        [SerializeField] private AvatarChargeBarView _energyBar;

        [Tooltip("Avatar activation input handler — assign for player avatar, leave empty for enemy")]
        [SerializeField] private AvatarActivationInputHandler _activationHandler;


        private CompositeDisposable _disposables;


        public RectTransform HitAnchor => _hitReaction ? (RectTransform)_hitReaction.transform : (RectTransform)transform;


        private void OnDestroy()
        {
            _disposables?.Dispose();
        }


        public void Bind(AvatarSlotViewModel viewModel, IReadyPulseCoordinator pulseCoordinator)
        {
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

            _energyBar?.Bind(viewModel.EnergyBar, pulseCoordinator);

            if (_activationHandler)
            {
                _activationHandler.Initialize(viewModel.EventBus);
                _activationHandler.SetInteractable(false);

                viewModel.EnergyBar.IsReady
                    .Subscribe(ready => _activationHandler.SetInteractable(ready))
                    .AddTo(_disposables);
            }
        }
    }
}
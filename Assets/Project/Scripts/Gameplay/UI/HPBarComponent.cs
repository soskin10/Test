using DG.Tweening;
using Project.Scripts.Configs;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.Gameplay.UI
{
    public class HPBarComponent : MonoBehaviour
    {
        [Tooltip("Main HP fill image (Type=Filled, Method=Horizontal) — snaps instantly on damage")]
        [SerializeField] private Image _hpBar;

        [Tooltip("Lag bar fill image placed visually behind the main HP bar — drains with a delay after damage")]
        [SerializeField] private Image _lagBar;


        private BattleAnimationConfig _config;
        private Tweener _hpBarTween;
        private Tweener _lagTween;

        
        private void OnDestroy()
        {
            _hpBarTween?.Kill();
            _lagTween?.Kill();
        }
        

        public void Init(BattleAnimationConfig config)
        {
            _config = config;
        }

        public void SetFillInstant(float fill)
        {
            _hpBarTween?.Kill();
            _lagTween?.Kill();

            if (_hpBar) 
                _hpBar.fillAmount = fill;
            if (_lagBar) 
                _lagBar.fillAmount = fill;
        }

        public void AnimateFill(float newFill)
        {
            if (!_config)
            {
                SetFillInstant(newFill);
                return;
            }

            var currentFill = _hpBar ? _hpBar.fillAmount : newFill;
            if (newFill < currentFill)
                AnimateDamage(newFill);
            else
                AnimateHeal(newFill);
        }


        private void AnimateDamage(float newFill)
        {
            _hpBarTween?.Kill();

            if (_hpBar)
                _hpBar.fillAmount = newFill;

            AnimateLagBar(newFill);
        }

        private void AnimateHeal(float newFill)
        {
            _hpBarTween?.Kill();
            _lagTween?.Kill();

            _hpBarTween = _hpBar
                ? _hpBar.DOFillAmount(newFill, _config.HPBarHealDuration).SetEase(_config.HPBarHealEase)
                : null;

            _lagTween = _lagBar
                ? _lagBar.DOFillAmount(newFill, _config.HPBarHealDuration).SetEase(_config.HPBarHealEase)
                : null;
        }

        private void AnimateLagBar(float targetFill)
        {
            if (!_lagBar)
                return;

            _lagTween?.Kill();
            _lagTween = _lagBar
                .DOFillAmount(targetFill, _config.HPBarLagDuration)
                .SetDelay(_config.HPBarLagDelay)
                .SetEase(_config.HPBarLagEase);
        }
    }
}
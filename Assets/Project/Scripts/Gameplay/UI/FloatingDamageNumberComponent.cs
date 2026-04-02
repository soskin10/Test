using System;
using DG.Tweening;
using Project.Scripts.Configs;
using TMPro;
using UnityEngine;

namespace Project.Scripts.Gameplay.UI
{
    public class FloatingDamageNumberComponent : MonoBehaviour
    {
        [Tooltip("TextMeshPro label that displays the damage value")]
        [SerializeField] private TextMeshProUGUI _label;

        
        private Sequence _sequence;

        
        private void OnDestroy()
        {
            _sequence?.Kill();
        }
        

        public void Play(int value, FloatingNumberType type, RectTransform anchor, BattleAnimationConfig config, Action onDone)
        {
            var rect = (RectTransform)transform;
            rect.position = anchor.position;

            _label.text = type switch
            {
                FloatingNumberType.Heal => $"+{value}",
                _ => $"-{value}"
            };
            _label.color = type switch
            {
                FloatingNumberType.Heal => config.HealNumberColor,
                _ => config.DamageNumberColor
            };
            _label.alpha = 1f;

            _sequence?.Kill();
            var startY = rect.anchoredPosition.y;
            _sequence = DOTween.Sequence()
                .Append(rect
                    .DOAnchorPosY(startY + config.FloatDamageDistance, config.FloatDamageDuration)
                    .SetEase(config.FloatDamageEase))
                .Join(_label
                    .DOFade(0f, config.FloatDamageDuration)
                    .SetEase(Ease.InQuad))
                .OnComplete(() => onDone?.Invoke());
        }

        public void Kill()
        {
            _sequence?.Kill();
        }
    }
}
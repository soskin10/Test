using System;
using DG.Tweening;
using Project.Scripts.Configs;
using Project.Scripts.Configs.Battle;
using Project.Scripts.Gameplay.UI;
using TMPro;
using UnityEngine;

namespace Project.Scripts.Gameplay.WorldSpace
{
    public class WorldFloatingDamageNumber : MonoBehaviour
    {
        [Tooltip("TextMeshPro label that displays the damage or heal value")]
        [SerializeField] private TextMeshPro _label;

        [Tooltip("Distance in world units the number floats upward during the animation")]
        [SerializeField] private float _floatDistance = 0.5f;


        private Sequence _sequence;


        private void OnDestroy()
        {
            _sequence?.Kill();
        }


        public void Play(int value, FloatingNumberType type, Transform anchor, BattleAnimationConfig config, Action onDone)
        {
            transform.position = anchor.position;

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
            var startPos = transform.position;

            _sequence = DOTween.Sequence()
                .Append(transform
                    .DOMove(startPos + Vector3.up * _floatDistance, config.FloatDamageDuration)
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
using DG.Tweening;
using UnityEngine;

namespace Project.Scripts.Configs
{
    [CreateAssetMenu(fileName = "BattleAnimationConfig", menuName = "Configs/Battle Animation Config")]
    public class BattleAnimationConfig : ScriptableObject
    {
        [Tooltip("Color the avatar flashes to when receiving damage")]
        [SerializeField] private Color _hitFlashColor = new Color(1f, 0.2f, 0.2f, 1f);

        [Tooltip("Total duration in seconds of the hit flash (fade to color + fade back)")]
        [SerializeField] private float _hitFlashDuration = 0.3f;

        [Tooltip("Easing curve applied to both phases of the hit flash")]
        [SerializeField] private Ease _hitFlashEase = Ease.InOutQuad;

        [Tooltip("Distance in canvas units the avatar panel moves during knockback")]
        [SerializeField] private float _knockbackDistance = 20f;

        [Tooltip("Total duration in seconds of the knockback (move away + return)")]
        [SerializeField] private float _knockbackDuration = 0.3f;

        [Tooltip("Easing curve applied to both phases of the knockback")]
        [SerializeField] private Ease _knockbackEase = Ease.OutQuad;

        [Tooltip("Delay in seconds before the Win/Lose window appears after the final hit animation finishes")]
        [SerializeField] private float _resultScreenDelay = 0.4f;

        [Header("HP Bar Animation")]
        [Tooltip("Delay in seconds before the lag bar starts draining after damage")]
        [SerializeField] private float _hpBarLagDelay = 0.2f;

        [Tooltip("Duration in seconds for the lag bar to drain to the new HP value")]
        [SerializeField] private float _hpBarLagDuration = 1f;

        [Tooltip("Easing curve for the lag bar drain")]
        [SerializeField] private Ease _hpBarLagEase = Ease.OutCubic;

        [Tooltip("Duration in seconds for a smooth HP tween when healing (no shake, no lag bar)")]
        [SerializeField] private float _hpBarHealDuration = 0.4f;

        [Tooltip("Easing curve for the heal tween")]
        [SerializeField] private Ease _hpBarHealEase = Ease.OutQuad;

        [Header("Ready Pulse")]
        [Tooltip("Duration of one full pulse cycle in seconds")]
        [SerializeField] private float _readyPulseDuration = 0.6f;

        [Tooltip("Minimum alpha reached at the bottom of the pulse (0-1)")]
        [SerializeField] private float _readyPulseAlpha = 0.4f;

        [Tooltip("Easing curve for the pulse animation")]
        [SerializeField] private Ease _readyPulseEase = Ease.InOutSine;

        [Header("Energy Bar Animation")]
        [Tooltip("Duration in seconds for the energy fill tween")]
        [SerializeField] private float _energyFillDuration = 0.35f;

        [Tooltip("Easing curve for the energy fill tween")]
        [SerializeField] private Ease _energyFillEase = Ease.OutCubic;

        [Header("Targeting Highlights")]
        [Tooltip("Glow color of the unit being dragged from (source)")]
        [SerializeField] private Color _sourceHighlightColor = Color.white;

        [Tooltip("Glow color of a valid attack target")]
        [SerializeField] private Color _attackTargetColor = new Color(1f, 0.15f, 0.15f, 1f);

        [Tooltip("Glow color of a valid heal target")]
        [SerializeField] private Color _healTargetColor = new Color(0.15f, 1f, 0.25f, 1f);

        [Header("Floating Numbers")]
        [Tooltip("Color of the damage number label")]
        [SerializeField] private Color _damageNumberColor = new Color(1f, 0.25f, 0.25f, 1f);

        [Tooltip("Color of the heal number label")]
        [SerializeField] private Color _healNumberColor = new Color(0.25f, 1f, 0.25f, 1f);

        [Tooltip("Distance in canvas units the damage number floats upward")]
        [SerializeField] private float _floatDamageDistance = 80f;

        [Tooltip("Total duration in seconds of the float-up and fade-out animation")]
        [SerializeField] private float _floatDamageDuration = 0.9f;

        [Tooltip("Easing curve for the upward float movement")]
        [SerializeField] private Ease _floatDamageEase = Ease.OutCubic;

        
        public Color HitFlashColor => _hitFlashColor;
        public float HitFlashDuration => _hitFlashDuration;
        public Ease HitFlashEase => _hitFlashEase;
        public float KnockbackDistance => _knockbackDistance;
        public float KnockbackDuration => _knockbackDuration;
        public Ease KnockbackEase => _knockbackEase;
        public float ResultScreenDelay => _resultScreenDelay;
        public float HPBarLagDelay => _hpBarLagDelay;
        public float HPBarLagDuration => _hpBarLagDuration;
        public Ease HPBarLagEase => _hpBarLagEase;
        public float HPBarHealDuration => _hpBarHealDuration;
        public Ease HPBarHealEase => _hpBarHealEase;
        public float EnergyFillDuration => _energyFillDuration;
        public Ease EnergyFillEase => _energyFillEase;
        public Color SourceHighlightColor => _sourceHighlightColor;
        public Color AttackTargetColor => _attackTargetColor;
        public Color HealTargetColor => _healTargetColor;
        public Color DamageNumberColor => _damageNumberColor;
        public Color HealNumberColor => _healNumberColor;
        public float FloatDamageDistance => _floatDamageDistance;
        public float FloatDamageDuration => _floatDamageDuration;
        public Ease FloatDamageEase => _floatDamageEase;
        public float ReadyPulseDuration => _readyPulseDuration;
        public float ReadyPulseAlpha => _readyPulseAlpha;
        public Ease ReadyPulseEase => _readyPulseEase;
    }
}
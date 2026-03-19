using DG.Tweening;
using UnityEngine;

namespace Project.Scripts.Configs
{
    [CreateAssetMenu(fileName = "AnimationConfig", menuName = "Configs/Animation Config")]
    public class AnimationConfig : ScriptableObject
    {
        [Tooltip("Duration in seconds for the tile swap animation")]
        [SerializeField] private float _swapDuration = 0.2f;

        [Tooltip("Duration in seconds for the tile fall animation")]
        [SerializeField] private float _fallDuration = 0.15f;

        [Tooltip("Easing curve applied to falling tiles")]
        [SerializeField] private Ease _fallEase = Ease.OutBounce;

        [Tooltip("Duration in seconds for the tile destroy animation")]
        [SerializeField] private float _destroyDuration = 0.1f;

        [Tooltip("Easing curve applied to tile destruction")]
        [SerializeField] private Ease _destroyEase = Ease.InBack;

        [Tooltip("Duration in seconds for the tile spawn animation")]
        [SerializeField] private float _spawnDuration = 0.15f;

        
        public float SwapDuration => _swapDuration;
        public float FallDuration => _fallDuration;
        public Ease FallEase => _fallEase;
        public float DestroyDuration => _destroyDuration;
        public Ease DestroyEase => _destroyEase;
        public float SpawnDuration => _spawnDuration;
    }
}
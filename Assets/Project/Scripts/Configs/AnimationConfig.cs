using DG.Tweening;
using UnityEngine;

namespace Project.Scripts.Configs
{
    [CreateAssetMenu(fileName = "AnimationConfig", menuName = "Configs/Animation Config")]
    public class AnimationConfig : ScriptableObject
    {
        [SerializeField] private float _swapDuration = 0.2f;
        [SerializeField] private float _fallDuration = 0.15f;
        [SerializeField] private Ease _fallEase = Ease.OutBounce;
        [SerializeField] private float _destroyDuration = 0.1f;
        [SerializeField] private Ease _destroyEase = Ease.InBack;
        [SerializeField] private float _spawnDuration = 0.15f;

        
        public float SwapDuration => _swapDuration;
        public float FallDuration => _fallDuration;
        public Ease FallEase => _fallEase;
        public float DestroyDuration => _destroyDuration;
        public Ease DestroyEase => _destroyEase;
        public float SpawnDuration => _spawnDuration;
    }
}
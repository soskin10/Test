using DG.Tweening;
using UnityEngine;

namespace Project.Scripts.Configs.Board
{
    [CreateAssetMenu(fileName = "BoardAnimationConfig", menuName = "Configs/Board Animation Config")]
    public class BoardAnimationConfig : ScriptableObject
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

        [Tooltip("Pause in seconds between successive chain-reaction waves (bomb → line rune → next explosion, etc.)")]
        [SerializeField] private float _chainReactionWaveDelay = 0.12f;


        public float SwapDuration => _swapDuration;
        public float FallDuration => _fallDuration;
        public Ease FallEase => _fallEase;
        public float DestroyDuration => _destroyDuration;
        public Ease DestroyEase => _destroyEase;
        public float SpawnDuration => _spawnDuration;
        public float ChainReactionWaveDelay => _chainReactionWaveDelay;
    }
}
using Project.Scripts.Shared.Moves;
using UnityEngine;

namespace Project.Scripts.Configs.UI
{
    [CreateAssetMenu(fileName = "MoveBarConfig", menuName = "Configs/Move Bar Config")]
    public class MoveBarConfig : ScriptableObject
    {
        [Tooltip("Maximum number of move charges that can be stored")]
        [SerializeField] private int _maxMoves = 10;

        [Tooltip("Time in seconds to regenerate one move charge")]
        [SerializeField] private float _secondsPerMove = 2.5f;

        [Tooltip("Number of move charges available at the start of each battle")]
        [SerializeField] private int _startMoves = 4;

        [Header("Visual")]
        [Tooltip("Gap between segments as a fraction of total bar width (e.g. 0.01 = 1%)")]
        [SerializeField] private float _gapFraction = 0.01f;

        [Tooltip("Punch scale strength applied to a segment when a new move charge activates")]
        [SerializeField] private float _punchStrength = 0.15f;

        [Tooltip("Duration in seconds of the activation punch scale animation")]
        [SerializeField] private float _punchDuration = 0.2f;

        [Tooltip("Minimum alpha at the trough of the full-bar blink cycle (0..1)")]
        [SerializeField] private float _fullBlinkMinAlpha = 0.35f;

        [Tooltip("Duration in seconds of one blink half-cycle when the bar is at maximum")]
        [SerializeField] private float _fullBlinkHalfDuration = 0.45f;

        [Tooltip("Duration in seconds of the shake animation when a swap is attempted with no moves")]
        [SerializeField] private float _emptyShakeDuration = 0.3f;

        [Tooltip("Horizontal shake amplitude in pixels when a swap is attempted with no moves")]
        [SerializeField] private float _emptyShakeStrength = 8f;


        public int MaxMoves => _maxMoves;
        public float SecondsPerMove => _secondsPerMove;
        public int StartMoves => _startMoves;
        public float GapFraction => _gapFraction;
        public float PunchStrength => _punchStrength;
        public float PunchDuration => _punchDuration;
        public float FullBlinkMinAlpha => _fullBlinkMinAlpha;
        public float FullBlinkHalfDuration => _fullBlinkHalfDuration;
        public float EmptyShakeDuration => _emptyShakeDuration;
        public float EmptyShakeStrength => _emptyShakeStrength;


        public MoveBarSettings ToSettings()
        {
            return new MoveBarSettings(_maxMoves, _secondsPerMove, _startMoves);
        }
    }
}
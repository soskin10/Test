using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Scripts.Configs
{
    [CreateAssetMenu(fileName = "InputConfig", menuName = "Configs/Input Config")]
    public class InputConfig : ScriptableObject
    {
        [Tooltip("Unity Input System action asset defining all input bindings")]
        [SerializeField] private InputActionAsset _inputActionAsset;

        [Tooltip("Minimum drag distance in pixels before a swipe is registered")]
        [SerializeField] private float _screenDragThresholdPixels = 10f;

        [Tooltip("Minimum drag distance in world units before a swap direction is determined")]
        [SerializeField] private float _worldDragThreshold = 0.03f;

        
        public InputActionAsset InputActionAsset => _inputActionAsset;
        public float ScreenDragThresholdPixels => _screenDragThresholdPixels;
        public float WorldDragThreshold => _worldDragThreshold;
    }
}
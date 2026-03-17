using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Scripts.Configs
{
    [CreateAssetMenu(fileName = "InputConfig", menuName = "Configs/Input Config")]
    public class InputConfig : ScriptableObject
    {
        [SerializeField] private InputActionAsset _inputActionAsset;
        [SerializeField] private float _screenDragThresholdPixels = 20f;
        [SerializeField] private float _worldDragThreshold = 0.3f;

        
        public InputActionAsset InputActionAsset => _inputActionAsset;
        public float ScreenDragThresholdPixels => _screenDragThresholdPixels;
        public float WorldDragThreshold => _worldDragThreshold;
    }
}
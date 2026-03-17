using Cysharp.Threading.Tasks;
using Project.Scripts.Services.Audio.AudioSystem.Configs;
using Project.Scripts.Services.ServiceLocatorSystem;
using UnityEngine;

namespace Project.Scripts.Configs
{
    [CreateAssetMenu(fileName = "MainConfig", menuName = "Configs/MainConfig")]
    public class MainConfig : ScriptableObject, IService
    {
        [SerializeField] private AudioMusicConfig _audioMusicConfig;
        [SerializeField] private AudioSFXConfig _audioSFXConfig;
        [SerializeField] private InputConfig _inputConfig;
        [SerializeField] private BoardConfig _boardConfig;
        [SerializeField] private AnimationConfig _animationConfig;
        [SerializeField] private ScoreConfig _scoreConfig;


        public AudioMusicConfig AudioMusicConfig => _audioMusicConfig;
        public AudioSFXConfig AudioSFXConfig => _audioSFXConfig;
        public InputConfig InputConfig => _inputConfig;
        public BoardConfig BoardConfig => _boardConfig;
        public AnimationConfig AnimationConfig => _animationConfig;
        public ScoreConfig ScoreConfig => _scoreConfig;

        
        public UniTask InitAsync()
        {
            if (!_audioMusicConfig)
                Debug.LogError("AudioMusicConfig is missing in MainConfig!");
            if (!_audioSFXConfig)
                Debug.LogError("AudioSFXConfig is missing in MainConfig!");
            if (!_inputConfig)
                Debug.LogError("InputConfig is missing in MainConfig!");
            if (!_boardConfig)
                Debug.LogError("BoardConfig is missing in MainConfig!");
            if (!_animationConfig)
                Debug.LogError("AnimationConfig is missing in MainConfig!");
            if (!_scoreConfig)
                Debug.LogError("ScoreConfig is missing in MainConfig!");

            return UniTask.CompletedTask;
        }
    }
}
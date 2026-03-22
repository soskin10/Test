using Project.Scripts.Services.Audio.AudioSystem.Configs;
using UnityEngine;

namespace Project.Scripts.Configs
{
    [CreateAssetMenu(fileName = "MainConfig", menuName = "Configs/MainConfig")]
    public class MainConfig : ScriptableObject
    {
        [SerializeField] private AudioMusicConfig _audioMusicConfig;
        [SerializeField] private AudioSFXConfig _audioSFXConfig;
        [SerializeField] private InputConfig _inputConfig;
        [SerializeField] private BoardConfig _boardConfig;
        [SerializeField] private AnimationConfig _animationConfig;
        [SerializeField] private DamageConfig _damageConfig;
        [SerializeField] private SpecialTileConfig _specialTileConfig;
        [SerializeField] private LevelDatabase _levelDatabase;


        public AudioMusicConfig AudioMusicConfig => _audioMusicConfig;
        public AudioSFXConfig AudioSFXConfig => _audioSFXConfig;
        public InputConfig InputConfig => _inputConfig;
        public BoardConfig BoardConfig => _boardConfig;
        public AnimationConfig AnimationConfig => _animationConfig;
        public DamageConfig DamageConfig => _damageConfig;
        public SpecialTileConfig SpecialTileConfig => _specialTileConfig;
        public LevelDatabase LevelDatabase => _levelDatabase;
    }
}
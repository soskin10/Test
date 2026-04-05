using Project.Scripts.Configs.Battle;
using Project.Scripts.Configs.Board;
using Project.Scripts.Configs.Levels;
using Project.Scripts.Configs.UI;
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
        [SerializeField] private BoardAnimationConfig _boardAnimationConfig;
        [SerializeField] private BattleAnimationConfig _battleAnimationConfig;
        [SerializeField] private MatchDamageConfig _matchDamageConfig;
        [SerializeField] private SpecialTileConfig _specialTileConfig;
        [SerializeField] private LevelDatabase _levelDatabase;
        [SerializeField] private UIConfig _uiConfig;
        [SerializeField] private HeroEnergyConfig _heroEnergyConfig;
        [SerializeField] private MoveBarConfig _moveBarConfig;
        [SerializeField] private BattleViewConfig _battleViewConfig;
        [SerializeField] private TileKindPaletteConfig _tileKindPaletteConfig;


        public AudioMusicConfig AudioMusicConfig => _audioMusicConfig;
        public AudioSFXConfig AudioSFXConfig => _audioSFXConfig;
        public InputConfig InputConfig => _inputConfig;
        public BoardConfig BoardConfig => _boardConfig;
        public BoardAnimationConfig BoardAnimationConfig => _boardAnimationConfig;
        public BattleAnimationConfig BattleAnimationConfig => _battleAnimationConfig;
        public MatchDamageConfig MatchDamageConfig => _matchDamageConfig;
        public SpecialTileConfig SpecialTileConfig => _specialTileConfig;
        public LevelDatabase LevelDatabase => _levelDatabase;
        public UIConfig UIConfig => _uiConfig;
        public HeroEnergyConfig HeroEnergyConfig => _heroEnergyConfig;
        public MoveBarConfig MoveBarConfig => _moveBarConfig;
        public BattleViewConfig BattleViewConfig => _battleViewConfig;
        public TileKindPaletteConfig TileKindPaletteConfig => _tileKindPaletteConfig;
    }
}
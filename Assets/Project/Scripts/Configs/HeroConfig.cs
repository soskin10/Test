using Project.Scripts.Shared.Heroes;
using Project.Scripts.Shared.Tiles;
using UnityEngine;

namespace Project.Scripts.Configs
{
    [CreateAssetMenu(fileName = "HeroConfig", menuName = "Configs/Hero Config")]
    public class HeroConfig : ScriptableObject
    {
        [Tooltip("Tile element type of this hero — determines which energy type fills this hero")]
        [SerializeField] private TileKind _kind;

        [Tooltip("Energy required to activate this hero's ability")]
        [SerializeField] private int _maxEnergy = 10;

        [Tooltip("What this hero does when activated")]
        [SerializeField] private HeroActionType _actionType;

        [Tooltip("Amount of damage dealt (DealDamage) or HP restored (HealAlly) on activation")]
        [SerializeField] private int _actionValue = 20;

        [Tooltip("Portrait sprite displayed in the hero slot (null = empty frame)")]
        [SerializeField] private Sprite _portrait;

        [Tooltip("Maximum HP of this hero. Zero means the hero is immortal (cannot be damaged)")]
        [SerializeField] private int _maxHP = 50;

        [Tooltip("Display name of the hero, for future UI labels")]
        [SerializeField] private string _displayName;


        public TileKind Kind => _kind;
        public int MaxEnergy => _maxEnergy;
        public HeroActionType ActionType => _actionType;
        public int ActionValue => _actionValue;
        public int MaxHP => _maxHP;
        public Sprite Portrait => _portrait;
        public string DisplayName => _displayName;
    }
}
using Project.Scripts.Shared.Bot;
using UnityEngine;

namespace Project.Scripts.Configs.Battle
{
    [CreateAssetMenu(fileName = "BotConfig", menuName = "Configs/Bot Config")]
    public class BotConfig : ScriptableObject
    {
        [Header("Debug")]
        [Tooltip("Uncheck to disable bot during analytics recording; does not affect registration")]
        [SerializeField] private bool _enabled = true;

        [Header("Identity")]
        [Tooltip("Name displayed for the opponent in battle UI")]
        [SerializeField] private string _opponentName = "Enemy";

        [Header("Attack")]
        [Tooltip("Minimum seconds between bot attacks on the player")]
        [SerializeField] private float _minAttackInterval = 1.5f;

        [Tooltip("Maximum seconds between bot attacks on the player")]
        [SerializeField] private float _maxAttackInterval = 3.5f;

        [Tooltip("Minimum damage dealt per bot attack")]
        [SerializeField] private int _minAttackDamage = 8;

        [Tooltip("Maximum damage dealt per bot attack")]
        [SerializeField] private int _maxAttackDamage = 26;

        [Header("Hero Energy")]
        [Tooltip("Seconds between each hero energy tick for the bot")]
        [SerializeField] private float _heroEnergyTickInterval = 0.8f;

        [Tooltip("Energy added to a random hero slot per tick")]
        [SerializeField] private int _heroEnergyPerTick = 2;

        [Header("Hero Activation")]
        [Tooltip("Minimum seconds the bot waits after a hero charges before activating it (simulates human reaction)")]
        [SerializeField] private float _minHeroActivationDelay = 1.0f;

        [Tooltip("Maximum seconds the bot waits after a hero charges before activating it")]
        [SerializeField] private float _maxHeroActivationDelay = 4.0f;

        [Header("Avatar Charge")]
        [Tooltip("Seconds between each enemy avatar charge tick")]
        [SerializeField] private float _enemyChargeTickInterval = 2f;

        [Tooltip("Charge points added to enemy avatar per tick")]
        [SerializeField] private int _enemyChargePerTick = 8;

        [Tooltip("Minimum seconds the bot waits after charge bar fills before discharging (simulates human reaction)")]
        [SerializeField] private float _minDischargeDelay = 0.5f;

        [Tooltip("Maximum seconds the bot waits after charge bar fills before discharging")]
        [SerializeField] private float _maxDischargeDelay = 2.0f;

        [Header("Heroes")]
        [Tooltip("If true, bot randomly selects 4 heroes from HeroPool at battle start; if false, uses LevelConfig enemy heroes")]
        [SerializeField] private bool _randomHeroSelection = true;

        [Tooltip("Pool of heroes the bot can randomly pick from when RandomHeroSelection is enabled")]
        [SerializeField] private HeroConfig[] _heroPool;


        public bool Enabled => _enabled;
        public string OpponentName => _opponentName;
        public float MinHeroActivationDelay => _minHeroActivationDelay;
        public float MaxHeroActivationDelay => _maxHeroActivationDelay;
        public float HeroEnergyTickInterval => _heroEnergyTickInterval;
        public int HeroEnergyPerTick => _heroEnergyPerTick;
        public float EnemyChargeTickInterval => _enemyChargeTickInterval;
        public int EnemyChargePerTick => _enemyChargePerTick;
        public float MinDischargeDelay => _minDischargeDelay;
        public float MaxDischargeDelay => _maxDischargeDelay;
        public bool RandomHeroSelection => _randomHeroSelection;
        public HeroConfig[] HeroPool => _heroPool;

        
        public BotSettings ToSettings()
        {
            return new BotSettings(_minDischargeDelay, _maxDischargeDelay);
        }
    }
}
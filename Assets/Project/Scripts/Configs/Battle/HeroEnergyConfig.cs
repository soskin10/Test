using UnityEngine;

namespace Project.Scripts.Configs.Battle
{
    [CreateAssetMenu(fileName = "HeroEnergyConfig", menuName = "Configs/Battle/Hero Energy Config")]
    public class HeroEnergyConfig : ScriptableObject
    {
        [Tooltip("Maximum energy capacity for each tile type")]
        [SerializeField] private int _maxEnergyPerType = 20;

        [Tooltip("Maximum charge the player and enemy avatar bars can hold")]
        [SerializeField] private int _maxAvatarCharge = 110;


        public int MaxEnergyPerType => _maxEnergyPerType;
        public int MaxAvatarCharge => _maxAvatarCharge;
    }
}

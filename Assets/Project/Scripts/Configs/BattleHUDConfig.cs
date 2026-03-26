using UnityEngine;

namespace Project.Scripts.Configs
{
    [CreateAssetMenu(fileName = "BattleHUDConfig", menuName = "Configs/BattleHUD Config")]
    public class BattleHUDConfig : ScriptableObject
    {
        [Tooltip("Portrait sprite shown for the player avatar (null = no sprite)")]
        [SerializeField] private Sprite _playerAvatarSprite;

        [Tooltip("Portrait sprite shown for the enemy avatar (null = no sprite)")]
        [SerializeField] private Sprite _enemyAvatarSprite;

        [Tooltip("Fill color of the player HP bar")]
        [SerializeField] private Color _playerHPBarColor = new Color(0.22f, 0.78f, 0.22f);

        [Tooltip("Fill color of the enemy HP bar")]
        [SerializeField] private Color _enemyHPBarColor = new Color(0.85f, 0.18f, 0.18f);


        public Sprite PlayerAvatarSprite => _playerAvatarSprite;
        public Sprite EnemyAvatarSprite => _enemyAvatarSprite;
        public Color PlayerHPBarColor => _playerHPBarColor;
        public Color EnemyHPBarColor => _enemyHPBarColor;
    }
}

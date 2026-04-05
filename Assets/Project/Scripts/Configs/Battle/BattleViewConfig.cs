using UnityEngine;

namespace Project.Scripts.Configs.Battle
{
    [CreateAssetMenu(fileName = "BattleViewConfig", menuName = "Configs/Battle/Battle View Config")]
    public class BattleViewConfig : ScriptableObject
    {
        [Tooltip("Prefab containing the WorldBattleHUDView component - world-space battle view root")]
        [SerializeField] private GameObject _battleHUDViewPrefab;

        [Tooltip("World-space Y offset added above the board top edge to position the battle HUD root")]
        [SerializeField] private float _battleAreaTopPadding = 0.4f;

        [Tooltip("Portrait sprite shown for the player avatar (null = no sprite)")]
        [SerializeField] private Sprite _playerAvatarSprite;

        [Tooltip("Portrait sprite shown for the enemy avatar (null = no sprite)")]
        [SerializeField] private Sprite _enemyAvatarSprite;


        public GameObject BattleHUDViewPrefab => _battleHUDViewPrefab;
        public float BattleAreaTopPadding => _battleAreaTopPadding;
        public Sprite PlayerAvatarSprite => _playerAvatarSprite;
        public Sprite EnemyAvatarSprite => _enemyAvatarSprite;
    }
}
using UnityEngine;

namespace Project.Scripts.Configs
{
    [CreateAssetMenu(fileName = "UIConfig", menuName = "Configs/UI Config")]
    public class UIConfig : ScriptableObject
    {
        [Tooltip("Prefab containing the GameplayView component — shown during gameplay")]
        [SerializeField] private GameObject _gameplayViewPrefab;

        [Tooltip("Prefab containing the WinView component — shown when the player defeats the enemy")]
        [SerializeField] private GameObject _winViewPrefab;

        [Tooltip("Prefab containing the LoseView component — shown when the player runs out of moves")]
        [SerializeField] private GameObject _loseViewPrefab;

        [Tooltip("Prefab containing the MoveBarView component — docked to the bottom of the screen")]
        [SerializeField] private GameObject _moveBarViewPrefab;

        [Tooltip("Prefab containing the BattleHUDView component — shows player and enemy avatars with HP bars")]
        [SerializeField] private GameObject _battleHUDViewPrefab;


        public GameObject GameplayViewPrefab => _gameplayViewPrefab;
        public GameObject WinViewPrefab => _winViewPrefab;
        public GameObject LoseViewPrefab => _loseViewPrefab;
        public GameObject MoveBarViewPrefab => _moveBarViewPrefab;
        public GameObject BattleHUDViewPrefab => _battleHUDViewPrefab;
    }
}
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


        public GameObject GameplayViewPrefab => _gameplayViewPrefab;
        public GameObject WinViewPrefab => _winViewPrefab;
        public GameObject LoseViewPrefab => _loseViewPrefab;
    }
}
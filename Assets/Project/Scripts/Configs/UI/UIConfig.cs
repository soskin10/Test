using UnityEngine;

namespace Project.Scripts.Configs.UI
{
    [CreateAssetMenu(fileName = "UIConfig", menuName = "Configs/UI Config")]
    public class UIConfig : ScriptableObject
    {
        [Tooltip("Prefab containing the WinView component — shown when the player defeats the enemy")]
        [SerializeField] private GameObject _winViewPrefab;

        [Tooltip("Prefab containing the LoseView component — shown when the player runs out of moves")]
        [SerializeField] private GameObject _loseViewPrefab;

        [Tooltip("Prefab containing the MoveBarView component — docked to the bottom of the screen")]
        [SerializeField] private GameObject _moveBarViewPrefab;

        [Tooltip("Prefab containing the TopBarView component — enemy name and secondary label, stays in Canvas")]
        [SerializeField] private GameObject _topBarViewPrefab;


        public GameObject WinViewPrefab => _winViewPrefab;
        public GameObject LoseViewPrefab => _loseViewPrefab;
        public GameObject MoveBarViewPrefab => _moveBarViewPrefab;
        public GameObject TopBarViewPrefab => _topBarViewPrefab;
    }
}
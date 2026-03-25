using Cysharp.Threading.Tasks;
using Project.Scripts.Services.UISystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.Gameplay.UI
{
    public class WinView : BaseView<WinViewModel>
    {
        [Tooltip("Text displaying the number of moves used to win")]
        [SerializeField] private TMP_Text _movesText;

        [Tooltip("Text displaying the total damage dealt to the enemy")]
        [SerializeField] private TMP_Text _damageText;

        [Tooltip("Button that advances to the next level")]
        [SerializeField] private Button _nextLevelButton;


        protected override UniTask OnBindViewModel()
        {
            _movesText.text = $"Moves used: {ViewModel.MovesUsed}";
            _damageText.text = $"Total damage: {ViewModel.TotalDamage}";
            _nextLevelButton.onClick.AddListener(ViewModel.NextLevel);
            return UniTask.CompletedTask;
        }

        protected override void OnClose()
        {
            _nextLevelButton.onClick.RemoveAllListeners();
        }
    }
}
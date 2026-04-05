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

        [Tooltip("Text displaying the current level ID")]
        [SerializeField] private TMP_Text _levelIdText;

        [Tooltip("Text displaying the defeated opponent's name")]
        [SerializeField] private TMP_Text _opponentNameText;

        [Tooltip("Button that advances to the next level")]
        [SerializeField] private Button _nextLevelButton;


        protected override bool EnablePumpAnimation => true;


        protected override UniTask OnBindViewModel()
        {
            _movesText.text = ViewModel.MovesUsed.ToString();
            _levelIdText.text = ViewModel.LevelId.ToString();
            _opponentNameText.text = ViewModel.OpponentName;
            _nextLevelButton.onClick.AddListener(ViewModel.NextLevel);
            return UniTask.CompletedTask;
        }

        protected override void OnClose()
        {
            _nextLevelButton.onClick.RemoveAllListeners();
        }
    }
}
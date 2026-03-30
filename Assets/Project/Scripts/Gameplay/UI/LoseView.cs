using Cysharp.Threading.Tasks;
using Project.Scripts.Services.UISystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.Gameplay.UI
{
    public class LoseView : BaseView<LoseViewModel>
    {
        [Tooltip("Text displaying the number of moves used")]
        [SerializeField] private TMP_Text _movesText;

        [Tooltip("Text displaying the total damage dealt to the enemy")]
        [SerializeField] private TMP_Text _damageText;

        [Tooltip("Text displaying the current level ID")]
        [SerializeField] private TMP_Text _levelIdText;

        [Tooltip("Text displaying the opponent's name")]
        [SerializeField] private TMP_Text _opponentNameText;

        [Tooltip("Button that retries the current level")]
        [SerializeField] private Button _retryButton;


        protected override bool EnablePumpAnimation => true;


        protected override UniTask OnBindViewModel()
        {
            _movesText.text = ViewModel.MovesUsed.ToString();
            _damageText.text = ViewModel.TotalDamage.ToString();
            _levelIdText.text = ViewModel.LevelId.ToString();
            _opponentNameText.text = ViewModel.OpponentName;
            _retryButton.onClick.AddListener(ViewModel.Retry);
            return UniTask.CompletedTask;
        }

        protected override void OnClose()
        {
            _retryButton.onClick.RemoveAllListeners();
        }
    }
}

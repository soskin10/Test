using Cysharp.Threading.Tasks;
using Project.Scripts.Services.UISystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.Gameplay.UI
{
    public class LoseView : BaseView<LoseViewModel>
    {
        [Tooltip("Text displaying the reason for losing")]
        [SerializeField] private TMP_Text _messageText;

        [Tooltip("Button that retries the current level")]
        [SerializeField] private Button _retryButton;


        protected override UniTask OnBindViewModel()
        {
            _messageText.text = "You lost!";
            _retryButton.onClick.AddListener(ViewModel.Retry);
            return UniTask.CompletedTask;
        }

        protected override void OnClose()
        {
            _retryButton.onClick.RemoveAllListeners();
        }
    }
}
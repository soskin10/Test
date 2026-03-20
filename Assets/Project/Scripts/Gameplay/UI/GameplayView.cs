using Cysharp.Threading.Tasks;
using Project.Scripts.Services.UISystem;
using R3;
using TMPro;
using UnityEngine;

namespace Project.Scripts.Gameplay.UI
{
    public class GameplayView : BaseView<GameplayViewModel>
    {
        [Tooltip("Text element displaying the last dealt damage")]
        [SerializeField] private TMP_Text _damageText;


        protected override UniTask OnBindViewModel()
        {
            ViewModel.LastDamage
                .Subscribe(v => _damageText.text = $"Damage: {v}")
                .AddTo(Disposables);

            return UniTask.CompletedTask;
        }
    }
}
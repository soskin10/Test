using Cysharp.Threading.Tasks;
using Project.Scripts.Services.UISystem;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.Gameplay.UI
{
    public class GameplayView : BaseView<GameplayViewModel>
    {
        [Tooltip("Text element displaying current level number")]
        [SerializeField] private TMP_Text _currentLevelText;

        [Tooltip("Text element displaying remaining enemy HP")]
        [SerializeField] private TMP_Text _enemyHpText;

        [Tooltip("Text element displaying the last dealt damage")]
        [SerializeField] private TMP_Text _damageText;

        [Tooltip("Fill image for Fire energy bar (0..1 fill amount)")]
        [SerializeField] private Image _fireEnergyBar;

        [Tooltip("Fill image for Water energy bar (0..1 fill amount)")]
        [SerializeField] private Image _waterEnergyBar;

        [Tooltip("Fill image for Nature energy bar (0..1 fill amount)")]
        [SerializeField] private Image _natureEnergyBar;

        [Tooltip("Fill image for Light energy bar (0..1 fill amount)")]
        [SerializeField] private Image _lightEnergyBar;

        [Tooltip("Fill image for Void energy bar (0..1 fill amount)")]
        [SerializeField] private Image _voidEnergyBar;


        protected override UniTask OnBindViewModel()
        {
            _currentLevelText.text = $"Current Level: {ViewModel.CurrentLevel}";
            
            ViewModel.EnemyHP
                .Subscribe(v => _enemyHpText.text = $"Enemy HP: {v}")
                .AddTo(Disposables);
            
            ViewModel.LastDamage
                .Subscribe(v => _damageText.text = $"Damage: {v}")
                .AddTo(Disposables);

            return UniTask.CompletedTask;
        }
    }
}
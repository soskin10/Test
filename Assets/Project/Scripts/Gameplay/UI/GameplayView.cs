using Cysharp.Threading.Tasks;
using Project.Scripts.Services.UISystem;
using R3;
using TMPro;
using UnityEngine;

namespace Project.Scripts.Gameplay.UI
{
    public class GameplayView : BaseView<GameplayViewModel>
    {
        [Tooltip("Text element displaying current level number")]
        [SerializeField] private TMP_Text _currentLevelText;

        [Tooltip("Text element displaying remaining enemy HP")]
        [SerializeField] private TMP_Text _enemyHpText;

        [Tooltip("Text element displaying remaining moves")]
        [SerializeField] private TMP_Text _movesLeftText;
        
        [Tooltip("Text element displaying the last dealt damage")]
        [SerializeField] private TMP_Text _damageText;


        protected override UniTask OnBindViewModel()
        {
            _currentLevelText.text = $"Current Level: {ViewModel.CurrentLevel}";
            
            ViewModel.EnemyHP
                .Subscribe(v => _enemyHpText.text = $"Enemy HP: {v}")
                .AddTo(Disposables);
            
            ViewModel.LastDamage
                .Subscribe(v => _damageText.text = $"Damage: {v}")
                .AddTo(Disposables);

            ViewModel.MovesLeft
                .Subscribe(v => _movesLeftText.text = $"Moves Left: {v}")
                .AddTo(Disposables);

            return UniTask.CompletedTask;
        }
    }
}
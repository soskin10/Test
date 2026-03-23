using System;
using Cysharp.Threading.Tasks;
using Project.Scripts.Services;
using Project.Scripts.Services.Combat;
using Project.Scripts.Services.UISystem;

namespace Project.Scripts.Gameplay.UI
{
    public class WinViewModel : BaseViewModel
    {
        public int MovesRemaining { get; private set; }
        public int TotalDamage { get; private set; }


        private readonly IMoveCounterService _moveCounter;
        private readonly IEnemyStateService _enemyState;
        private readonly ILevelProgressionService _progression;
        private readonly Action _onClose;


        public WinViewModel(
            IMoveCounterService moveCounter,
            IEnemyStateService enemyState,
            ILevelProgressionService progression,
            Action onClose)
        {
            _moveCounter = moveCounter;
            _enemyState = enemyState;
            _progression = progression;
            _onClose = onClose;
        }


        public void NextLevel()
        {
            _onClose?.Invoke();
            _progression.Advance();
        }


        protected override UniTask OnInitializeAsync()
        {
            MovesRemaining = _moveCounter.RemainingMoves;
            TotalDamage = _enemyState.MaxHP;
            return UniTask.CompletedTask;
        }
    }
}
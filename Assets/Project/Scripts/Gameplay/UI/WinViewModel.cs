using System;
using Cysharp.Threading.Tasks;
using Project.Scripts.Services;
using Project.Scripts.Services.Combat;
using Project.Scripts.Services.UISystem;

namespace Project.Scripts.Gameplay.UI
{
    public class WinViewModel : BaseViewModel
    {
        public int MovesUsed { get; private set; }
        public int LevelId { get; private set; }
        public string OpponentName { get; private set; }


        private readonly IMoveCounterService _moveCounter;
        private readonly ILevelProgressionService _progression;
        private readonly Action _onClose;


        public WinViewModel(
            IMoveCounterService moveCounter,
            ILevelProgressionService progression,
            int levelId,
            string opponentName,
            Action onClose)
        {
            _moveCounter = moveCounter;
            _progression = progression;
            LevelId = levelId;
            OpponentName = opponentName;
            _onClose = onClose;
        }


        public void NextLevel()
        {
            _onClose?.Invoke();
            _progression.Advance();
        }


        protected override UniTask OnInitializeAsync()
        {
            MovesUsed = _moveCounter.MovesUsed;
            return UniTask.CompletedTask;
        }
    }
}
using System;
using Project.Scripts.Services;
using Project.Scripts.Services.UISystem;

namespace Project.Scripts.Gameplay.UI
{
    public class LoseViewModel : BaseViewModel
    {
        private readonly ILevelProgressionService _progression;
        private readonly Action _onClose;


        public LoseViewModel(ILevelProgressionService progression, Action onClose)
        {
            _progression = progression;
            _onClose = onClose;
        }


        public void Retry()
        {
            _onClose?.Invoke();
            _progression.Retry();
        }
    }
}
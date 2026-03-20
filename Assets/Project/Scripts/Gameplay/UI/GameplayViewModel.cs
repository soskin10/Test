using Cysharp.Threading.Tasks;
using Project.Scripts.Services.EventBusSystem;
using Project.Scripts.Services.EventBusSystem.Events;
using Project.Scripts.Services.UISystem;
using R3;

namespace Project.Scripts.Gameplay.UI
{
    public class GameplayViewModel : BaseViewModel
    {
        private readonly EventBus _eventBus;


        public ReactiveProperty<int> LastDamage { get; } = new(0);


        public GameplayViewModel(EventBus eventBus)
        {
            _eventBus = eventBus;
        }


        protected override UniTask OnInitializeAsync()
        {
            Disposables.Add(_eventBus.Subscribe<DamageDealtEvent>(OnDamageDealt));
            return UniTask.CompletedTask;
        }

        protected override void OnCleanup()
        {
            LastDamage.Dispose();
        }


        private void OnDamageDealt(DamageDealtEvent e)
        {
            LastDamage.Value = e.Total;
        }
    }
}
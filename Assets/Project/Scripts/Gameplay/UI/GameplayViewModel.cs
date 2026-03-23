using Cysharp.Threading.Tasks;
using Project.Scripts.Configs;
using Project.Scripts.Services.Combat;
using Project.Scripts.Services.EventBusSystem;
using Project.Scripts.Services.EventBusSystem.Events;
using Project.Scripts.Services.UISystem;
using R3;

namespace Project.Scripts.Gameplay.UI
{
    public class GameplayViewModel : BaseViewModel
    {
        private readonly EventBus _eventBus;
        private readonly IEnemyStateService _enemyState;
        private readonly IMoveCounterService _moveCounter;
        private readonly LevelConfig _levelConfig;


        public ReactiveProperty<int> LastDamage { get; } = new(0);
        public ReactiveProperty<int> EnemyHP { get; } = new(0);
        public ReactiveProperty<int> MovesLeft { get; } = new(0);
        public int CurrentLevel { get; private set; }


        public GameplayViewModel(EventBus eventBus, IEnemyStateService enemyState, IMoveCounterService moveCounter,
            LevelConfig levelConfig)
        {
            _eventBus = eventBus;
            _enemyState = enemyState;
            _moveCounter = moveCounter;
            _levelConfig = levelConfig;
        }


        protected override UniTask OnInitializeAsync()
        {
            CurrentLevel = _levelConfig.LevelId;
            EnemyHP.Value = _enemyState.CurrentHP;
            MovesLeft.Value = _moveCounter.RemainingMoves;

            Disposables.Add(_eventBus.Subscribe<DamageDealtEvent>(OnDamageDealt));
            Disposables.Add(_eventBus.Subscribe<EnemyHPChangedEvent>(OnEnemyHPChanged));
            Disposables.Add(_eventBus.Subscribe<MoveCountChangedEvent>(OnMoveCountChanged));
            
            return UniTask.CompletedTask;
        }

        protected override void OnCleanup()
        {
            LastDamage.Dispose();
            EnemyHP.Dispose();
            MovesLeft.Dispose();
        }


        private void OnDamageDealt(DamageDealtEvent e) => LastDamage.Value = e.Total;
        
        private void OnEnemyHPChanged(EnemyHPChangedEvent e) => EnemyHP.Value = e.Current;
        
        private void OnMoveCountChanged(MoveCountChangedEvent e) => MovesLeft.Value = e.Remaining;
    }
}
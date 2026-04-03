using Cysharp.Threading.Tasks;
using Project.Scripts.Configs;
using Project.Scripts.Services.Combat;
using Project.Scripts.Services.EventBusSystem;
using Project.Scripts.Services.EventBusSystem.Events;
using Project.Scripts.Services.UISystem;
using Project.Scripts.Shared.Tiles;
using R3;

namespace Project.Scripts.Gameplay.UI
{
    public class GameplayViewModel : BaseViewModel
    {
        public ReactiveProperty<int> LastDamage { get; } = new(0);
        public ReactiveProperty<int> EnemyHP { get; } = new(0);
        public int CurrentLevel { get; private set; }
        public ReactiveProperty<float> FireEnergy { get; } = new(0f);
        public ReactiveProperty<float> WaterEnergy { get; } = new(0f);
        public ReactiveProperty<float> NatureEnergy { get; } = new(0f);
        public ReactiveProperty<float> LightEnergy { get; } = new(0f);
        public ReactiveProperty<float> VoidEnergy { get; } = new(0f);
        
        
        private readonly EventBus _eventBus;
        private readonly IEnemyStateService _enemyState;
        private readonly LevelConfig _levelConfig;
        private readonly IEnergyService _energyService;


        public GameplayViewModel(EventBus eventBus, IEnemyStateService enemyState,
            LevelConfig levelConfig, IEnergyService energyService)
        {
            _eventBus = eventBus;
            _enemyState = enemyState;
            _levelConfig = levelConfig;
            _energyService = energyService;
        }


        protected override UniTask OnInitializeAsync()
        {
            CurrentLevel = _levelConfig.LevelId;
            EnemyHP.Value = _enemyState.CurrentHP;

            Disposables.Add(_eventBus.Subscribe<AbilityExecutedEvent>(OnAbilityExecuted));
            Disposables.Add(_eventBus.Subscribe<EnemyHPChangedEvent>(OnEnemyHPChanged));
            Disposables.Add(_eventBus.Subscribe<EnergyChangedEvent>(OnEnergyChanged));

            return UniTask.CompletedTask;
        }

        protected override void OnCleanup()
        {
            LastDamage.Dispose();
            EnemyHP.Dispose();
            FireEnergy.Dispose();
            WaterEnergy.Dispose();
            NatureEnergy.Dispose();
            LightEnergy.Dispose();
            VoidEnergy.Dispose();
        }


        private void OnAbilityExecuted(AbilityExecutedEvent e)
        {
            LastDamage.Value = e.Value;
        }

        private void OnEnemyHPChanged(EnemyHPChangedEvent e)
        {
            EnemyHP.Value = e.Current;
        }

        private void OnEnergyChanged(EnergyChangedEvent e)
        {
            var normalized = e.Max > 0 ? (float)e.Current / e.Max : 0f;
            switch (e.Kind)
            {
                case TileKind.Fire: FireEnergy.Value = normalized; break;
                case TileKind.Water: WaterEnergy.Value = normalized; break;
                case TileKind.Nature: NatureEnergy.Value = normalized; break;
                case TileKind.Light: LightEnergy.Value = normalized; break;
                case TileKind.Void: VoidEnergy.Value = normalized; break;
            }
        }
    }
}
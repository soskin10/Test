using Project.Scripts.Configs;
using Project.Scripts.Configs.Levels;
using Project.Scripts.Gameplay;
using Project.Scripts.Gameplay.UI;
using Project.Scripts.Services;
using Project.Scripts.Services.Bot;
using Project.Scripts.Services.Combat;
using VContainer;
using VContainer.Unity;

namespace Project.Scripts.DI
{
    public class GameplayLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            var levelDatabase = Parent.Container.Resolve<LevelDatabase>();
            var levelConfig = levelDatabase.GetById(LevelProgressionService.CurrentLevelId);
            builder.RegisterInstance(levelConfig);

            builder.RegisterComponentInHierarchy<GameplayEntryPoint>();

            builder.Register<IGameStateService, GameStateService>(Lifetime.Singleton);
            builder.Register<IMoveCounterService, MoveCounterService>(Lifetime.Singleton);
            builder.Register<IEnemyStateService, EnemyStateService>(Lifetime.Singleton);
            builder.Register<IPlayerStateService, PlayerStateService>(Lifetime.Singleton);
            builder.Register<ILevelProgressionService, LevelProgressionService>(Lifetime.Singleton);
            builder.Register<IEnergyService, EnergyService>(Lifetime.Singleton);
            builder.Register<IMoveBarService, MoveBarService>(Lifetime.Singleton);
            builder.Register<IHeroService, HeroService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<PlayerAvatarChargeService>().As<IPlayerAvatarChargeService>();
            builder.Register<IEnemyAvatarChargeService, EnemyAvatarChargeService>(Lifetime.Singleton);
            builder.Register<IAbilityExecutionService, AbilityExecutionService>(Lifetime.Singleton);

            builder.Register<MoveBarViewModel>(Lifetime.Singleton);
            builder.Register<BattleHUDViewModel>(Lifetime.Singleton);
            builder.Register<GameResultPresenter>(Lifetime.Singleton);
            builder.Register<IReadyPulseCoordinator, ReadyPulseCoordinator>(Lifetime.Singleton);

            builder.Register<IBoardBoundsProvider, BoardBoundsProvider>(Lifetime.Singleton);

            if (levelConfig.BotConfig)
            {
                builder.RegisterInstance(levelConfig.BotConfig);
                builder.RegisterEntryPoint<BotOpponentService>().As<IBotOpponentService>();
            }
        }
    }
}
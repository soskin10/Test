using Project.Scripts.Gameplay;
using Project.Scripts.Gameplay.UI;
using VContainer;
using VContainer.Unity;

namespace Project.Scripts.DI
{
    public class GameplayLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<GameplayEntryPoint>();
            builder.Register<GameplayViewModel>(Lifetime.Singleton);
        }
    }
}
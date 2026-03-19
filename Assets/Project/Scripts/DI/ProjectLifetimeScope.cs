using Project.Scripts.Configs;
using Project.Scripts.Services.Audio.AudioSystem;
using Project.Scripts.Services.Damage;
using Project.Scripts.Services.EventBusSystem;
using Project.Scripts.Services.UISystem;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Project.Scripts.DI
{
    public class ProjectLifetimeScope : LifetimeScope
    {
        [SerializeField] private MainConfig _mainConfig;


        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_mainConfig.BoardConfig);
            builder.RegisterInstance(_mainConfig.AnimationConfig);
            builder.RegisterInstance(_mainConfig.InputConfig);
            builder.RegisterInstance(_mainConfig.DamageConfig);
            builder.RegisterInstance(_mainConfig.AudioMusicConfig);
            builder.RegisterInstance(_mainConfig.AudioSFXConfig);

            builder.Register<IDamageCalculator, DamageCalculator>(Lifetime.Singleton);

            builder.Register<EventBus>(Lifetime.Singleton);
            builder.Register<AudioService>(Lifetime.Singleton);

            builder.RegisterComponentInHierarchy<AudioManager>();
            builder.RegisterComponentInHierarchy<UIService>();
        }
    }
}
using _.Scripts.Entry_Point;
using _.Scripts.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _.Scripts.Installers
{
    public class GameplayInstaller : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<UserInterfaceService>(Lifetime.Scoped).AsSelf();
            builder.Register<SessionService>(Lifetime.Scoped).AsImplementedInterfaces().AsSelf();
            
            builder.RegisterEntryPoint<GameplayEntryPoint>();
        }
    }
}
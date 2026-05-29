using _.Scripts.Scriptable_Objects;
using _.Scripts.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _.Scripts.Installers
{
    public class RootInstaller : LifetimeScope
    {
        [SerializeField] private ProjectSettings _projectSettings;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance<ProjectSettings>(_projectSettings).AsSelf();
            builder.Register<DataService>(Lifetime.Singleton).AsSelf();
        }
    }
}
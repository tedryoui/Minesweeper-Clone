using _.Scripts.Entry_Point;
using _.Scripts.Models;
using _.Scripts.Scriptable_Objects;
using _.Scripts.Services;
using _.Scripts.Utility;
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
            _projectSettings.CacheEverything();
            
            var dataService = new DataService();

            var playerModel = new PlayerModel();
            var sessionModel = new SessionModel();
            var settingsModel = new SettingsModel();
            
            dataService.AddModel(MODEL_IDENTITIES.PLAYER_MODEL, playerModel);
            dataService.AddModel(MODEL_IDENTITIES.SESSION_MODEL, sessionModel);
            dataService.AddModel(MODEL_IDENTITIES.SETTINGS_MODEL, settingsModel);

            builder.RegisterInstance<ProjectSettings>(_projectSettings).AsSelf();
            builder.RegisterInstance<DataService>(dataService).AsSelf();
            builder.Register<UserInterfaceService>(Lifetime.Singleton).AsSelf();
            builder.Register<VolumeService>(Lifetime.Singleton).AsSelf();
            
            builder.RegisterEntryPoint<RootEntryPoint>();
        }
    }
}
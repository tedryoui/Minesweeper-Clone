using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using _.Scripts.Scriptable_Objects;
using _.Scripts.Services;
using _.Scripts.Utility;
using DG.Tweening;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace _.Scripts.Entry_Point
{
    public class GameplayEntryPoint : IPostInitializable
    {
#region VContainer

        [Inject] private ProjectSettings      _projectSettings;
        [Inject] private UserInterfaceService _userInterfaceService;
        [Inject] private VolumeService        _volumeService;
        [Inject] private SessionService       _sessionService;

#endregion

        public void PostInitialize()
        {
            CreateMenuUserInterfaces().ContinueWith(_ =>
            {
                _userInterfaceService.Show(USER_INTERFACE_IDENTITIES.MENU_UI);
                
                _volumeService.Bind(USER_INTERFACE_IDENTITIES.SETTINGS_UI);
                _sessionService.Bind(USER_INTERFACE_IDENTITIES.GAMEPLAY_PRESET_UI);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

#region User Interface Managing

        private async Task CreateMenuUserInterfaces()
        {
            await AsyncInstantiateUI(USER_INTERFACE_IDENTITIES.MENU_UI);
            await AsyncInstantiateUI(USER_INTERFACE_IDENTITIES.SETTINGS_UI);
            await AsyncInstantiateUI(USER_INTERFACE_IDENTITIES.GAMEPLAY_PRESET_UI);
            await AsyncInstantiateUI(USER_INTERFACE_IDENTITIES.CONFIRMATION_UI);
        }

        private async Task AsyncInstantiateUI(string identity)
        {
            var assetReference          = FindUIAssetReference(identity);

            var loadedAsset = await assetReference.LoadAssetAsync<GameObject>().Task;

            var gameObject = await Object.InstantiateAsync(loadedAsset);
            
            var result = gameObject.First();
            
            if (result.TryGetComponent(out AbstractUserInterface userInterface))
                _userInterfaceService.RegisterUserInterface(identity, userInterface);
            else 
                throw new Exception($"{identity} user interface was not found.");
        }
        
        private AssetReference FindUIAssetReference(string identity)
        {
            if (!_projectSettings.UserInterfaces.TryGetValue(identity, out var assetReference))
                throw new Exception($"{identity} user interface was not found.");
            
            return assetReference;
        }

#endregion
    }
}
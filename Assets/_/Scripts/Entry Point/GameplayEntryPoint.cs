using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _.Scripts.Scriptable_Objects;
using _.Scripts.Services;
using _.Scripts.Utility;
using NUnit.Framework;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace _.Scripts.Entry_Point
{
    public class GameplayEntryPoint : IPostInitializable
    {
        [Inject] private ProjectSettings      _projectSettings;
        [Inject] private UserInterfaceService _userInterfaceService;

        public void PostInitialize()
        {
            RegisterMenuUserInterfaces().ContinueWith(_ =>
            {
                _userInterfaceService.Show(USER_INTERFACE_IDENTITIES.MENU_UI);
                
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private async Task RegisterMenuUserInterfaces()
        {
            await InstantiateUserInterface(USER_INTERFACE_IDENTITIES.MENU_UI);
            await InstantiateUserInterface(USER_INTERFACE_IDENTITIES.SETTINGS_UI);
            await InstantiateUserInterface(USER_INTERFACE_IDENTITIES.GAMEPLAY_PRESET_UI);
            await InstantiateUserInterface(USER_INTERFACE_IDENTITIES.CONFIRMATION_UI);
            
        }

        private async Task InstantiateUserInterface(string identity)
        {
            var prefab          = FindUserInterface(identity);
            var instantiateTask = Object.InstantiateAsync(prefab, _userInterfaceService.Root);
            
            await instantiateTask;

            var result = instantiateTask.Result[0];
            _userInterfaceService.RegisterUserInterface(identity, result);
        }

        private AbstractUserInterface FindUserInterface(string identity)
        {
            var pair = _projectSettings.UserInterfacePairs.FirstOrDefault(x => x.Identity.Equals(identity));
            
            if (string.IsNullOrEmpty(pair.Identity))
                throw new Exception($"{identity} user interface was not found.");
            
            return pair.UserInterface;
        }
    }
}
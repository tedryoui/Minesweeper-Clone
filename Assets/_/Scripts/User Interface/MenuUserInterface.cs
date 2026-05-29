using System;
using _.Scripts.Services;
using _.Scripts.Utility;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _.Scripts.User_Interface
{
    public class MenuUserInterface : AbstractUserInterface
    {
        private UserInterfaceService _userInterfaceService;
        
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton;
        
        [Inject]
        public void Configure(UserInterfaceService userInterfaceService)
        {
            _userInterfaceService = userInterfaceService;
            
            _playButton.onClick.AddListener(OnPlayButtonClicked);
            _settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        }

        private void OnSettingsButtonClicked()
        {
            _userInterfaceService.Show(USER_INTERFACE_IDENTITIES.SETTINGS_UI);
        }

        private void OnPlayButtonClicked()
        {
            _userInterfaceService.Show(USER_INTERFACE_IDENTITIES.GAMEPLAY_PRESET_UI);
        }
        
        public override void Show()
        {
            gameObject.SetActive(true);
        }

        public override void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
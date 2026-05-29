using System;
using _.Scripts.Services;
using _.Scripts.Utility;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _.Scripts.User_Interface
{
    public class LoseUserInterface : AbstractUserInterface
    {
        private UserInterfaceService _userInterfaceService;
        
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _menuButton;
        
        private event Action _onMenuButtonClicked;
        private event Action _onRestartButtonClicked;

        [Inject]
        private void Configure(UserInterfaceService userInterfaceService)
        {
            _userInterfaceService = userInterfaceService;
            
            _restartButton.onClick.AddListener(OnRestartButtonClicked);
            _menuButton.onClick.AddListener(OnMenuButtonClicked);
        }
        
        private void OnMenuButtonClicked()
        {
            _userInterfaceService.Hide(USER_INTERFACE_IDENTITIES.LOSE_UI);
            _userInterfaceService.Hide(USER_INTERFACE_IDENTITIES.SESSION_UI);
            _userInterfaceService.Show(USER_INTERFACE_IDENTITIES.MENU_UI);
            
            _onMenuButtonClicked?.Invoke();
        }

        public void OnMenuButtonClicked(Action callback)
        {
            _onMenuButtonClicked = callback;
        }
        
        private void OnRestartButtonClicked()
        {
            _userInterfaceService.Hide(USER_INTERFACE_IDENTITIES.LOSE_UI);
            
            _onRestartButtonClicked?.Invoke();
        }

        public void OnRestartButtonClicked(Action callback)
        {
            _onRestartButtonClicked = callback;
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
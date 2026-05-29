using System;
using _.Scripts.Services;
using _.Scripts.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _.Scripts.User_Interface
{
    public class WinUserInterface : AbstractUserInterface
    {
        private UserInterfaceService _userInterfaceService;
        
        [SerializeField] private TextMeshProUGUI _scoreTMP;
        [SerializeField] private Button          _menuButton;

        private event Action _onMenuButtonClicked;

        private void Awake()
        {
            _onMenuButtonClicked = delegate { };
        }

        [Inject]
        private void Configure(UserInterfaceService userInterfaceService)
        {
            _userInterfaceService = userInterfaceService;
            
            _menuButton.onClick.AddListener(OnMenuButtonClicked);
        }

        public void SetScore(int score)
        {
            _scoreTMP.text = $"{score} pts.";
        }

        private void OnMenuButtonClicked()
        {
            _userInterfaceService.Hide(USER_INTERFACE_IDENTITIES.SESSION_UI);
            _userInterfaceService.Hide(USER_INTERFACE_IDENTITIES.WIN_UI);
            _userInterfaceService.Show(USER_INTERFACE_IDENTITIES.MENU_UI);
            
            _onMenuButtonClicked?.Invoke();
        }

        public void OnMenuButtonClicked(Action callback)
        {
            _onMenuButtonClicked = callback;
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
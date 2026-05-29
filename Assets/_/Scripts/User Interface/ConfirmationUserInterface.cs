using System;
using _.Scripts.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _.Scripts.User_Interface
{
    public class ConfirmationUserInterface : AbstractUserInterface
    {
        private UserInterfaceService _userInterfaceService;
        
        [SerializeField] private TextMeshProUGUI _titleTMP;
        [SerializeField] private Button          _acceptButton;
        [SerializeField] private Button          _dismissButton;
        
        private event Action _onAcceptButtonClicked;
        private event Action _onDismissButtonClicked;

        private void Awake()
        {
            _onAcceptButtonClicked  = delegate { };
            _onDismissButtonClicked = delegate { };
        }

        [Inject]
        private void Configure(UserInterfaceService userInterfaceService)
        {
            _userInterfaceService = userInterfaceService;
            
            _acceptButton.onClick.AddListener(OnAcceptButtonClicked);
            _dismissButton.onClick.AddListener(OnDismissButtonClicked);
        }
        
        private void OnAcceptButtonClicked()
        {
            _onAcceptButtonClicked?.Invoke();
        }

        public void OnAcceptButtonClicked(Action callback)
        {
            _onAcceptButtonClicked = callback;
        }

        private void OnDismissButtonClicked()
        {
            _onDismissButtonClicked?.Invoke();
        }

        public void OnDismissButtonClicked(Action callback)
        {
            _onDismissButtonClicked = callback;
        }

        public void SetTitle(string title)
        {
            _titleTMP.text = title;
        }
        
        public override void Show()
        {
            gameObject.SetActive(true);
        }

        public override void Hide()
        {
            _onAcceptButtonClicked = delegate { };
            _onDismissButtonClicked = delegate { };
            
            gameObject.SetActive(false);
        }
    }
}
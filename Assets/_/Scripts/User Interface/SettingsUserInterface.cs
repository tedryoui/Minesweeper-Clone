using _.Scripts.Services;
using _.Scripts.UI;
using _.Scripts.Utility;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _.Scripts.User_Interface
{
    public class SettingsUserInterface : AbstractUserInterface
    {
        private UserInterfaceService _userInterfaceService;

        [SerializeField] private Button _backButton;
        
        [SerializeField] private SnapSlider _volumeSlider;

        private int2 VolumeSliderLimits => new int2(0, 5); 

        [Inject]
        private void Configure(UserInterfaceService userInterfaceService)
        {
            _userInterfaceService = userInterfaceService;
            
            _volumeSlider.SetLimits(VolumeSliderLimits.x, VolumeSliderLimits.y);
            
            _backButton.onClick.AddListener(OnBackButtonClicked);
            _volumeSlider.OnValueChanged += OnVolumeSliderValueChanged;
        }

        private void OnVolumeSliderValueChanged(int currentValue)
        {
            var percentage = currentValue / (float)VolumeSliderLimits.y;
            
            Debug.Log("Setting volume: " + (int)(percentage * 100) + "%.");
        }

        private void OnBackButtonClicked()
        {
            _userInterfaceService.Hide(USER_INTERFACE_IDENTITIES.SETTINGS_UI);
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
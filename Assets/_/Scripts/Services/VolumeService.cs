using System;
using System.Threading.Tasks;
using _.Scripts.Models;
using _.Scripts.Scriptable_Objects;
using _.Scripts.User_Interface;
using _.Scripts.Utility;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Audio;
using VContainer;

namespace _.Scripts.Services
{
    public class VolumeService
    {
#region VContainer

        [Inject] private UserInterfaceService _userInterfaceService;
        [Inject] private ProjectSettings      _projectSettings;

        [Inject]
        private void Configure(DataService dataService)
        {
            _settingsModel = dataService.GetModel<SettingsModel>(MODEL_IDENTITIES.SETTINGS_MODEL);

            LoadAudioMixer();
        }

#endregion

#region Fields

        public  AudioSource   audioSource;

        private AudioMixer    _audioMixer;
        private SettingsModel _settingsModel;

#endregion

#region Events

        private event Action _onVolumeChanged;

#endregion

        public VolumeService()
        {
            _onVolumeChanged = delegate { };
        }

#region Private Methods

#region Settings User Interface (VIEW_MODEL CONTEXT)

        private void BindSettingsUserInterface()
        {
            SetVolumeSliderLimits();
            SetVolumeSliderCallbacks();
            
            _userInterfaceService.OnUserInterfaceRemoved += UnbindSettingsUserInterface;
        }
        
        private void UnbindSettingsUserInterface(string identity)
        {
            if (identity.Equals(USER_INTERFACE_IDENTITIES.SETTINGS_UI))
            {
                RemoveVolumeSliderCallbacks();
                
                _userInterfaceService.OnUserInterfaceRemoved -= UnbindSettingsUserInterface;
            }
        }

        private void SetVolumeSliderLimits()
        {
            var ui = _userInterfaceService.GetUserInterface<SettingsUserInterface>(USER_INTERFACE_IDENTITIES.SETTINGS_UI);
            var volumeSliderLimits = new int2(0, _projectSettings.VolumeSteps);
            
            ui.SetVolumeSliderLimits(volumeSliderLimits.x, volumeSliderLimits.y);
        }

        private void SetVolumeSliderCallbacks()
        {
            var ui = _userInterfaceService.GetUserInterface<SettingsUserInterface>(USER_INTERFACE_IDENTITIES.SETTINGS_UI);

            ui.OnVolumeSliderValueChanged(SetVolumeSegmented);
            _onVolumeChanged += SetVolumeSliderValue;
            
            _onVolumeChanged?.Invoke();
        }

        private void RemoveVolumeSliderCallbacks()
        {
            _onVolumeChanged -= SetVolumeSliderValue;
        }

        private void SetVolumeSliderValue()
        {
            var ui = _userInterfaceService.GetUserInterface<SettingsUserInterface>(USER_INTERFACE_IDENTITIES.SETTINGS_UI);

            var uiSliderValue   = ui.VolumeSliderValue / (float)_projectSettings.VolumeSteps;
            var dataSliderValue = _settingsModel.VolumePercentage;

            if (Math.Abs(uiSliderValue - dataSliderValue) > 0.05f)
            {
                var intValue = (int)math.round(dataSliderValue * _projectSettings.VolumeSteps);
                
                ui.SetVolumeSliderValue(intValue);
            }
        }

#endregion

        private async Task LoadAudioMixer()
        {
            var assetReference = _projectSettings.AudioMixerAssetReference;

            var result = await assetReference.LoadAssetAsync<AudioMixer>().Task;

            _audioMixer = result;
        }
        
#endregion
        
#region Public API

        public void Bind(params string[] userInterfaceIdentities)
        {
            foreach (var identity in userInterfaceIdentities)
            {
                if (!_userInterfaceService.HasUserInterface(identity))
                    continue;
                
                switch (identity)
                {
                    case USER_INTERFACE_IDENTITIES.SETTINGS_UI:
                        BindSettingsUserInterface();
                        break;
                    default:
                        continue;
                }
            }
        }

        public void SetVolumeSegmented(int value)
        {
            var steps = _projectSettings.VolumeSteps;
            var percentage = value / (float)steps;
            var clampValue = math.clamp(percentage, 0f, 1f);
            
            _settingsModel.VolumePercentage = clampValue;
            _audioMixer?.SetFloat("Volume", math.lerp(_projectSettings.VolumeRange.x, _projectSettings.VolumeRange.y, clampValue));
            
            _onVolumeChanged?.Invoke();
        }

        public void PlayClip(AudioClip clip)
        {
            audioSource.PlayOneShot(clip);
        }

#endregion
    }
}
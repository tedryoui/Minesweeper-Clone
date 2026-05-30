using System;
using _.Scripts.Extensions;
using _.Scripts.Services;
using _.Scripts.Utility;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace _.Scripts.User_Interface
{
    public class MenuUserInterface : AbstractUserInterface
    {
#region VContainer

        [Inject] private UserInterfaceService _userInterfaceService;
        [Inject] private VolumeService        _volumeService;
        
        [Inject]
        private void Configure()
        {
            _playButton.onClick.AddListener(OnPlayButtonClicked);
            _settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        }

#endregion

#region Scene References

        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton;

#endregion

#region Fields

        [SerializeField] private AudioClip _panelShowSFX;
        [SerializeField] private AudioClip _panelHideSFX;
        [SerializeField] private AudioClip _clickSFX;
        [SerializeField] private AudioClip _hoverSFX;

#endregion
        
#region Animation Setup

        [Space(10)]
        [Header("Animation")]
        [SerializeField] private float _buttonScaleFactor = 0.95f;
        [SerializeField] private float _buttonScaleDuration = 0.125f;
        [SerializeField] private Ease  _buttonScaleEase     = Ease.InOutQuad;
        [Space(5)]
        [SerializeField] private float _buttonRotationStrength = 0.5f;
        [SerializeField] private float _buttonRotationDuration   = 0.5f;
        [SerializeField] private int   _buttonRotationVibratio   = 1;
        [SerializeField] private float _buttonRotationElasticity = 10f;

#endregion

        private void Start()
        {
            (_settingsButton.transform as RectTransform).OnPointerEnter(OnSettingsButtonEnter);
            (_playButton.transform as RectTransform).OnPointerEnter(OnPlayButtonEnter);
        }

#region Private Methods

        private void OnSettingsButtonClicked()
        {
            _volumeService.PlayClip(_clickSFX);
            
            PlayButtonClickAnimation(_settingsButton, () =>
            {
                _userInterfaceService.Show(USER_INTERFACE_IDENTITIES.SETTINGS_UI);
            });
        }

        private void OnPlayButtonClicked()
        {
            _volumeService.PlayClip(_clickSFX);

            PlayButtonClickAnimation(_playButton, () =>
            {
                _userInterfaceService.Show(USER_INTERFACE_IDENTITIES.GAMEPLAY_PRESET_UI);
            });
        }

        private void OnSettingsButtonEnter(PointerEventData pointerEventData)
        {
            _volumeService.PlayClip(_hoverSFX);

            PlayButtonEnterAnimation(_settingsButton);
        }
        
        private void OnPlayButtonEnter(PointerEventData pointerEventData)
        {
            _volumeService.PlayClip(_hoverSFX);

            PlayButtonEnterAnimation(_playButton);
        }

#region Animations

        public Tween PlayButtonClickAnimation(Button button, Action onCompleted = null, bool autoPlay = true)
        {
            if (DOTween.IsTweening($"{button.GetEntityId()}_button_scale"))
                DOTween.Complete($"{button.GetEntityId()}_button_scale");

            var baseScale = button.transform.localScale;

            Tween tween = default;
            tween = button.transform
                .DOScale(baseScale * _buttonScaleFactor, _buttonScaleDuration)
                .SetEase(_buttonScaleEase)
                .SetLoops(2, LoopType.Yoyo)
                .OnStepComplete(() =>
                {
                    if (tween == null)
                        return;
                    if (tween.CompletedLoops() == 1)
                        onCompleted?.Invoke();
                } )
                .OnComplete(() =>
                {
                    button.transform.localScale = baseScale;
                })
                .SetLink(button.gameObject)
                .SetId($"{button.GetEntityId()}_button_scale")
                .Pause();

            if (autoPlay)
                tween.Play();
                
            return tween;
        }

        public Tween PlayButtonEnterAnimation(Button button, bool autoPlay = true)
        {
            if (DOTween.IsTweening($"{button.GetEntityId()}_button_rotate"))
                DOTween.Complete($"{button.GetEntityId()}_button_rotate");

            var baseRotation = button.transform.rotation;
            
            Tween tween = button.transform
                .DOPunchRotation(Vector3.forward * _buttonRotationStrength, _buttonRotationDuration, _buttonRotationVibratio, _buttonRotationElasticity)
                .OnComplete(() => button.transform.rotation = baseRotation)
                .SetLink(button.gameObject)
                .SetId($"{button.GetEntityId()}_button_rotate")
                .Pause();
            
            if (autoPlay)
                tween.Play();
            
            return tween;
        }

#endregion

#endregion

#region Parent overrides

        public override void Show(bool animate = true)
        {
            gameObject.SetActive(true);
        }

        public override void Hide(bool animate = true)
        {
            gameObject.SetActive(false);
        }
        
#endregion
    }
}
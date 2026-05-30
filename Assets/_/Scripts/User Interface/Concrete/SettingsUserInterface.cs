using System;
using _.Scripts.Extensions;
using _.Scripts.Services;
using _.Scripts.UI;
using _.Scripts.Utility;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace _.Scripts.User_Interface
{
    public class SettingsUserInterface : AbstractUserInterface
    {
#region VContainer

        [Inject] private UserInterfaceService _userInterfaceService;
        [Inject] private VolumeService        _volumeService;

        [Inject]
        private void Configure()
        {
            _backButton.onClick.AddListener(OnBackButtonClicked);
            _volumeSlider.OnValueChanged += OnVolumeSliderValueChanged;
        }

#endregion

#region Properties

        public int VolumeSliderValue => _volumeSlider.Value;

#endregion

#region Scene References

        [SerializeField] private Button     _backButton;
        [SerializeField] private SnapSlider _volumeSlider;

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
        
        [SerializeField] private CanvasGroup _canvasGroup;
        [Space(5)]
        [SerializeField] private float _fadeInDuration = 0.25f;
        [SerializeField] private Ease  _fadeInEase     = Ease.OutQuad;
        [Space(5)]
        [SerializeField] private float _fadeOutDuration = 0.25f;
        [SerializeField] private Ease  _fadeOutEase     = Ease.OutQuad;
        [Space(10)]
        [SerializeField] private RectTransform _rotatePanel;
        [Space(5)]
        [SerializeField] private float _panelPunchRotateStrength = 5f;
        [SerializeField] private float _panelPunchRotateDuration   = 0.5f;
        [SerializeField] private int   _panelPunchRotateVibratio   = 10;
        [SerializeField] private float _panelPunchRotateElasticity = 1f;
        [Space(10)]
        [SerializeField] private float _buttonScaleFactor   = 0.95f;
        [SerializeField] private float _buttonScaleDuration = 0.125f;
        [SerializeField] private Ease  _buttonScaleEase     = Ease.InOutQuad;
        [Space(5)]
        [SerializeField] private float _buttonRotationStrength   = 5f;
        [SerializeField] private float _buttonRotationDuration   = 0.5f;
        [SerializeField] private int   _buttonRotationVibratio   = 10;
        [SerializeField] private float _buttonRotationElasticity = 1f;
        [Space(5)]
        [SerializeField] private float _snapSliderScaleFactor = 1.1f;
        [SerializeField] private float _snapSliderScaleDuration = 0.25f;
        [SerializeField] private Ease  _snapSliderScaleEase     = Ease.OutQuart;
        [SerializeField] private float _snapSliderScaleDelay = 0.05f;
        [Space(10)]
        [SerializeField] private float _panelPunchRotationOffset = 0.3f;
        [SerializeField] private float _hideFadeOutDelay = 0.25f;

#endregion

#region Events

        private event Action<int> _volumeSliderValueChanged;

#endregion

        private void Awake()
        {
            _volumeSliderValueChanged = delegate { };
        }
        
        private void Start()
        {
            (_backButton.transform as RectTransform).OnPointerEnter(OnBackButtonEnter);
        }

#region Private Methods

        private void OnVolumeSliderValueChanged(int currentValue)
        {
            _volumeService.PlayClip(_clickSFX);
            
            _volumeSliderValueChanged?.Invoke(currentValue);

            PlaySnapSliderLinerPunchScaleAnimation();
        }

        private void OnBackButtonClicked()
        {
            _volumeService.PlayClip(_clickSFX);
            
            PlayButtonClickAnimation(_backButton, () =>
            {
                _userInterfaceService.Hide(USER_INTERFACE_IDENTITIES.SETTINGS_UI);
            });
        }

        private void OnBackButtonEnter(PointerEventData obj)
        {
            _volumeService.PlayClip(_hoverSFX);
            
            PlayButtonEnterAnimation(_backButton);
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

            var baseRotation = button.transform.localRotation;
            
            Tween tween = button.transform
                .DOPunchRotation(Vector3.forward * _buttonRotationStrength, _buttonRotationDuration, _buttonRotationVibratio, _buttonRotationElasticity)
                .OnComplete(() => button.transform.localRotation = baseRotation)
                .SetLink(button.gameObject)
                .SetId($"{button.GetEntityId()}_button_rotate")
                .Pause();
            
            if (autoPlay)
                tween.Play();
            
            return tween;
        }
        
        private Tween PlayFadeInAnimation(Action onComplete = null, bool autoPlay = true)
        {
            if (DOTween.IsTweening($"{_canvasGroup.GetEntityId()}_fadeIn"))
                return null;
            
            _canvasGroup.interactable   = false;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.alpha          = 0f;

            var tween = _canvasGroup
                .DOFade(1.0f, _fadeInDuration)
                .SetEase(_fadeInEase)
                .OnComplete(() =>
                {
                    _canvasGroup.interactable   = true;
                    _canvasGroup.blocksRaycasts = true;
                    _canvasGroup.alpha          = 1f;
                    
                    onComplete?.Invoke();
                })
                .Pause()
                .SetLink(_canvasGroup.gameObject)
                .SetTarget(_canvasGroup)
                .SetId($"{_canvasGroup.GetEntityId()}_fadeIn");

            if (autoPlay)
                tween.Play();
            
            return tween;
        }
        
        private Tween PlayFadeOutAnimation(Action onComplete = null, bool autoPlay = true)
        {
            if (DOTween.IsTweening($"{_canvasGroup.GetEntityId()}_fadeOut"))
                DOTween.Complete($"{_canvasGroup.GetEntityId()}_fadeOut");
            
            _canvasGroup.interactable   = false;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.alpha          = 1f;

            var tween = _canvasGroup
                .DOFade(0.0f, _fadeInDuration)
                .SetEase(_fadeInEase)
                .OnComplete(() =>
                {
                    _canvasGroup.interactable   = false;
                    _canvasGroup.blocksRaycasts = false;
                    _canvasGroup.alpha          = 0f;
                    
                    onComplete?.Invoke();
                })
                .Pause()
                .SetLink(_canvasGroup.gameObject)
                .SetId($"{_canvasGroup.GetEntityId()}_fadeOut");
            
            if (autoPlay)
                tween.Play();
            
            return tween;
        }

        private Tween PlayPanelPunchRotationAnimation(bool autoPlay = true)
        {
            if (DOTween.IsTweening($"{_rotatePanel.GetEntityId()}_punch_rotate"))
                DOTween.Complete($"{_rotatePanel.GetEntityId()}_punch_rotate");
            
            var baseRotation = _rotatePanel.transform.rotation;
            
            var tween = _rotatePanel
                .DOPunchRotation(Vector3.forward * _panelPunchRotateStrength, _panelPunchRotateDuration, _panelPunchRotateVibratio, _panelPunchRotateElasticity)
                .OnComplete(() => _rotatePanel.transform.rotation = baseRotation)
                .SetLink(_rotatePanel.gameObject)
                .SetTarget(_rotatePanel)
                .SetId($"{_rotatePanel.GetEntityId()}_punch_rotate")
                .Pause();
            
            if (autoPlay)
                tween.Play();

            return tween;
        }

        private Sequence PlaySnapSliderLinerPunchScaleAnimation(bool autoPlay = true)
        {
            if (DOTween.IsTweening($"{_volumeSlider.GetEntityId()}_liner_punch_scale"))
                DOTween.Complete($"{_volumeSlider.GetEntityId()}_liner_punch_scale");

            var sequence = DOTween.Sequence();
            sequence
                .SetId($"{_volumeSlider.GetEntityId()}_liner_punch_scale")
                .SetTarget(_volumeSlider)
                .SetLink(_volumeSlider.gameObject)
                .Pause();

            for (var i = 0; i < _volumeSlider.Segments.Count; i++)
            {
                if (i > _volumeSlider.Value)
                    break;
                
                if (DOTween.IsTweening($"{_volumeSlider.Segments[i].GetEntityId()}_liner_punch_scale"))
                    DOTween.Complete($"{_volumeSlider.Segments[i].GetEntityId()}_liner_punch_scale");
                
                var segment   = _volumeSlider.Segments[i];
                var baseScale = segment.transform.localScale;

                Tween tween = default;
                tween = segment
                    .DOScale(baseScale * _snapSliderScaleFactor, _snapSliderScaleDuration)
                    .SetEase(_snapSliderScaleEase)
                    .OnComplete(() => segment!.localScale = baseScale)
                    .SetLoops(2, LoopType.Yoyo)
                    .SetLink(segment.gameObject)
                    .SetTarget(segment)
                    .SetId($"{segment.GetEntityId()}_liner_punch_scale")
                    .Pause();

                sequence.Insert(i * _snapSliderScaleDelay, tween);
            }

            if (autoPlay)
                sequence.Play();

            return sequence;
        }

#endregion

#endregion

#region Public API

        public void OnVolumeSliderValueChanged(Action<int> callback)
        {
            _volumeSliderValueChanged = callback;
        }

        public void SetVolumeSliderValue(int value)
        {
            _volumeSlider.Value = value;
        }

        public void SetVolumeSliderLimits(int min, int max)
        {
            _volumeSlider.SetLimits(min, max);
        }

#endregion

#region Parent overrides

        public override void Show(bool animate = true)
        {
            gameObject.SetActive(true);
            
            var fadeInTween             = PlayFadeInAnimation(() =>
            {
                if (animate)
                    _volumeService.PlayClip(_panelShowSFX);
            }, autoPlay: false);
            var panelPunchRotationTween = PlayPanelPunchRotationAnimation(autoPlay: false);

            if (fadeInTween != null && panelPunchRotationTween != null)
            {
                var sequence = DOTween.Sequence();
                sequence
                    .SetTarget(gameObject)
                    .SetLink(gameObject)
                    .SetId($"{_canvasGroup.GetEntityId()}_fadeIn");
            
                sequence.Insert(0.0f,                      fadeInTween);
                sequence.Insert(_panelPunchRotationOffset, panelPunchRotationTween);

                sequence.Play();
                
                if (!animate)
                    sequence.Complete();
            }
        }

        public override void Hide(bool animate = true)
        {
            if (animate)
                _volumeService.PlayClip(_panelHideSFX);
            
            var tween = PlayFadeOutAnimation(() =>
            {
                gameObject.SetActive(false);
            }, false).SetDelay(_hideFadeOutDelay).Play();
            
            if (!animate)
                tween.Complete();
        }

#endregion
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using _.Scripts.Extensions;
using _.Scripts.Services;
using _.Scripts.UI;
using _.Scripts.Utility;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace _.Scripts.User_Interface
{
    public class GameplayPresetUserInterface : AbstractUserInterface
    {
#region VContainer

        [Inject] private UserInterfaceService _userInterfaceService;
        [Inject] private VolumeService        _volumeService;

        [Inject]
        private void Configure()
        {
            StartChoosePanel();
            StartCustomPanel();
        }

#endregion

#region Scene References

        [Header("Choose Panel")]
        [SerializeField] private RectTransform _choosePanel;
        [SerializeField] private ToggleGroup   _chooseToggleGroup;
        
        [Space(5)]
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _playChosenButton;
        [SerializeField] private Button _customPresetButton;
        
        [Space(5)]
        [SerializeField] private CacheIdentityPairArray<Toggle> _chooseToggles;
        
        [Space(10)]
        [Header("Custom Panel")]
        [SerializeField] private RectTransform   _customPanel;

        [Space(5)]
        [SerializeField] private SnapSlider      _widthSnapSlider;
        [SerializeField] private SnapSlider      _heightSnapSlider;
        
        [Space(5)]
        [SerializeField] private TextMeshProUGUI _mineCountTMP;
        [SerializeField] private TextMeshProUGUI _timerValueTMP;
        
        [Space(5)]
        [SerializeField] private Button          _backButton;
        [SerializeField] private Button          _playCustomButton;
        [SerializeField] private Button          _increaseMineCountButton;
        [SerializeField] private Button          _decreaseMineCountButton;
        [SerializeField] private Button          _nextButton;
        [SerializeField] private Button          _previousButton;

#endregion

#region Fields

        [SerializeField] private AudioClip _panelShowSFX;
        [SerializeField] private AudioClip _panelHideSFX;
        [SerializeField] private AudioClip _clickSFX;
        [SerializeField] private AudioClip _hoverSFX;
        
        private Dictionary<Toggle, string> _backTogglesCache;

        private float _baseXChosePanelPosition;
        private float _baseXCustomPanelPosition;

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
        [SerializeField] private float _anchorPositionDuration = 0.5f;
        [SerializeField] private Ease  _anchorPositionEase     = Ease.InOutQuad;
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
        [SerializeField] private float _snapSliderScaleDelay    = 0.05f;
        [Space(5)]
        [SerializeField] private Color _textWarningColor = Color.orange;
        [SerializeField] private float _textWarningDuration = 0.25f;
        [SerializeField] private Ease  _textWarningEase     = Ease.InOutQuad;
        [Space(10)]
        [SerializeField] private float _panelPunchRotationOffset = 0.3f;
        [SerializeField] private float _hideFadeOutDelay = 0.25f;

#endregion
        
#region Events

        private event Action         _onPlayButtonClicked;
        private event Action         _onCustomPresetButtonClicked;
        private event Action<string> _onChoosePresetToggleActiveChanged;
        private event Action         _onBackButtonClicked;
        private event Action         _onIncreaseMineCountButtonClicked;
        private event Action         _onDecreaseMineCountButtonClicked;
        private event Action         _onPreviousButtonClicked;
        private event Action         _onNextButtonClicked;
        public event  Action<int>    _onWidthSnapSliderValueChanged;
        public event  Action<int>    _onHeightSnapSliderValueChanged;

#endregion
        
        private void Awake()
        {
            _baseXChosePanelPosition  = _choosePanel.anchoredPosition.x;
            _baseXCustomPanelPosition = _customPanel.anchoredPosition.x;
            
            _onPlayButtonClicked         = delegate { };
            _onCustomPresetButtonClicked = delegate { };
            
            _onChoosePresetToggleActiveChanged = delegate { };
            _onBackButtonClicked               = delegate { };
            
            _onIncreaseMineCountButtonClicked = delegate { };
            _onDecreaseMineCountButtonClicked = delegate { };
            
            _onPreviousButtonClicked = delegate { };
            _onNextButtonClicked     = delegate { };

            _onWidthSnapSliderValueChanged  = delegate { };
            _onHeightSnapSliderValueChanged = delegate { };
        }

        private void Start()
        {
            (_customPresetButton.transform as RectTransform).OnPointerEnter(OnCustomPresetButtonPointerEnter);
            (_closeButton.transform as RectTransform).OnPointerEnter(OnCloseButtonPointerEnter);
            (_playChosenButton.transform as RectTransform).OnPointerEnter(OnPlayChosenButtonPointerEnter);

            (_backButton.transform as RectTransform).OnPointerEnter(OnBackButtonPointerEnter);
            (_playCustomButton.transform as RectTransform).OnPointerEnter(OnPlayCustomButtonPointerEnter);
            (_increaseMineCountButton.transform as RectTransform).OnPointerEnter(OnIncreaseMineCountButtonPointerEnter);
            (_decreaseMineCountButton.transform as RectTransform).OnPointerEnter(OnDecreaseMineCountButtonPointerEnter);
            (_nextButton.transform as RectTransform).OnPointerEnter(OnNextButtonPointerEnter);
            (_previousButton.transform as RectTransform).OnPointerEnter(OnPreviousButtonPointerEnter);
        }

#region Private Methods

#region Choose Panel

        private void StartChoosePanel()
        {
            CacheToggles();
            
            BindChoosePanelButtonsClickEvent();
            BindTogglesSelectEvent();
        }

        private void CacheToggles()
        {
            _chooseToggles.Cache();
            _backTogglesCache = _chooseToggles.Values
                .ToDictionary(
                    k => k.Value,
                    v => v.Key
                );
        }

        private void BindChoosePanelButtonsClickEvent()
        {
            _closeButton.onClick.AddListener(OnCloseButtonClicked);                                                           
            _playChosenButton.onClick.AddListener(OnPlayButtonClicked);                                                       
            _customPresetButton.onClick.AddListener(OnCustomPresetButtonClicked);
        }

        private void OnCustomPresetButtonClicked()
        {
            _volumeService.PlayClip(_clickSFX);
            
            PlayRectTransformClickAnimation(_customPresetButton.transform as RectTransform, () =>
            {
                _onCustomPresetButtonClicked?.Invoke();

                ShowCustomPanel();
            });
        }

        private void OnCloseButtonClicked()
        {
            _volumeService.PlayClip(_clickSFX);

            PlayRectTransformClickAnimation(_closeButton.transform as RectTransform, () =>
            {
                _userInterfaceService.Hide(USER_INTERFACE_IDENTITIES.GAMEPLAY_PRESET_UI);
            });
        }

        private void BindTogglesSelectEvent()
        {
            foreach (var chooseToggle in _chooseToggles.Values.Values)
            {
                chooseToggle.onValueChanged.AddListener(OnChoosePresetToggleValueChanged);

                (chooseToggle.transform as RectTransform).OnPointerEnter(_ => OnTogglePointerEnter(chooseToggle.transform as RectTransform));
                (chooseToggle.transform as RectTransform).OnPointerClick(_ => OnTogglePointerClick(chooseToggle.transform as RectTransform));
            }
        }

        private void OnTogglePointerClick(RectTransform rectTransform)
        {
            _volumeService.PlayClip(_clickSFX);

            PlayRectTransformClickAnimation(rectTransform);
        }

        private void OnTogglePointerEnter(RectTransform rectTransform)
        {
            _volumeService.PlayClip(_hoverSFX);

            PlayRectTransformEnterAnimation(rectTransform);
        }

        private void OnChoosePresetToggleValueChanged(bool value)
        {
            var activeToggle = _chooseToggleGroup.GetFirstActiveToggle();

            if (activeToggle == null)
                activeToggle = _chooseToggles.Values.Values.First(x => x.isOn);

            var identity = _backTogglesCache[activeToggle];

            _onChoosePresetToggleActiveChanged?.Invoke(identity);
        }

        private void OnCloseButtonPointerEnter(PointerEventData pointerEventData)
        {
            _volumeService.PlayClip(_hoverSFX);

            PlayRectTransformEnterAnimation(_closeButton.transform as RectTransform);
        }

        private void OnPlayChosenButtonPointerEnter(PointerEventData pointerEventData)
        {
            _volumeService.PlayClip(_hoverSFX);

            PlayRectTransformEnterAnimation(_playChosenButton.transform as RectTransform);
        }

        private void OnCustomPresetButtonPointerEnter(PointerEventData pointerEventData)
        {
            _volumeService.PlayClip(_hoverSFX);

            PlayRectTransformEnterAnimation(_customPresetButton.transform as RectTransform);
        }

#endregion

#region Custom Panel

        private void StartCustomPanel()
        {
            _backButton.onClick.AddListener(OnBackButtonClicked);
            _playCustomButton.onClick.AddListener(OnPlayButtonClicked);
            
            _increaseMineCountButton.onClick.AddListener(OnIncreaseMineCountButtonClicked);
            _decreaseMineCountButton.onClick.AddListener(OnDecreaseMineCountButtonClicked);
            
            _nextButton.onClick.AddListener(OnNextButtonClicked);
            _previousButton.onClick.AddListener(OnPreviousButtonClicked);
            
            _widthSnapSlider.OnValueChanged  += OnWidthSnapSliderValueChanged;
            _heightSnapSlider.OnValueChanged += OnHeightSnapSliderValueChanged;
        }
        
        private void OnHeightSnapSliderValueChanged(int currentValue)
        {
            PlaySnapSliderLinerPunchScaleAnimation(_heightSnapSlider);
            
            _onHeightSnapSliderValueChanged?.Invoke(currentValue);
        }

        private void OnWidthSnapSliderValueChanged(int currentValue)
        {
            PlaySnapSliderLinerPunchScaleAnimation(_widthSnapSlider);
            
            _onWidthSnapSliderValueChanged?.Invoke(currentValue);
        }
        
        private void OnPreviousButtonClicked()
        {
            _volumeService.PlayClip(_clickSFX);

            PlayRectTransformClickAnimation(_previousButton.transform as RectTransform, () =>
            {
                _onPreviousButtonClicked?.Invoke();
            });
        }

        private void OnNextButtonClicked()
        {
            _volumeService.PlayClip(_clickSFX);

            PlayRectTransformClickAnimation(_nextButton.transform as RectTransform, () =>
            {
                _onNextButtonClicked?.Invoke();
            });
        }

        private void OnIncreaseMineCountButtonClicked()
        {
            _volumeService.PlayClip(_clickSFX);

            PlayRectTransformClickAnimation(_increaseMineCountButton.transform as RectTransform, () =>
            {
                _onIncreaseMineCountButtonClicked?.Invoke();
            });
        }

        private void OnDecreaseMineCountButtonClicked()
        {
            _volumeService.PlayClip(_clickSFX);

            PlayRectTransformClickAnimation(_decreaseMineCountButton.transform as RectTransform, () =>
            {
                _onDecreaseMineCountButtonClicked?.Invoke();
            });
        }

        private void OnBackButtonClicked()
        {
            _volumeService.PlayClip(_clickSFX);

            PlayRectTransformClickAnimation(_backButton.transform as RectTransform, () =>
            {
                _onBackButtonClicked?.Invoke();
            
                ShowChoosePanel();
            });
        }
        
        private void OnBackButtonPointerEnter(PointerEventData pointerEventData)
        {
            _volumeService.PlayClip(_hoverSFX);

            PlayRectTransformEnterAnimation(_backButton.transform as RectTransform);
        }

        private void OnPlayCustomButtonPointerEnter(PointerEventData pointerEventData)
        {
            _volumeService.PlayClip(_hoverSFX);

            PlayRectTransformEnterAnimation(_playCustomButton.transform as RectTransform);
        }

        private void OnIncreaseMineCountButtonPointerEnter(PointerEventData pointerEventData)
        {
            _volumeService.PlayClip(_hoverSFX);

            PlayRectTransformEnterAnimation(_increaseMineCountButton.transform as RectTransform);
        }

        private void OnDecreaseMineCountButtonPointerEnter(PointerEventData pointerEventData)
        {
            _volumeService.PlayClip(_hoverSFX);

            PlayRectTransformEnterAnimation(_decreaseMineCountButton.transform as RectTransform);
        }

        private void OnNextButtonPointerEnter(PointerEventData pointerEventData)
        {
            _volumeService.PlayClip(_hoverSFX);

            PlayRectTransformEnterAnimation(_nextButton.transform as RectTransform);
        }

        private void OnPreviousButtonPointerEnter(PointerEventData pointerEventData)
        {
            _volumeService.PlayClip(_hoverSFX);

            PlayRectTransformEnterAnimation(_previousButton.transform as RectTransform);
        }

#endregion

        #region Animations

        public Tween PlayRectTransformClickAnimation(RectTransform rectTransform, Action onCompleted = null, bool autoPlay = true)
        {
            if (DOTween.IsTweening($"{rectTransform.GetEntityId()}_rectTransform_scale"))
                DOTween.Complete($"{rectTransform.GetEntityId()}_rectTransform_scale");

            var baseScale = rectTransform.localScale;

            Tween tween = default;
            tween = rectTransform
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
                    rectTransform.localScale = baseScale;
                })
                .SetLink(rectTransform.gameObject)
                .SetId($"{rectTransform.GetEntityId()}_rectTransform_scale")
                .Pause();

            if (autoPlay)
                tween.Play();
                
            return tween;
        }

        public Tween PlayRectTransformEnterAnimation(RectTransform rectTransform, bool autoPlay = true)
        {
            if (DOTween.IsTweening($"{rectTransform.GetEntityId()}_rectTransform_rotate"))
                DOTween.Complete($"{rectTransform.GetEntityId()}_rectTransform_rotate");

            var baseRotation = rectTransform.localRotation;
            
            Tween tween = rectTransform
                .DOPunchRotation(Vector3.forward * _buttonRotationStrength, _buttonRotationDuration, _buttonRotationVibratio, _buttonRotationElasticity)
                .OnComplete(() => rectTransform.localRotation = baseRotation)
                .SetLink(rectTransform.gameObject)
                .SetId($"{rectTransform.GetEntityId()}_rectTransform_rotate")
                .Pause();
            
            if (autoPlay)
                tween.Play();
            
            return tween;
        }

        public Tween PlayPanelAnchorPositionAnimation(RectTransform rectTransform, Vector2 endValue, bool autoPlay = true)
        {
            if (DOTween.IsTweening($"{rectTransform.GetEntityId()}_rectTransform_anchor_position"))
                DOTween.Complete($"{rectTransform.GetEntityId()}_rectTransform_anchor_position");
            
            var tween = rectTransform
                .DOAnchorPos(endValue, _anchorPositionDuration)
                .SetEase(_anchorPositionEase)
                .OnComplete(() => rectTransform.anchoredPosition = endValue)
                .SetLink(rectTransform.gameObject)
                .SetId($"{rectTransform.GetEntityId()}_rectTransform_anchor_position")
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
                .SetTarget(gameObject)
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
                .SetTarget(gameObject)
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
                .SetId($"{_rotatePanel.GetEntityId()}_punch_rotate")
                .Pause();
            
            if (autoPlay)
                tween.Play();

            return tween;
        }

        private Sequence PlaySnapSliderLinerPunchScaleAnimation(SnapSlider snapSlider, bool autoPlay = true)
        {
            if (DOTween.IsTweening($"{snapSlider.GetEntityId()}_liner_punch_scale"))
                DOTween.Complete($"{snapSlider.GetEntityId()}_liner_punch_scale");

            var sequence = DOTween.Sequence();
            sequence
                .SetId($"{snapSlider.GetEntityId()}_liner_punch_scale")
                .SetTarget(snapSlider)
                .Pause();

            for (var i = 0; i < snapSlider.Segments.Count; i++)
            {
                if (i > snapSlider.Value)
                    break;
                
                if (DOTween.IsTweening($"{snapSlider.Segments[i].GetEntityId()}_liner_punch_scale"))
                    DOTween.Complete($"{snapSlider.Segments[i].GetEntityId()}_liner_punch_scale");
                
                var segment   = snapSlider.Segments[i];

                var tween = segment
                    .DOScale(Vector3.one * _snapSliderScaleFactor, _snapSliderScaleDuration)
                    .SetEase(_snapSliderScaleEase)
                    .OnStart(() => segment.localScale = Vector3.one)
                    .OnComplete(() => segment.localScale = Vector3.one)
                    .SetLoops(2, LoopType.Yoyo)
                    .SetLink(segment.gameObject)
                    .SetId($"{segment.GetEntityId()}_liner_punch_scale")
                    .Pause();

                sequence.Insert(i * _snapSliderScaleDelay, tween);
            }

            if (autoPlay)
                sequence.Play();

            return sequence;
        }

        private Sequence PlayTextWarningAnimation(TextMeshProUGUI textMeshProUGUI, bool autoPlay = true)
        {
            if (DOTween.IsTweening($"{textMeshProUGUI.GetEntityId()}_text_warning")) 
                DOTween.Complete($"{textMeshProUGUI.GetEntityId()}_text_warning");
            
            var textInitialColor = textMeshProUGUI.color;
            
            var sequence = DOTween.Sequence();
            sequence
                .SetTarget(textMeshProUGUI)
                .SetLink(textMeshProUGUI.gameObject)
                .SetId($"{textMeshProUGUI.GetEntityId()}_text_warning");
            
            sequence.Insert(
                0.0f, 
                textMeshProUGUI
                    .DOColor(_textWarningColor, _textWarningDuration)
                    .SetEase(_textWarningEase)
                    .OnComplete(() => textMeshProUGUI.color = textInitialColor)
                    .SetLoops(2, LoopType.Yoyo)
                    .SetLink(textMeshProUGUI.gameObject));
            sequence.Insert(
                0.0f,
                PlayRectTransformEnterAnimation(textMeshProUGUI.rectTransform, false));
            
            if (autoPlay)
                sequence.Play();
            
            return sequence;
        }

#endregion
        
        private void OnPlayButtonClicked()
        {
            _volumeService.PlayClip(_clickSFX);
            
            _onPlayButtonClicked?.Invoke();
        }
        
        private void ShowChoosePanel(bool animate = true)
        {
            _volumeService.PlayClip(_panelShowSFX);
            
            var tween1 = PlayPanelAnchorPositionAnimation(_choosePanel, Vector2.zero);
            var tween2 = PlayPanelAnchorPositionAnimation(_customPanel, new Vector2(_baseXChosePanelPosition, 0f));

            if (!animate)
            {
                tween1.Complete();
                tween2.Complete();
            }
        }

        private void ShowCustomPanel(bool animate = true)
        {
            _volumeService.PlayClip(_panelHideSFX);
            
            var tween1 = PlayPanelAnchorPositionAnimation(_choosePanel, new Vector2(_baseXCustomPanelPosition, 0f));
            var tween2 = PlayPanelAnchorPositionAnimation(_customPanel, Vector2.zero);

            if (!animate)
            {
                tween1.Complete();
                tween2.Complete();
            }
        }
        
#endregion

#region Public API

        public void OnPlayButtonClicked(Action callback)
        {
            _onPlayButtonClicked = callback;
        }

        public void OnCustomPresetButtonClicked(Action callback)
        {
            _onCustomPresetButtonClicked = callback;
        }

        public void OnChoosePresetToggleActiveChanged(Action<string> callback)
        {
            _onChoosePresetToggleActiveChanged = callback;
        }

        public void SetActiveChoosePresetToggle(string identity)
        {
            var chooseTogglesValue = _chooseToggles.Values[identity];
            var activeToggleValue = _chooseToggleGroup.GetFirstActiveToggle();

            if (activeToggleValue == null)
                chooseTogglesValue.isOn = true;
            else if (chooseTogglesValue != activeToggleValue)
                chooseTogglesValue.isOn = true;
        }

        public void OnBackButtonClicked(Action callback)
        {
            _onBackButtonClicked = callback;
        }

        public void OnIncreaseMineCountButtonClicked(Action callback)
        {
            _onIncreaseMineCountButtonClicked = callback;
        }

        public void OnDecreaseMineCountButtonClicked(Action callback)
        {
            _onDecreaseMineCountButtonClicked = callback;
        }

        public void OnPreviousButtonClicked(Action callback)
        {
            _onPreviousButtonClicked = callback;
        }

        public void OnNextButtonClicked(Action callback)
        {
            _onNextButtonClicked = callback;
        }

        public void OnWidthSnapSliderValueChanged(Action<int> callback)
        {
            _onWidthSnapSliderValueChanged = callback;
        }

        public void OnHeightSnapSliderValueChanged(Action<int> callback)
        {
            _onHeightSnapSliderValueChanged = callback;
        }

        public void SetWidthSnapSliderValue(int value)
        {
            _widthSnapSlider.Value = value;
        }

        public void SetHeightSnapSliderValue(int value)
        {
            _heightSnapSlider.Value = value;
        }

        public void SetMinesCount(int value)
        {
            _mineCountTMP.SetText(value.ToString());
        }

        public void SetTimerSeconds(TimeSpan value)
        {
            _timerValueTMP.SetText($"{value:mm}m {value:ss}s");
        }
        
        public void PlayNotifyMineCountAnimation()
        {
            PlayTextWarningAnimation(_mineCountTMP);
        }

#endregion

#region Parent overrides

        public override void Show(bool animate = true)
        {
            gameObject.SetActive(true);
            _onBackButtonClicked?.Invoke();
            ShowChoosePanel(false);
            
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
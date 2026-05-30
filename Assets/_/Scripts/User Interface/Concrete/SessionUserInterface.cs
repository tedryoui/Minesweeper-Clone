using System;
using System.Collections.Generic;
using _.Scripts.Extensions;
using _.Scripts.Services;
using _.Scripts.User_Interface.Elements;
using _.Scripts.Utility;
using DG.Tweening;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace _.Scripts.User_Interface
{
    public class SessionUserInterface : AbstractUserInterface
    {
#region VContainer

        [Inject] private UserInterfaceService _userInterfaceService;
        [Inject] private VolumeService        _volumeService;
        
        [Inject]
        private void Configure()
        {
            _closeButton.onClick.AddListener(OnCloseButtonClicked);
            _restartButton.onClick.AddListener(OnRestartButtonClicked);
            
            _smartGrid.OnRebuild += RefreshMinesweeperCellsCache;
        }
        
#endregion

#region Scene References

        [SerializeField] private TextMeshProUGUI _stepCountTMP;
        [SerializeField] private TextMeshProUGUI _mineCountTMP;
        [Space(5)]
        [SerializeField] private Image           _timerFillImage;
        [SerializeField] private RectTransform   _timerFill;
        [SerializeField] private TextMeshProUGUI _timerTMP;
        [SerializeField] private AnimationCurve  _timerFillCurve;
        [Space(5)]
        [SerializeField] private Button          _closeButton;
        [SerializeField] private Button          _restartButton;
        [Space(5)]
        [SerializeField] private SmartGrid       _smartGrid;

#endregion

#region Fields

        [SerializeField] private AudioClip _panelShowSFX;
        [SerializeField] private AudioClip _panelHideSFX;
        [SerializeField] private AudioClip _clickSFX;
        [SerializeField] private AudioClip _hoverSFX;
        [SerializeField] private AudioClip _timerChangedSFX;
        
        private Dictionary<int2, MinesweeperCell> _cache;
        private float                             _timerFillMaxWidth;

        [GradientUsage(false, ColorSpace.Linear)]
        [SerializeField] private Gradient       _timerColorChangeGradient;
        [SerializeField] private AnimationCurve _timerColorChangeCurve;

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
        [SerializeField] private float _buttonScaleFactor   = 0.95f;
        [SerializeField] private float _buttonScaleDuration = 0.125f;
        [SerializeField] private Ease  _buttonScaleEase     = Ease.InOutQuad;
        [Space(5)]
        [SerializeField] private float _buttonRotationStrength   = 5f;
        [SerializeField] private float _buttonRotationDuration   = 0.5f;
        [SerializeField] private int   _buttonRotationVibratio   = 10;
        [SerializeField] private float _buttonRotationElasticity = 1f;
        [Space(5)]
        [SerializeField] private Color _textWarningColor = Color.orange;
        [SerializeField] private float _textWarningDuration = 0.25f;
        [SerializeField] private Ease  _textWarningEase     = Ease.InOutQuad;
        [Space(10)]
        [SerializeField] private float _panelPunchRotationOffset = 0.3f;
        [SerializeField] private float _hideFadeOutDelay = 0.25f;

#endregion
        
#region Events

        private event Action                                     _onCloseButtonClicked;
        private event Action                                     _onRestartButtonClicked;
        private event Action                                     _onConfirmationUserInterfaceShowed;
        private event Action                                     _onConfirmationUserInterfaceHidden;
        public event  Action<int2, PointerEventData.InputButton> _onMinesweeperCellClicked;

#endregion

        private void Awake()
        {
            _timerFillMaxWidth   =  _timerFill.sizeDelta.x;
            
            _onCloseButtonClicked              = delegate { };
            _onRestartButtonClicked            = delegate { };
            _onConfirmationUserInterfaceShowed = delegate { };
            _onConfirmationUserInterfaceHidden = delegate { };
        }

        private void Start()
        {
            (_closeButton.transform as RectTransform).OnPointerEnter(OnCloseButtonPointerEnter);
            (_restartButton.transform as RectTransform).OnPointerEnter(OnRestartButtonPointerEnter);
        }

#region Private Methods

        private void OnRestartButtonClicked()
        {
            _volumeService.PlayClip(_clickSFX);
            PlayButtonClickAnimation(_restartButton);
            
            _userInterfaceService.Show(USER_INTERFACE_IDENTITIES.CONFIRMATION_UI);
            _onConfirmationUserInterfaceShowed?.Invoke();

            var confirmationUI =
                _userInterfaceService.GetUserInterface<ConfirmationUserInterface>(USER_INTERFACE_IDENTITIES
                    .CONFIRMATION_UI);
            confirmationUI.SetTitle("Are you sure you want to restart?");
            confirmationUI.OnAcceptButtonClicked(() =>
            {
                _onRestartButtonClicked?.Invoke();
                _userInterfaceService.Hide(USER_INTERFACE_IDENTITIES.CONFIRMATION_UI);
            });
            confirmationUI.OnDismissButtonClicked(() =>
            {
                _userInterfaceService.Hide(USER_INTERFACE_IDENTITIES.CONFIRMATION_UI);
                _onConfirmationUserInterfaceHidden?.Invoke();
            });
        }

        private void OnCloseButtonClicked()
        {
            _volumeService.PlayClip(_clickSFX);
            PlayButtonClickAnimation(_closeButton);
            
            _userInterfaceService.Show(USER_INTERFACE_IDENTITIES.CONFIRMATION_UI);
            _onConfirmationUserInterfaceShowed?.Invoke();

            var confirmationUI =
                _userInterfaceService.GetUserInterface<ConfirmationUserInterface>(USER_INTERFACE_IDENTITIES
                    .CONFIRMATION_UI);
            confirmationUI.SetTitle("Are you sure you want to exit?");
            confirmationUI.OnAcceptButtonClicked(() =>
            {
                _onCloseButtonClicked?.Invoke();
                _userInterfaceService.Hide(USER_INTERFACE_IDENTITIES.CONFIRMATION_UI);
            });
            confirmationUI.OnDismissButtonClicked(() =>
            {
                _userInterfaceService.Hide(USER_INTERFACE_IDENTITIES.CONFIRMATION_UI);
                _onConfirmationUserInterfaceHidden?.Invoke();
            });
        }

        private void RefreshMinesweeperCellsCache()
        {
            _cache = new Dictionary<int2, MinesweeperCell>();
            
            foreach (var cell in _smartGrid.Cells)
            {
                var identity        = cell.Key;
                var minesweeperCell = cell.Value.gameObject.GetComponent<MinesweeperCell>();
                
                _cache.Add(identity, minesweeperCell);
                minesweeperCell.OnPointerClick((inputButton) =>
                {
                    _volumeService.PlayClip(_clickSFX);

                    _onMinesweeperCellClicked?.Invoke(identity, inputButton);
                });
                minesweeperCell.OnPointerEnter(() =>
                {
                    _volumeService.PlayClip(_hoverSFX);
                });
            }
        }
        
        public MinesweeperCell GetMinesweeperCell(int2 identity)
        {
            return _cache[identity];
        }

        private void OnCloseButtonPointerEnter(PointerEventData eventData)
        {
            _volumeService.PlayClip(_hoverSFX);

            PlayButtonEnterAnimation(_closeButton);
        }

        private void OnRestartButtonPointerEnter(PointerEventData eventData)
        {
            _volumeService.PlayClip(_hoverSFX);
            
            PlayButtonEnterAnimation(_restartButton);
        }
        
#region Animations

        public Tween PlayButtonClickAnimation(Button button, Action onCompleted = null, bool autoPlay = true)
        {
            if (DOTween.IsTweening($"{button.GetEntityId()}_rectTransform_scale"))
                DOTween.Complete($"{button.GetEntityId()}_rectTransform_scale");

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
                .SetId($"{button.GetEntityId()}_rectTransform_scale")
                .Pause();

            if (autoPlay)
                tween.Play();
                
            return tween;
        }

        public Tween PlayButtonEnterAnimation(Button button, bool autoPlay = true)
        {
            if (DOTween.IsTweening($"{button.GetEntityId()}_rectTransform_rotate"))
                DOTween.Complete($"{button.GetEntityId()}_rectTransform_rotate");

            var baseRotation = button.transform.localRotation;
            
            Tween tween = button.transform
                .DOPunchRotation(Vector3.forward * _buttonRotationStrength, _buttonRotationDuration, _buttonRotationVibratio, _buttonRotationElasticity)
                .OnComplete(() => button.transform.localRotation = baseRotation)
                .SetLink(button.gameObject)
                .SetId($"{button.GetEntityId()}_rectTransform_rotate")
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

        private Tween PlayTextNotionAnimation(TextMeshProUGUI textMeshProUGUI, bool autoPlay = true)
        {
            if (DOTween.IsTweening($"{textMeshProUGUI.GetEntityId()}_text_notion")) 
                DOTween.Complete($"{textMeshProUGUI.GetEntityId()}_text_notion");
            
            var textInitialColor = textMeshProUGUI.color;
            
            var tween = textMeshProUGUI
                    .DOColor(_textWarningColor, _textWarningDuration)
                    .SetEase(_textWarningEase)
                    .OnComplete(() => textMeshProUGUI.color = textInitialColor)
                    .SetLoops(2, LoopType.Yoyo)
                    .SetLink(textMeshProUGUI.gameObject)
                    .SetId($"{textMeshProUGUI.GetEntityId()}_text_notion")
                    .Pause();
            
            if (autoPlay)
                tween.Play();
            
            return tween;
        }

#endregion

#endregion

#region Public API

        public void OnRestartButtonClicked(Action callback)
        {
            _onRestartButtonClicked = callback;
        }
        
        public void OnCloseButtonClicked(Action callback)
        {
            _onCloseButtonClicked = callback;
        }
        
        public void OnConfirmationUserInterfaceShowed(Action callback)
        {
            _onConfirmationUserInterfaceShowed = callback;
        }

        public void OnConfirmationUserInterfaceHidden(Action callback)
        {
            _onConfirmationUserInterfaceHidden = callback;
        }

        public void OnMinesweeperCellClicked(Action<int2, PointerEventData.InputButton> callback)
        {
            _onMinesweeperCellClicked = callback;
        }
        
        public void SetMineCount(int mineCount)
        {
            _mineCountTMP.text = $"{mineCount:00}";
        }

        public void SetTimerFill(float fillAmount)
        {
            var curvedFillAmount = _timerFillCurve.Evaluate(fillAmount);
            var sizeDelta        = _timerFill.sizeDelta;
            sizeDelta.x = curvedFillAmount *  _timerFillMaxWidth;
            
            var colorPosition = _timerColorChangeCurve.Evaluate(fillAmount);
            var color = _timerColorChangeGradient.Evaluate(colorPosition);
            
            _timerFill.sizeDelta = sizeDelta;
            _timerFillImage.color = color;
        }

        public void SetTimerFillText(TimeSpan timeSpan)
        {
            _timerTMP.SetText($"{timeSpan.Minutes:00}:{timeSpan.Seconds:00}");
        }

        public void PlayTimerTickClip()
        {
            _volumeService.PlayClip(_timerChangedSFX);
        }
        
        public void SetTimerFillText(string value)
        {
            _timerTMP.SetText(value);
        }
        
        public void SetStepsCount(int value)
        {
            _stepCountTMP.SetText($"{value:00}");

            PlayTextNotionAnimation(_stepCountTMP);
        }

        public void SetSmartGridSize(int width, int height)
        {
            _smartGrid.SetSize(width, height);
            _smartGrid.Rebuild();
        }

#endregion

#region Parent overrides

        public override void Show(bool animate = true)
        {
            gameObject.SetActive(true);

            var tween = PlayFadeInAnimation(() =>
            {
                if (animate)
                    _volumeService.PlayClip(_panelShowSFX);
            });
            
            if (!animate)
                tween.Complete();
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
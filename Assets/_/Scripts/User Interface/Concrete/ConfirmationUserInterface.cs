using System;
using _.Scripts.Extensions;
using _.Scripts.Services;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace _.Scripts.User_Interface
{
    public class ConfirmationUserInterface : AbstractUserInterface
    {
#region VContainer

        [Inject] private UserInterfaceService _userInterfaceService;
        [Inject] private VolumeService        _volumeService;
        
        [Inject]
        private void Configure()
        {
            _acceptButton.onClick.AddListener(OnAcceptButtonClicked);
            _dismissButton.onClick.AddListener(OnDismissButtonClicked);
        }

#endregion

#region Scene References

        [Header("General")]
        [SerializeField] private TextMeshProUGUI _titleTMP;
        [SerializeField] private Button          _acceptButton;
        [SerializeField] private Button          _dismissButton;

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
        [SerializeField] private float _fadeInDuration  = 0.25f;
        [SerializeField] private Ease  _fadeInEase      = Ease.OutQuad;
        [SerializeField] private float _fadeOutDuration = 0.25f;
        [SerializeField] private Ease  _fadeOutEase     = Ease.OutQuad;
        [Space(5)]
        [SerializeField] private RectTransform _panelRectTransform;
        [SerializeField] private float _punchStrength   = 0.25f;
        [SerializeField] private float _punchDuration   = 0.35f;
        [SerializeField] private int   _punchVibratio   = 2;
        [SerializeField] private float _punchElasticity = 0.25f;
        [Space(5)]
        [SerializeField] private float _buttonScaleFactor;
        [SerializeField] private float _buttonScaleDuration = 0.25f;
        [SerializeField] private Ease  _buttonScaleEase     = Ease.OutQuad;
        [Space(5)]
        [SerializeField] private float _buttonRotationStrength   = 5f;
        [SerializeField] private float _buttonRotationDuration   = 0.5f;
        [SerializeField] private int   _buttonRotationVibratio   = 10;
        [SerializeField] private float _buttonRotationElasticity = 1f;
        [Space(10)]
        [SerializeField] private float _showPunchTweenOffset = 0.1f;
        [SerializeField] private float _hideFadeOutDelay = 0.3f;

#endregion

#region Events

        private event Action _onAcceptButtonClicked;
        private event Action _onDismissButtonClicked;

#endregion
        
        private void Awake()
        {
            _onAcceptButtonClicked  = delegate { };
            _onDismissButtonClicked = delegate { };
        }

        private void Start()
        {
            (_acceptButton.transform as RectTransform).OnPointerEnter(OnAcceptButtonPointerEnter);
            (_dismissButton.transform as RectTransform).OnPointerEnter(OnDismissButtonPointerEnter);
        }

#region Private Methods

        private void OnAcceptButtonClicked()
        {
            _onAcceptButtonClicked?.Invoke();

            _volumeService.PlayClip(_clickSFX);
            PlayButtonClickAnimation(_acceptButton);
        }
        
        private void OnDismissButtonClicked()
        {
            _onDismissButtonClicked?.Invoke();
            
            _volumeService.PlayClip(_clickSFX);
            PlayButtonClickAnimation(_dismissButton);
        }

        private void OnAcceptButtonPointerEnter(PointerEventData eventData)
        {
            _volumeService.PlayClip(_hoverSFX);
            PlayButtonClickAnimation(_acceptButton);
        }

        private void OnDismissButtonPointerEnter(PointerEventData eventData)
        {
            _volumeService.PlayClip(_hoverSFX);
            PlayButtonEnterAnimation(_dismissButton);
        }

#region Animations

        private Tween PlayFadeInAnimation(Action onComplete = null, bool autoPlay = true)
        {
            if (DOTween.IsTweening($"{_canvasGroup.GetEntityId()}_fadeIn"))
                return null;
            
            _canvasGroup.interactable   = false;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.alpha          = 0f;

            var tween = _canvasGroup.DOFade(1.0f, _fadeInDuration).SetEase(_fadeInEase).OnComplete(() =>
            {
                _canvasGroup.interactable   = true;
                _canvasGroup.blocksRaycasts = true;
                _canvasGroup.alpha          = 1f;
                
                onComplete?.Invoke();
            }).Pause().SetLink(gameObject).SetId($"{_canvasGroup.GetEntityId()}_fadeIn");

            if (autoPlay)
                tween.Play();
            
            return tween;
        }
        
        private Tween PlayFadeOutAnimation(Action onComplete = null, bool autoPlay = true)
        {
            if (DOTween.IsTweening($"{_canvasGroup.GetEntityId()}_fadeOut"))
                return null;
            
            _canvasGroup.interactable   = false;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.alpha          = 1f;

            var tween = _canvasGroup.DOFade(0.0f, _fadeInDuration).SetEase(_fadeInEase).OnComplete(() =>
            {
                _canvasGroup.interactable   = false;
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.alpha          = 0f;
                
                onComplete?.Invoke();
            }).Pause().SetLink(gameObject).SetId($"{_canvasGroup.GetEntityId()}_fadeOut");
            
            if (autoPlay)
                tween.Play();
            
            return tween;
        }

        private Tween PlayPunchAnimation(bool autoPlay = true)
        {
            if (DOTween.IsTweening($"{_canvasGroup.GetEntityId()}_punch"))
                return null;
            
            var baseValue = _panelRectTransform.localScale;

            var tween = _panelRectTransform
                .DOPunchScale(Vector3.one * _punchStrength, _punchDuration, _punchVibratio, _punchElasticity)
                .OnComplete(() =>
                {
                    _panelRectTransform.localScale = baseValue;
                }).Pause().SetLink(gameObject).SetId($"{_panelRectTransform.GetEntityId()}_punch");
            
            if (autoPlay)
                tween.Play();

            return tween;
        }

        private Tween PlayButtonClickAnimation(Button button, Action onComplete = null)
        {
            if (DOTween.IsTweening($"{_canvasGroup.GetEntityId()}_button"))
                return null;
            
            var baseScale = button.transform.localScale;

            var tween = button.transform
                .DOScale(baseScale * _buttonScaleFactor, _buttonScaleDuration)
                .SetEase(_buttonScaleEase)
                .OnComplete(() =>
                {
                    button.transform.localScale = baseScale;
                    onComplete?.Invoke();
                })
                .SetLoops(2, LoopType.Yoyo)
                .SetLink(button.gameObject)
                .SetId($"{button.GetEntityId()}_button_scale");
            
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

#region Public API

        public void OnAcceptButtonClicked(Action callback)
        {
            _onAcceptButtonClicked = callback;
        }

        public void OnDismissButtonClicked(Action callback)
        {
            _onDismissButtonClicked = callback;
        }

        public void SetTitle(string title)
        {
            _titleTMP.text = title;
        }

#endregion

#region Parent overrides

        public override void Show(bool animate = true)
        {
            gameObject.SetActive(true);

            var fadeInAnimation = PlayFadeInAnimation(() =>
            {
                if (animate)
                    _volumeService.PlayClip(_panelShowSFX);
            }, autoPlay: false);
            var punchAnimation  = PlayPunchAnimation(autoPlay: false);

            if (fadeInAnimation != null && punchAnimation != null)
            {
                var sequence = DOTween.Sequence().SetTarget(gameObject).SetLink(gameObject);

                sequence.Insert(0.0f,                  fadeInAnimation);
                sequence.Insert(_showPunchTweenOffset, punchAnimation);

                sequence.Play();
                
                if (!animate)
                    sequence.Complete();
            }
        }

        public override void Hide(bool animate = true)
        {
            _onAcceptButtonClicked  = delegate { };
            _onDismissButtonClicked = delegate { };

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
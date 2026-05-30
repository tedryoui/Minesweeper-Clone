using System;
using _.Scripts.Extensions;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _.Scripts.User_Interface.Elements
{
    public class MinesweeperCell : MonoBehaviour
    {
#region Scene References

        [SerializeField] private GameObject _opened;
        [SerializeField] private GameObject _closed;
        
        [SerializeField] private GameObject      _flag;
        [SerializeField] private GameObject      _bomb;
        [SerializeField] private TextMeshProUGUI _number;

        [SerializeField] private RectTransform _raycast;

#endregion
        
#region Animation setup

        [Space(10)]
        [Header("Animation")]
        [SerializeField] private float _blowScaleFactor = 1.2f;
        [SerializeField] private float _blowScaleDuration = 0.25f;
        [SerializeField] private Ease  _blowScaleEase = Ease.OutQuart;
        [Space(5)]
        [SerializeField] private float _clickScaleFactor   = 0.95f;
        [SerializeField] private float _clickScaleDuration = 0.125f;
        [SerializeField] private Ease  _clickScaleEase     = Ease.InOutQuad;
        [Space(5)]
        [SerializeField] private float _enterRotationStrength   = 2f;
        [SerializeField] private float _enterRotationDuration   = 0.5f;
        [SerializeField] private int   _enterRotationVibratio   = 6;
        [SerializeField] private float _enterRotationElasticity = 1f;

#endregion
        
#region Events

        private event Action<PointerEventData.InputButton> _onClick;
        private event Action                               _onEnter;

#endregion

        private void Start()
        {
            ResetToDefaultState();
            
            _raycast.OnPointerClick(OnPointerClick);
            _raycast.OnPointerEnter(OnPointerEnter);
        }

#region Private Methods

        private void ResetToDefaultState()
        {
            EnableFlag(false);
            EnableBomb(false);
            EnableNumber(false, 0);
            
            Close();
        }

        private void OnPointerEnter(PointerEventData eventData)
        {
            PlayRectTransformEnterAnimation();
            
            _onEnter?.Invoke();
        }

        private void OnPointerClick(PointerEventData eventData)
        {
            PlayRectTransformClickAnimation();
            
            var mouseButton = eventData.button;
            
            _onClick?.Invoke(mouseButton);
        }

#region Animations

        private Tween PlayBlowAnimation(Action onStepComplete = null, bool autoPlay = true)
        {
            if (DOTween.IsTweening(transform))
                DOTween.Complete(transform, true);

            var baseScale = transform.localScale;

            Tween tween = default;
            
            tween = transform
                .DOScale(baseScale * _blowScaleFactor, _blowScaleDuration)
                .SetEase(_blowScaleEase)
                .OnComplete(() => transform.localScale = baseScale)
                .SetLoops(2,  LoopType.Yoyo)
                .OnStepComplete(() =>
                {
                    if (tween == null) 
                        return;
                    
                    if (tween.CompletedLoops() == 1)
                        onStepComplete?.Invoke();
                })
                .SetLink(gameObject)
                .SetTarget(transform)
                .Pause();
            
            if (autoPlay)
                tween.Play();
            
            return tween;
        }
        
        public Tween PlayRectTransformClickAnimation()
        {
            if (DOTween.IsTweening($"{transform.GetEntityId()}_rectTransform_scale"))
                DOTween.Complete($"{transform.GetEntityId()}_rectTransform_scale");

            var baseScale = transform.localScale;

            Tween tween = default;
            tween = transform
                .DOScale(baseScale * _clickScaleFactor, _clickScaleDuration)
                .SetEase(_clickScaleEase)
                .SetLoops(2, LoopType.Yoyo)
                .OnComplete(() =>
                {
                    transform.localScale = baseScale;
                })
                .SetLink(gameObject)
                .SetTarget(transform)
                .SetId($"{transform.GetEntityId()}_rectTransform_scale");
                
            return tween;
        }

        public Tween PlayRectTransformEnterAnimation()
        {
            if (DOTween.IsTweening($"{transform.GetEntityId()}_rectTransform_rotate"))
                DOTween.Complete($"{transform.GetEntityId()}_rectTransform_rotate");

            var baseRotation = transform.localRotation;
            
            Tween tween = transform
                .DOPunchRotation(Vector3.forward * _enterRotationStrength, _enterRotationDuration, _enterRotationVibratio, _enterRotationElasticity)
                .OnComplete(() => transform.localRotation = baseRotation)
                .SetLink(gameObject)
                .SetTarget(transform)
                .SetId($"{transform.GetEntityId()}_rectTransform_rotate");
            
            return tween;
        }

#endregion

#endregion

#region Public API

        public void Open(bool animate = false)
        {
            if (animate)
            {
                PlayBlowAnimation(() =>
                {
                    _opened.SetActive(true);
                    _closed.SetActive(false);
                });
            }
            else
            {
                _opened.SetActive(true);
                _closed.SetActive(false);
            }
        }

        public void Close()
        {
            _opened.SetActive(false);
            _closed.SetActive(true);
        }

        public void EnableFlag(bool value, bool animate = false)
        {
            if (animate)
            {
                PlayBlowAnimation(() =>
                {
                    _flag.SetActive(value);
                });
            }
            else
            {
                _flag.SetActive(value);
            }
        }

        public void EnableBomb(bool value, bool animate = false)
        {
            if (animate)
            {
                PlayBlowAnimation(() =>
                {
                    _bomb.SetActive(value);
                });
            }
            else
            {
                _bomb.SetActive(value);
            }
        }

        public void EnableNumber(bool value, int number, bool animate = false)
        {
            if (animate)
            {
                PlayBlowAnimation(() =>
                {
                    _number.gameObject.SetActive(value);

                    _number.SetText(number.ToString());
                });
            }
            else
            {
                _number.gameObject.SetActive(value);
                            
                _number.SetText(number.ToString());
            }
        }

        public void OnPointerClick(Action<PointerEventData.InputButton> action)
        {
            _onClick = action;
        }
        
        public void OnPointerEnter(Action action)
        {
            _onEnter = action;
        }

#endregion
    }
}
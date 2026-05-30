using System;
using UnityEngine;

namespace @_.Scripts.Services
{
    public abstract class AbstractUserInterface : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas;

        public Canvas Canvas => _canvas;

#region Validation

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_canvas == null)
            {
                if (TryGetComponent(out _canvas) && _canvas != null) return;
                
                var childCanvas = gameObject.GetComponentInChildren<Canvas>();

                if (childCanvas == null)
                    throw new Exception($"Can not find canvas component {gameObject.name}");
                else
                    _canvas = childCanvas;
            }
        }
#endif

#endregion
        
#region Public API

        public abstract void Show(bool animate = true);
        public abstract void Hide(bool animate = true);

#endregion
    }
}
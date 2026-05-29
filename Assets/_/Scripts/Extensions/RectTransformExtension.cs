using System;
using _.Scripts.Utility;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _.Scripts.Extensions
{
    public static class RectTransformExtension
    {
        public static RectTransform OnPointerClick(this RectTransform rectTransform, Action<PointerEventData> action)
        {
            if (!rectTransform.TryGetComponent(out RectTransformOnPointerClick component))
                component = rectTransform.gameObject.AddComponent<RectTransformOnPointerClick>();
            
            component.SetOnPointerClick(action);

            return rectTransform;
        }
    }
}
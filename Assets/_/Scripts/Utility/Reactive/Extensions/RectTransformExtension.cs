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
            
            component.OnPointerClick(action);

            return rectTransform;
        }

        public static RectTransform OnPointerEnter(this RectTransform rectTransform, Action<PointerEventData> action)
        {
            if (!rectTransform.TryGetComponent(out RectTransformOnPointerEnter component))
                component = rectTransform.gameObject.AddComponent<RectTransformOnPointerEnter>();
            
            component.OnPointerEnter(action);

            return rectTransform;
        }
    }
}
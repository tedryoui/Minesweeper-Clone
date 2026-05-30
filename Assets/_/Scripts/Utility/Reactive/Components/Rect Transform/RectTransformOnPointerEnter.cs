using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _.Scripts.Utility
{
    public class RectTransformOnPointerEnter : MonoBehaviour, IPointerEnterHandler
    {
        private event Action<PointerEventData> _onPointerEnter;

        public void OnPointerEnter(Action<PointerEventData> action)
        {
            _onPointerEnter = action;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            _onPointerEnter?.Invoke(eventData);
        }
    }
}
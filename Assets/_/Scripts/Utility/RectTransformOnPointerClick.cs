using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _.Scripts.Utility
{
    public class RectTransformOnPointerClick : MonoBehaviour, IPointerClickHandler
    {
        private event Action<PointerEventData> _onPointerClick;

        public void SetOnPointerClick(Action<PointerEventData> action)
        {
            _onPointerClick = action;
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            _onPointerClick?.Invoke(eventData);
        }
    }
}
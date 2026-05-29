using System;
using _.Scripts.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _.Scripts.User_Interface.Elements
{
    public class MinesweeperCell : MonoBehaviour
    {
        [SerializeField] private GameObject _opened;
        [SerializeField] private GameObject _closed;
        
        [SerializeField] private GameObject      _flag;
        [SerializeField] private GameObject      _bomb;
        [SerializeField] private TextMeshProUGUI _number;

        [SerializeField] private RectTransform _raycast;

        private Action<PointerEventData.InputButton> _onClick;

        private void Awake()
        {
            SetOnClick(null);
            _raycast.OnPointerClick(OnPointerClickRaycast);
        }

        private void Start()
        {
            EnableFlag(false);
            EnableBomb(false);
            EnableNumber(false, 0);
            
            Close();
        }

        public void Open()
        {
            _opened.SetActive(true);
            _closed.SetActive(false);
        }

        public void Close()
        {
            _opened.SetActive(false);
            _closed.SetActive(true);
        }

        public void EnableFlag(bool value)
        {
            _flag.SetActive(value);
        }

        public void EnableBomb(bool value)
        {
            _bomb.SetActive(value);
        }

        public void EnableNumber(bool value, int number)
        {
            _number.gameObject.SetActive(value);
            
            _number.SetText(number.ToString());
        }

        public void SetOnClick(Action<PointerEventData.InputButton> action)
        {
            _onClick = action;
        }

        private void OnPointerClickRaycast(PointerEventData eventData)
        {
            var mouseButton = eventData.button;
            
            _onClick?.Invoke(mouseButton);
        }
    }
}
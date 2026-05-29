using System;
using System.Collections.Generic;
using _.Scripts.Services;
using _.Scripts.User_Interface.Elements;
using _.Scripts.Utility;
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
        private UserInterfaceService _userInterfaceService;
        
        [SerializeField] private TextMeshProUGUI _mineCountTMP;
        [SerializeField] private RectTransform   _timerFill;
        [SerializeField] private TextMeshProUGUI _timerTMP;
        [SerializeField] private AnimationCurve  _timerFillCurve;
        [SerializeField] private SmartGrid       _smartGrid;
        [SerializeField] private Button          _closeButton;
        [SerializeField] private Button          _restartButton;

        private Dictionary<int2, MinesweeperCell> _smartGridCells;
        
        private float _timerFillMaxWidth;

        private event Action _onCloseButtonClicked;
        private event Action _onRestartButtonClicked;
        private event Action _onConfirmationUserInterfaceShowed;
        private event Action _onConfirmationUserInterfaceHidden;

        private void Awake()
        {
            _onCloseButtonClicked              = delegate { };
            _onRestartButtonClicked            = delegate { };
            _onConfirmationUserInterfaceShowed = delegate { };
            _onConfirmationUserInterfaceHidden = delegate { };
        }

        [Inject]
        private void Configure(UserInterfaceService userInterfaceService)
        {
            _userInterfaceService = userInterfaceService;
            
            _timerFillMaxWidth   =  _timerFill.sizeDelta.x;
            _smartGrid.OnRebuild += OnSmartGridRebuild;
            OnSmartGridRebuild();
            
            _closeButton.onClick.AddListener(OnCloseButtonClicked);
            _restartButton.onClick.AddListener(OnRestartButtonClicked);
        }

        public void OnConfirmationUserInterfaceShowed(Action callback)
        {
            _onConfirmationUserInterfaceShowed = callback;
        }

        public void OnConfirmationUserInterfaceHidden(Action callback)
        {
            _onConfirmationUserInterfaceHidden = callback;
        }

        private void OnRestartButtonClicked()
        {
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

        public void OnRestartButtonClicked(Action callback)
        {
            _onRestartButtonClicked = callback;
        }

        private void OnCloseButtonClicked()
        {
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

        public void OnCloseButtonClicked(Action callback)
        {
            _onCloseButtonClicked = callback;
        }

        private void OnSmartGridRebuild()
        {
            _smartGridCells = new Dictionary<int2, MinesweeperCell>();
            
            foreach (var cell in _smartGrid.Cells)
            {
                var identity = cell.Key;
                var minesweeperCell = cell.Value.gameObject.GetComponent<MinesweeperCell>();
                
                _smartGridCells.Add(identity, minesweeperCell);
            }
        }

        public MinesweeperCell GetCell(int2 identity)
        {
            return _smartGridCells[identity];
        }

        public void SetMineCount(int mineCount)
        {
            _mineCountTMP.text = $"{mineCount:00}";
        }

        public void SetTimerFill(float fillAmount)
        {
            var curvedFillAmount = _timerFillCurve.Evaluate(fillAmount);
            var sizeDelta = _timerFill.sizeDelta;
            sizeDelta.x = curvedFillAmount *  _timerFillMaxWidth;
            
            _timerFill.sizeDelta = sizeDelta;
        }

        public void SetTimerFillText(TimeSpan timeSpan)
        {
            _timerTMP.SetText($"{timeSpan.Minutes:00}:{timeSpan.Seconds:00}");
        }

        public void SetSmartGridSize(int width, int height)
        {
            _smartGrid.SetSize(width, height);
            _smartGrid.Rebuild();
        }

        public void SetCellClickedAction(Action<int2, PointerEventData.InputButton, MinesweeperCell> action)
        {
            foreach (var smartGridCell in _smartGrid.Cells)
            {
                var minesweeperCell = smartGridCell.Value.gameObject.GetComponent<MinesweeperCell>();
                
                minesweeperCell.SetOnClick((button) => action(smartGridCell.Key, button, minesweeperCell));
            }
        }
        
        public override void Show()
        {
            gameObject.SetActive(true);
        }

        public override void Hide()
        {
            gameObject.SetActive(false);
            
            _onRestartButtonClicked = delegate { };
            _onCloseButtonClicked   = delegate { };
        }
    }
}
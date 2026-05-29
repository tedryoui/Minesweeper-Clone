using System;
using System.Linq;
using _.Scripts.Scriptable_Objects;
using _.Scripts.Services;
using _.Scripts.UI;
using _.Scripts.Utility;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _.Scripts.User_Interface
{
    public class GameplayPresetUserInterface : AbstractUserInterface
    {
        private UserInterfaceService _userInterfaceService;
        private ProjectSettings      _projectSettings;
        private SessionService       _sessionService;
        
        [Serializable]
        public struct TogglePair
        {
            public string Identity;
            public Toggle Toggle;
        }

        [SerializeField] private RectTransform _choosePanel;
        [SerializeField] private Button        _closeButton;
        [SerializeField] private Button        _playChosenButton;
        [SerializeField] private Button        _customPresetButton;
        [SerializeField] private TogglePair[]  _chooseToggles;
        [SerializeField] private ToggleGroup   _chooseToggleGroup;

        [SerializeField] private RectTransform   _customPanel;
        [SerializeField] private Button          _backButton;
        [SerializeField] private Button          _playCustomButton;
        [SerializeField] private SnapSlider      _widthSnapSlider;
        [SerializeField] private SnapSlider      _heightSnapSlider;
        [SerializeField] private TextMeshProUGUI _mineCountTMP;
        [SerializeField] private Button          _increaseMineCountButton;
        [SerializeField] private Button          _decreaseMineCountButton;
        [SerializeField] private TextMeshProUGUI _timerValueTMP;
        [SerializeField] private Button          _nextButton;
        [SerializeField] private Button          _previousButton;
        
        [Inject]
        private void Configure(
            UserInterfaceService userInterfaceService,
            SessionService sessionService,
            ProjectSettings projectSettings)
        {
            _projectSettings      = projectSettings;
            _sessionService       = sessionService;
            _userInterfaceService = userInterfaceService;
            
            _closeButton.onClick.AddListener(OnCloseButtonClicked);
            _playChosenButton.onClick.AddListener(OnPlayButtonClicked);
            _customPresetButton.onClick.AddListener(OnCustomPresetButtonClicked);
            _chooseToggles.ToList().ForEach(pair => pair.Toggle.onValueChanged.AddListener(OnChooseToggleValueChanged));
            
            _backButton.onClick.AddListener(OnBackButtonClicked);
            _playCustomButton.onClick.AddListener(OnPlayButtonClicked);
            _increaseMineCountButton.onClick.AddListener(OnIncreaseMineCountButtonClicked);
            _decreaseMineCountButton.onClick.AddListener(OnDecreaseMineCountButtonClicked);
            _widthSnapSlider.OnValueChanged += OnWidthSnapSliderValueChanged;
            _heightSnapSlider.OnValueChanged += OnHeightSnapSliderValueChanged;
            _nextButton.onClick.AddListener(OnNextButtonClicked);
            _previousButton.onClick.AddListener(OnPreviousButtonClicked);
        }

        private void OnPreviousButtonClicked()
        {
            var index = _projectSettings.SessionSettings.TimerVariants.ToList().FindIndex(x => x.Seconds.Equals(_sessionService.GameplayPreset.TimerSeconds));
            var previousIndex = (index - 1 < 0) ? _projectSettings.SessionSettings.TimerVariants.Length - 1 : index - 1;

            _sessionService.GameplayPreset.TimerSeconds = _projectSettings.SessionSettings.TimerVariants[previousIndex].Seconds;
            
            var timerValue = TimeSpan.FromSeconds(_sessionService.GameplayPreset.TimerSeconds);
            _timerValueTMP.SetText($"{timerValue.Minutes:00}m {timerValue.Seconds:00}s");
        }

        private void OnNextButtonClicked()
        {
            var index = _projectSettings.SessionSettings.TimerVariants.ToList().FindIndex(x => x.Seconds.Equals(_sessionService.GameplayPreset.TimerSeconds));
            var nextIndex = (index + 1 >= _projectSettings.SessionSettings.TimerVariants.Length) ? 0 : index + 1;

            _sessionService.GameplayPreset.TimerSeconds = _projectSettings.SessionSettings.TimerVariants[nextIndex].Seconds;
            
            var timerValue = TimeSpan.FromSeconds(_sessionService.GameplayPreset.TimerSeconds);
            _timerValueTMP.SetText($"{timerValue.Minutes:00}m {timerValue.Seconds:00}s");
        }

        private void OnHeightSnapSliderValueChanged(int currentValue)
        {
            var actualHeight = math
                .clamp(
                    currentValue,
                    _projectSettings.SessionSettings.MapHeight.x,
                    _projectSettings.SessionSettings.MapHeight.y
                );
            
            _sessionService.GameplayPreset.Height = actualHeight;
            
            ValidateMinesCount();
        }

        private void OnWidthSnapSliderValueChanged(int currentValue)
        {
            var actualWidth = math
                .clamp(
                    currentValue,
                    _projectSettings.SessionSettings.MapWidth.x,
                    _projectSettings.SessionSettings.MapWidth.y
                );
            
            _sessionService.GameplayPreset.Width = actualWidth;
            
            ValidateMinesCount();
        }

        private void OnDecreaseMineCountButtonClicked()
        {
            _sessionService.GameplayPreset.MineCount -= 1;
            
            ValidateMinesCount();
        }

        private void OnIncreaseMineCountButtonClicked()
        {
            _sessionService.GameplayPreset.MineCount += 1;
            
            ValidateMinesCount();
        }

        private void ValidateMinesCount()
        {
            var cellCount          = _sessionService.GameplayPreset.Width * _sessionService.GameplayPreset.Height;
            var expectedMinesCount = _sessionService.GameplayPreset.MineCount;
            var minimumMinesCount  = math.ceil(cellCount * _projectSettings.SessionSettings.MineCountPercentage.x);
            var maximumMinesCount  = math.ceil(cellCount * _projectSettings.SessionSettings.MineCountPercentage.y);
            var actualMinesCount = (int)math
                .clamp(
                    expectedMinesCount,
                    minimumMinesCount,
                    maximumMinesCount
                );
            
            _sessionService.GameplayPreset.MineCount = actualMinesCount;   
            _mineCountTMP.SetText(_sessionService.GameplayPreset.MineCount.ToString());
        }

        private void OnChooseToggleValueChanged(bool value)
        {
            var identity     = _chooseToggles.First(x => x.Toggle.isOn.Equals(true)).Identity;
            var gameplayPreset = _projectSettings.GameplayPresets.First(x => x.Identity.Equals(identity)).PresetReference;
            
            _sessionService.ChooseGameplayPreset(gameplayPreset);
        }

        private void OnBackButtonClicked()
        {
            var firstGameplayPreset = _projectSettings.GameplayPresets.First().PresetReference;
            _sessionService.ChooseGameplayPreset(firstGameplayPreset);
            
            EnsureChoosePanelValid();
            ShowChoosePanel();
        }

        private void OnCustomPresetButtonClicked()
        {
            _sessionService.CreateNewGameplayPreset();
            EnsureCustomPanelValid();
            
            ShowCustomPanel();
        }

        private void OnPlayButtonClicked()
        {
            _sessionService.StartSession();
        }

        private void OnCloseButtonClicked()
        {
            _userInterfaceService.Hide(USER_INTERFACE_IDENTITIES.GAMEPLAY_PRESET_UI);
        }

        public override void Show()
        {
            var firstGameplayPreset = _projectSettings.GameplayPresets.First().PresetReference;
            _sessionService.ChooseGameplayPreset(firstGameplayPreset);
            
            EnsureChoosePanelValid();
            ShowChoosePanel();
            
            gameObject.SetActive(true);
        }

        private void ShowChoosePanel()
        {
            _choosePanel.anchoredPosition = Vector2.zero;
            _customPanel.anchoredPosition = new Vector2(Screen.width, 0f);
        }

        private void EnsureChoosePanelValid()
        {
            var currentGameplayPreset = _sessionService.GameplayPreset;

            if (currentGameplayPreset == null)
            {
                var pair = _chooseToggles[0];
                pair.Toggle.isOn = true;
            }
            else
            {
                var identity =
                    _projectSettings.GameplayPresets.First(x =>
                        x.PresetReference.Equals(currentGameplayPreset)).Identity;
                var pair = _chooseToggles.First(x => x.Identity.Equals(identity));
                
                pair.Toggle.isOn = true;
            }
        }

        private void ShowCustomPanel()
        {
            _choosePanel.anchoredPosition = new Vector2(-Screen.width, 0f);
            _customPanel.anchoredPosition = Vector2.zero;
        }

        private void EnsureCustomPanelValid()
        {
            var currentGameplayPreset = _sessionService.GameplayPreset;
            
            _widthSnapSlider.SetLimits(_projectSettings.SessionSettings.MapWidth.x, _projectSettings.SessionSettings.MapWidth.y);
            _heightSnapSlider.SetLimits(_projectSettings.SessionSettings.MapHeight.x, _projectSettings.SessionSettings.MapHeight.y);

            var width = currentGameplayPreset.Width;
            _widthSnapSlider.Value = width;
            
            var height = currentGameplayPreset.Height;
            _heightSnapSlider.Value = height;
            
            var mineCount = currentGameplayPreset.MineCount;
            _mineCountTMP.text = mineCount.ToString();
            
            var timerValue = TimeSpan.FromSeconds(currentGameplayPreset.TimerSeconds);
            _timerValueTMP.SetText($"{timerValue.Minutes:00}m {timerValue.Seconds:00}s");
        }

        public override void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
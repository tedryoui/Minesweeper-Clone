using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using _.Scripts.Extensions;
using _.Scripts.Gameplay;
using _.Scripts.Gameplay.Scriiptable_Objects;
using _.Scripts.Models;
using _.Scripts.Scriptable_Objects;
using _.Scripts.User_Interface;
using _.Scripts.User_Interface.Elements;
using _.Scripts.Utility;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;
using Timer = _.Scripts.Utility.Timer;

namespace _.Scripts.Services
{
    public class SessionService : ITickable
    {
#region VContainer

        [Inject] private UserInterfaceService _userInterfaceService;
        [Inject] private ProjectSettings      _projectSettings;

        [Inject]
        private void Configure(DataService dataService)
        {
            _sessionModel = dataService.GetModel<SessionModel>(MODEL_IDENTITIES.SESSION_MODEL);
        }

#endregion

#region Fields

        private SessionModel         _sessionModel;
        private SessionUserInterface _sessionUserInterface;
        
        private string           _gameplayPresetIdentity;
        private GameplayPreset   _gameplayPreset;
        private SessionBehaviour _sessionBehaviour;
        private bool             _isRevealed;
        private int2             _initialCellIdentity;

#endregion

#region Properties

        public GameplayPreset GameplayPreset => _gameplayPreset;

#endregion

#region Events

        private event Action _onGameplayPresetChanged;

#endregion

        public SessionService()
        {
            _gameplayPresetIdentity = string.Empty;
            _gameplayPreset         = null;
            _sessionBehaviour       = null;
            _isRevealed             = false;
            _initialCellIdentity    = new int2(-1, -1);
            
            _onGameplayPresetChanged = null;
        }
        
        public void Tick()
        {
#if UNITY_EDITOR
            if (_sessionBehaviour != null && _sessionUserInterface != null)
            {
                TickDebug();
            }
#endif
        }

#region Private Methods

#if UNITY_EDITOR
        
        private void TickDebug()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (_isRevealed)
                    RevealAll(false);
                else 
                    RevealAll(true);
            }
        }
        
#endif
        
#region Gameplay User Interface (VIEW_MODEL CONTEXT)

        private void BindGameplayUserInterface()
        {
            SetChoosePanelEventCallbacks();
            SetCustomPanelEventCallbacks();

            _userInterfaceService.OnUserInterfaceRemoved += UnbindGameplayUserInterface;
            _onGameplayPresetChanged                     += OnGameplayPresetChanged;
        }
        
#region Chose Panel

        private void SetChoosePanelEventCallbacks()
        {
            var ui = _userInterfaceService.GetUserInterface<GameplayPresetUserInterface>(USER_INTERFACE_IDENTITIES.GAMEPLAY_PRESET_UI);
            
            ui.OnPlayButtonClicked(StartSession);
            ui.OnCustomPresetButtonClicked(OnCustomPresetButtonClicked);
            ui.OnChoosePresetToggleActiveChanged(OnChoosePresetActiveChanged);
        }

        private void OnCustomPresetButtonClicked()
        {
            CreateCustomGameplayPreset();
            
            _onGameplayPresetChanged?.Invoke();
        }

        private void OnChoosePresetActiveChanged(string identity)
        {
            var gameplayPreset = _projectSettings.GameplayPresets[identity];

            if (gameplayPreset.GetHashCode() != _gameplayPreset.GetHashCode())
            {
                _gameplayPresetIdentity = identity;
                _gameplayPreset         = gameplayPreset;
                
                _onGameplayPresetChanged?.Invoke();
            }
        }

        private void UnbindGameplayUserInterface(string identity)
        {
            if (identity.Equals(USER_INTERFACE_IDENTITIES.GAMEPLAY_PRESET_UI))
            {
                _onGameplayPresetChanged -= OnGameplayPresetChanged;
                
                _userInterfaceService.OnUserInterfaceRemoved -= UnbindGameplayUserInterface;
            }
        }

#endregion

#region Custom Panel

        private void SetCustomPanelEventCallbacks()
        {
            var ui = _userInterfaceService.GetUserInterface<GameplayPresetUserInterface>(USER_INTERFACE_IDENTITIES.GAMEPLAY_PRESET_UI);

            ui.OnBackButtonClicked(OnBackButtonClicked);
            
            ui.OnIncreaseMineCountButtonClicked(OnIncreaseMineCountButtonClicked);
            ui.OnDecreaseMineCountButtonClicked(OnDecreaseMineCountButtonClicked);
            
            ui.OnPreviousButtonClicked(OnPreviousButtonClicked);
            ui.OnNextButtonClicked(OnNextButtonClicked);
            
            ui.OnWidthSnapSliderValueChanged(OnWidthSnapSliderValueChanged);
            ui.OnHeightSnapSliderValueChanged(OnHeightSnapSliderValueChanged);
        }

        private void OnWidthSnapSliderValueChanged(int value)
        {
            var actualValue = math
                .clamp(
                    value,
                    _projectSettings.SessionSettings.MapWidth.x,
                    _projectSettings.SessionSettings.MapWidth.y
                );

            _gameplayPreset.width     = actualValue;

            var validateMineCount = ClampMinesCount(_gameplayPreset.mineCount);
            if (validateMineCount != _gameplayPreset.mineCount)
            {
                _gameplayPreset.mineCount = validateMineCount;
                
                var ui = _userInterfaceService.GetUserInterface<GameplayPresetUserInterface>(USER_INTERFACE_IDENTITIES.GAMEPLAY_PRESET_UI);
                
                ui.PlayNotifyMineCountAnimation();
            }
            
            _onGameplayPresetChanged?.Invoke();
        }

        private void OnHeightSnapSliderValueChanged(int value)
        {
            var actualValue = math
                .clamp(
                    value,
                    _projectSettings.SessionSettings.MapHeight.x,
                    _projectSettings.SessionSettings.MapHeight.y
                );

            _gameplayPreset.height     = actualValue;
            
            var validateMineCount = ClampMinesCount(_gameplayPreset.mineCount);
            if (validateMineCount != _gameplayPreset.mineCount)
            {
                _gameplayPreset.mineCount = validateMineCount;
                
                var ui = _userInterfaceService.GetUserInterface<GameplayPresetUserInterface>(USER_INTERFACE_IDENTITIES.GAMEPLAY_PRESET_UI);
                
                ui.PlayNotifyMineCountAnimation();
            }
            
            _onGameplayPresetChanged?.Invoke();
        }

        private void OnNextButtonClicked()
        {
            var variants = _projectSettings.SessionSettings.TimerVariants.Values.Keys.ToList();
            var currentTimerSeconds = _gameplayPreset.timerSeconds;
            var identity = _projectSettings.SessionSettings.TimerVariants.Values.First(x => Math.Abs(x.Value - currentTimerSeconds) < 0.1f).Key;
            var index = variants.IndexOf(identity);
            var loopedIndex = (index + 1 >= variants.Count) ? 0 : index + 1;

            _gameplayPreset.timerSeconds = _projectSettings.SessionSettings.TimerVariants.Values[variants[loopedIndex]];
            
            _onGameplayPresetChanged?.Invoke();
        }

        private void OnPreviousButtonClicked()
        {
            var variants = _projectSettings.SessionSettings.TimerVariants.Values.Keys.ToList();
            var currentTimerSeconds = _gameplayPreset.timerSeconds;
            var identity = _projectSettings.SessionSettings.TimerVariants.Values.First(x => Math.Abs(x.Value - currentTimerSeconds) < 0.1f).Key;
            var index = variants.IndexOf(identity);
            var loopedIndex = (index - 1 < 0) ? variants.Count - 1 : index - 1;

            _gameplayPreset.timerSeconds = _projectSettings.SessionSettings.TimerVariants.Values[variants[loopedIndex]];
            
            _onGameplayPresetChanged?.Invoke();
        }

        private void OnDecreaseMineCountButtonClicked()
        {
            var nextMinesCount   = _gameplayPreset.mineCount - 1;
            var actualMinesCount = ClampMinesCount(nextMinesCount);

            if (nextMinesCount != actualMinesCount)
            {
                var ui = _userInterfaceService.GetUserInterface<GameplayPresetUserInterface>(USER_INTERFACE_IDENTITIES.GAMEPLAY_PRESET_UI);

                ui.PlayNotifyMineCountAnimation();
            }
            else
            {
                _gameplayPreset.mineCount = actualMinesCount;
                
                _onGameplayPresetChanged?.Invoke();
            }
        }

        private void OnIncreaseMineCountButtonClicked()
        {
            
            var nextMinesCount   = _gameplayPreset.mineCount + 1;
            var actualMinesCount = ClampMinesCount(nextMinesCount);

            if (nextMinesCount != actualMinesCount)
            {
                var ui = _userInterfaceService.GetUserInterface<GameplayPresetUserInterface>(USER_INTERFACE_IDENTITIES.GAMEPLAY_PRESET_UI);

                ui.PlayNotifyMineCountAnimation();
            }
            else
            {
                _gameplayPreset.mineCount = actualMinesCount;
                
                _onGameplayPresetChanged?.Invoke();
            }
        }

        private void OnBackButtonClicked()
        {
            var gameplayPreset = _projectSettings.GameplayPresets.First();
            
            _gameplayPresetIdentity = gameplayPreset.Key;
            _gameplayPreset          = gameplayPreset.Value;
            
            _onGameplayPresetChanged?.Invoke();
        }

#endregion
        
        private void OnGameplayPresetChanged()
        {
            var ui = _userInterfaceService.GetUserInterface<GameplayPresetUserInterface>(USER_INTERFACE_IDENTITIES.GAMEPLAY_PRESET_UI);
            
            if (_gameplayPresetIdentity.Equals("Custom Preset"))
            {
                ui.SetWidthSnapSliderValue(_gameplayPreset.width);
                ui.SetHeightSnapSliderValue(_gameplayPreset.height);
                ui.SetMinesCount(_gameplayPreset.mineCount);
                ui.SetTimerSeconds(TimeSpan.FromSeconds(_gameplayPreset.timerSeconds));
            }
            else
            {
                ui.SetActiveChoosePresetToggle(_gameplayPresetIdentity);
            }
        }

#endregion

#region Session User Interface (VIEW_MODEL CONTEXT)

        private void BindSessionUserInterface()
        {
            SetSessionUserInterfaceEventCallback();

            _userInterfaceService.OnUserInterfaceRemoved += UnbindSessionUserInterface;
        }

        private void UnbindSessionUserInterface(string identity)
        {
            if (identity.Equals(USER_INTERFACE_IDENTITIES.SESSION_UI))
            {
                
                
                _userInterfaceService.OnUserInterfaceRemoved -= UnbindSessionUserInterface;
            }
        }

        private void SetSessionUserInterfaceEventCallback()
        {
            var ui = _userInterfaceService.GetUserInterface<SessionUserInterface>(USER_INTERFACE_IDENTITIES.SESSION_UI);
            
            ui.OnCloseButtonClicked(OnCloseButtonClicked);
            ui.OnRestartButtonClicked(OnRestartButtonClicked);
            ui.OnConfirmationUserInterfaceShowed(OnConfirmationUserInterfaceShowed);
            ui.OnConfirmationUserInterfaceHidden(OnConfirmationUserInterfaceHidden);
            ui.OnMinesweeperCellClicked(OnMinesweeperCellClicked);
        }

        private void OnMinesweeperCellOpen(int2 identity)
        {
            _sessionBehaviour?.OpenCell(identity);
        }

        private void OnMinesweeperCellFlag(int2 identity)
        {
            _sessionBehaviour?.FlagCell(identity);
        }
        
        private void OnMinesweeperCellClicked(int2 identity, PointerEventData.InputButton button)
        {
            if (_initialCellIdentity.Equals(new int2(-1, -1)))
            {
                _initialCellIdentity = identity;
                return;
            }
            
            var isRevealed = _sessionBehaviour.Grid.GetNode(identity).Value.overwrittenFlags.HasFlag(SessionBehaviour.Cell.CellType.Opened);
            
            if (!isRevealed)
            {
                var ui = _userInterfaceService.GetUserInterface<SessionUserInterface>(USER_INTERFACE_IDENTITIES
                    .SESSION_UI);

                _sessionModel.Steps++;
                ui.SetStepsCount(_sessionModel.Steps);
            }
            
            if (button is PointerEventData.InputButton.Left)
                OnMinesweeperCellOpen(identity);
            else if (button is PointerEventData.InputButton.Right)
                OnMinesweeperCellFlag(identity);
        }

        private void OnConfirmationUserInterfaceHidden()
        {
            ResumeSession();
        }

        private void OnConfirmationUserInterfaceShowed()
        {
            PauseSession();
        }

        private void OnCloseButtonClicked()
        {
            EndSession();
        }

        private void OnRestartButtonClicked()
        {
            RestartSession();
        }

#endregion
        
#region Custom Gameplay Preset Generation

        private void CreateCustomGameplayPreset()
        {
            _gameplayPresetIdentity = "Custom Preset";
            _gameplayPreset         = ScriptableObject.CreateInstance<GameplayPreset>();
            
            RandomizeGameplayPresetWidth();
            RandomizeGameplayPresetHeight();
            RandomizeGameplayPresetMineCount();
            RandomizeGameplayPresetTimerSeconds();
        }

        private void RandomizeGameplayPresetWidth()
        {
            var random = new Unity.Mathematics.Random((uint)System.DateTime.Now.Ticks);
            var randomWidth = random
                .NextInt(
                    _projectSettings.SessionSettings.MapWidth.x, 
                    _projectSettings.SessionSettings.MapWidth.y
                );
            
            _gameplayPreset.width  = randomWidth;
        }

        private void RandomizeGameplayPresetHeight()
        {
            var random = new Unity.Mathematics.Random((uint)System.DateTime.Now.Ticks);
            var randomHeight = random
                .NextInt(
                    _projectSettings.SessionSettings.MapHeight.x, 
                    _projectSettings.SessionSettings.MapHeight.y
                );
            
            _gameplayPreset.height  = randomHeight;
        }

        private void RandomizeGameplayPresetMineCount()
        {
            var random = new Unity.Mathematics.Random((uint)System.DateTime.Now.Ticks);
            var size   = _gameplayPreset.width * _gameplayPreset.height;
            var percentage = random
                .NextFloat(
                    _projectSettings.SessionSettings.MineCountPercentage.x,
                    _projectSettings.SessionSettings.MineCountPercentage.y
                );
            
            _gameplayPreset.mineCount = (int)(size * percentage);
        }

        private void RandomizeGameplayPresetTimerSeconds()
        {
            var random            = new Unity.Mathematics.Random((uint)System.DateTime.Now.Ticks);
            var variantIdentities = _projectSettings.SessionSettings.TimerVariants.Values.Keys.ToArray();
            var variantCount      = variantIdentities.Count();
            var variantIndex      = random.NextInt(0,  variantCount);
            var variant           = variantIdentities[variantIndex];
            
            _gameplayPreset.timerSeconds = _projectSettings.SessionSettings.TimerVariants.Values[variant];
        }

#endregion
      
#region Session Startup

        private void StartSession()
        {
            RegisterSessionUserInterface().ContinueWith(_ =>
            {
                _sessionUserInterface = _userInterfaceService.GetUserInterface<SessionUserInterface>(USER_INTERFACE_IDENTITIES.SESSION_UI);
                Bind(USER_INTERFACE_IDENTITIES.SESSION_UI);

                var cancellationToken = CancellationTokenSource
                    .CreateLinkedTokenSource(
                        Application.exitCancellationToken,
                        _sessionUserInterface.destroyCancellationToken
                    ).Token;
                
                RunUserInterfaceTweenSynchronously(cancellationToken);
                
                InitializeSession(cancellationToken);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private async Task RunUserInterfaceTweenSynchronously(CancellationToken cancellationToken = default)
        {
            _userInterfaceService.Hide(USER_INTERFACE_IDENTITIES.GAMEPLAY_PRESET_UI);

            await WaitForUserInterfaceTweenCompleted(USER_INTERFACE_IDENTITIES.GAMEPLAY_PRESET_UI, cancellationToken);
            
            _userInterfaceService.Hide(USER_INTERFACE_IDENTITIES.MENU_UI);
            
            await WaitForUserInterfaceTweenCompleted(USER_INTERFACE_IDENTITIES.MENU_UI, cancellationToken);
            
            _userInterfaceService.Show(USER_INTERFACE_IDENTITIES.SESSION_UI);
        }

        private async Task InitializeSession(CancellationToken cancellationToken = default)
        {
            async Task WaitForValidStartCellIdentity(CancellationToken cancellationToken = default)
            {
                while (_initialCellIdentity.Equals(new int2(-1, -1)) && !cancellationToken.IsCancellationRequested)
                    await Task.Yield();
            }
            
            _initialCellIdentity  = new int2(-1, -1);
            
            _sessionUserInterface.SetSmartGridSize(_gameplayPreset.width, _gameplayPreset.height);
            
            _sessionUserInterface.SetTimerFillText("Wait for click!");
            _sessionUserInterface.SetTimerFill(1f);
            
            await WaitForValidStartCellIdentity(cancellationToken);
            
            ExportGameplayPresetValues();
            _sessionBehaviour = BuildSessionBehaviour();

            _sessionBehaviour.Timer.OnTimerCompleted += OnTimerCompleted;
            _sessionBehaviour.Timer.OnTimerChanged   += OnTimerChanged;
            
            _sessionUserInterface.SetStepsCount(_sessionModel.Steps);
            _sessionUserInterface.SetMineCount(_sessionModel.MineCount);
            _sessionUserInterface.SetSmartGridSize(_sessionModel.Width, _sessionModel.Height);

            _sessionBehaviour.onCellChanged += OnSessionBehaviourCellChanged;
            _sessionBehaviour.onMine        += OnMineRevealed;

            await Task.Delay(250, cancellationToken);
                
            OnMinesweeperCellClicked(_initialCellIdentity, PointerEventData.InputButton.Left);
            
            RevealAll(false);
        }

        private void OnTimerChanged()
        {
            if ((int)_sessionModel.CurrentTimerSeconds != (int)_sessionBehaviour.Timer.CurrentValue)
                _sessionUserInterface.PlayTimerTickClip();
            
            _sessionModel.CurrentTimerSeconds = _sessionBehaviour.Timer.CurrentValue;
            
            var totalSeconds   = _sessionModel.TimerSeconds;
            var currentSeconds = _sessionModel.CurrentTimerSeconds;
            var fillAmount     = 1.0f - currentSeconds / totalSeconds;
                
            _sessionUserInterface.SetTimerFill(fillAmount);
            _sessionUserInterface.SetTimerFillText(TimeSpan.FromSeconds(totalSeconds - currentSeconds));
        }

        private void ExportGameplayPresetValues()
        {
            _sessionModel.Width               = _gameplayPreset.width;
            _sessionModel.Height              = _gameplayPreset.height;
            _sessionModel.MineCount           = _gameplayPreset.mineCount;
            _sessionModel.TimerSeconds        = _gameplayPreset.timerSeconds;
            _sessionModel.CurrentTimerSeconds = 0f;
            _sessionModel.Steps               = 0;
        }
        
        private SessionBehaviour BuildSessionBehaviour()
        {
            var grid               = new GridStructure<SessionBehaviour.Cell>(
                _sessionModel.Width, 
                _sessionModel.Height, 
                () => SessionBehaviour.Cell.Default
                );
            var mineCellIdentities = CreateMineCellIdentities(grid.Nodes.Values.Select(x => x.Identity).Where(x => !x.Equals(_initialCellIdentity)).ToArray()).ToHashSet();
            foreach (var mineCellIdentity in mineCellIdentities)
            {
                var node     = grid.GetNode(mineCellIdentity);
                node.Value.initialFlags = SessionBehaviour.Cell.CellType.Mine;
            }

            for (int x = 0; x < _sessionModel.Width; x++)
            {
                for (int y = 0; y < _sessionModel.Height; y++)
                {
                    var identity        = new int2(x, y);
                    var node            = grid.GetNode(identity);

                    if (node.Value.initialFlags is SessionBehaviour.Cell.CellType.Mine)
                        continue;
                    
                    var neighboursNodes = grid.GetNeighborsFor(identity);
                    var mineCount       = 0;
                    
                    foreach (var neighbourNode in neighboursNodes)
                    {
                        if (neighbourNode.Value.initialFlags is SessionBehaviour.Cell.CellType.Mine)
                            mineCount++;
                    }
                    
                    if (mineCount != 0)
                    {
                        node.Value.minesAround  = mineCount;
                        node.Value.initialFlags = SessionBehaviour.Cell.CellType.Number;
                    }
                }
            }

            var timer = new Timer(_sessionModel.TimerSeconds);
            
            return new SessionBehaviour(grid, timer);
        }

        private IEnumerable<int2> CreateMineCellIdentities(ICollection<int2> identities)
        {
            var copy      = identities.ToHashSet();
            var random    = new Unity.Mathematics.Random();
            var mineCount = _sessionModel.MineCount;

            for (int m = 0; m < mineCount; m++)
            {
                random.InitState((uint)UnityEngine.Random.Range(0, 999999));
                
                var identity = copy.Random(random);
                copy.Remove(identity);
                
                yield return identity;
            }
        }
        
        private void OnMineRevealed()
        {
            Lose();
        }
        
        private void OnSessionBehaviourCellChanged(int2 identity)
        {
            var minesweeperCell = _sessionUserInterface.GetMinesweeperCell(identity);
            VisualizeMinesweeperCell(identity, minesweeperCell, true);
            
            if (IsWinThroughClosedCells() || IsWinThroughFlaggedCells())
                Win();
        }
        
        private void OnTimerCompleted()
        {
            Lose();
        }

#endregion

#region Session State Control

        private void Win()
        {
            PauseSession();
            RevealAll(true);
            _userInterfaceService.Show(USER_INTERFACE_IDENTITIES.WIN_UI);

            var winUserInterface = _userInterfaceService.GetUserInterface<WinUserInterface>(USER_INTERFACE_IDENTITIES.WIN_UI);

            var score = (int)(_sessionModel.TimerSeconds - _sessionBehaviour.Timer.CurrentValue +
                _sessionModel.MineCount * 5 - _sessionModel.Steps * 3);
            
            winUserInterface.SetScore(score < 0 ? 0 : score);
            winUserInterface.OnMenuButtonClicked(EndSession);
        }

        private void Lose()
        {
            PauseSession();
            RevealAll(true);
            _userInterfaceService.Show(USER_INTERFACE_IDENTITIES.LOSE_UI);

            var loseUserInterface =
                _userInterfaceService.GetUserInterface<LoseUserInterface>(USER_INTERFACE_IDENTITIES
                    .LOSE_UI);
            loseUserInterface.OnMenuButtonClicked(EndSession);
            loseUserInterface.OnRestartButtonClicked(RestartSession);
        }
        
        private void RestartSession()
        {
            var cancellationToken = CancellationTokenSource
                .CreateLinkedTokenSource(
                    Application.exitCancellationToken,
                    _sessionUserInterface.destroyCancellationToken
                )
                .Token;
            
            InitializeSession(cancellationToken);
        }

        private void PauseSession()
        {
            _sessionBehaviour.Timer.Pause();
        }

        private void ResumeSession()
        {
            _sessionBehaviour.Timer.Pause(false);
        }
        
        private void EndSession()
        {
            _sessionBehaviour.Exit();

            _gameplayPreset       = null;
            _sessionBehaviour     = null;
            _sessionUserInterface = null;

            RemoveSessionUserInterface();
            
            _userInterfaceService.Show(USER_INTERFACE_IDENTITIES.MENU_UI);
        }

#endregion

#region Session Utilities

        private void VisualizeMinesweeperCell(int2 identity, MinesweeperCell minesweeperCell, bool doAnimate = false)
        {
            var data  = _sessionBehaviour.Grid.GetNode(identity);
            var flags = (_isRevealed) ? data.Value.initialFlags : data.Value.overwrittenFlags;

            switch (flags)
            {
                case SessionBehaviour.Cell.CellType.Closed:
                    minesweeperCell.Close();
                    minesweeperCell.EnableFlag(false, doAnimate);
                    break;
                case SessionBehaviour.Cell.CellType.Flag:
                    minesweeperCell.Close();
                    minesweeperCell.EnableFlag(true, doAnimate);
                    break;
                case SessionBehaviour.Cell.CellType.Opened:
                    minesweeperCell.EnableBomb(false, false);
                    minesweeperCell.EnableNumber(false, 0, false);
                    minesweeperCell.Open(doAnimate);
                    break;
                case SessionBehaviour.Cell.CellType.Empty:
                    minesweeperCell.EnableNumber(false, 0, false);
                    minesweeperCell.EnableBomb(false, false);
                    minesweeperCell.Open(doAnimate);
                    break;
                case SessionBehaviour.Cell.CellType.Mine:
                    minesweeperCell.EnableNumber(false, 0, false);
                    minesweeperCell.EnableBomb(true, false);
                    minesweeperCell.Open(doAnimate);
                    break;
                case SessionBehaviour.Cell.CellType.Number:
                    minesweeperCell.EnableBomb(false, false);
                    minesweeperCell.EnableNumber(true, data.Value.minesAround, false);
                    minesweeperCell.Open(doAnimate);
                    break;
            }
        }

        private bool IsWinThroughFlaggedCells()
        {
            if (_sessionBehaviour.FlaggedCellIdentities.Count == _sessionModel.MineCount)
            {
                foreach (var cell in _sessionBehaviour.FlaggedCellIdentities)
                {
                    var data   = _sessionBehaviour.Grid.GetNode(cell);
                    var isMine = data.Value.initialFlags is SessionBehaviour.Cell.CellType.Mine;

                    if (!isMine)
                        return false;
                }
                return true;
            }
            return false;
        }

        private bool IsWinThroughClosedCells()
        {
            if (_sessionBehaviour.ClosedCellIdentities.Count == _sessionModel.MineCount)
            {
                foreach (var cell in _sessionBehaviour.ClosedCellIdentities)
                {
                    var data   = _sessionBehaviour.Grid.GetNode(cell);
                    var isMine = data.Value.initialFlags is SessionBehaviour.Cell.CellType.Mine;

                    if (!isMine)
                        return false;
                }
                return true;
            }
            return false;
        }
        
        private void RevealAll(bool reveal)
        {
            var nodes = _sessionBehaviour.Grid.Nodes.Values;
            _isRevealed = reveal;

            foreach (var node in nodes)
            {
                var identity = node.Identity;
                var cell     = _sessionUserInterface.GetMinesweeperCell(identity);

                VisualizeMinesweeperCell(identity, cell);
            }
        }
        
        private int ClampMinesCount(int value)
        {
            var size              = _gameplayPreset.width * _gameplayPreset.height;
            var minimumMinesCount = math.ceil(size * _projectSettings.SessionSettings.MineCountPercentage.x);
            var maximumMinesCount = math.ceil(size * _projectSettings.SessionSettings.MineCountPercentage.y);
            var actualMinesCount = (int)math
                .clamp(
                    value,
                    minimumMinesCount,
                    maximumMinesCount
                );

            return actualMinesCount;
        }

#endregion

#region User Interface Managing

        private async Task RegisterSessionUserInterface()
        {
            await InstantiateUserInterface(USER_INTERFACE_IDENTITIES.SESSION_UI);
            await InstantiateUserInterface(USER_INTERFACE_IDENTITIES.WIN_UI);
            await InstantiateUserInterface(USER_INTERFACE_IDENTITIES.LOSE_UI);
        }
        
        private void RemoveSessionUserInterface()
        {
            _userInterfaceService.RemoveUserInterface(USER_INTERFACE_IDENTITIES.SESSION_UI, out var sessionUI);
            Object.Destroy(sessionUI.gameObject);
            var sessionUIAssetReference = FindUserInterfaceAssetReference(USER_INTERFACE_IDENTITIES.SESSION_UI);
            if (sessionUIAssetReference.IsValid())
                sessionUIAssetReference.ReleaseAsset();
            
            _userInterfaceService.RemoveUserInterface(USER_INTERFACE_IDENTITIES.WIN_UI, out var winUI);
            Object.Destroy(winUI.gameObject);
            var winUIAssetReference = FindUserInterfaceAssetReference(USER_INTERFACE_IDENTITIES.WIN_UI);
            if (winUIAssetReference.IsValid())
                winUIAssetReference.ReleaseAsset();
            
            _userInterfaceService.RemoveUserInterface(USER_INTERFACE_IDENTITIES.LOSE_UI, out var loseUI);
            Object.Destroy(loseUI.gameObject);
            var loseUIAssetReference = FindUserInterfaceAssetReference(USER_INTERFACE_IDENTITIES.LOSE_UI);
            if (loseUIAssetReference.IsValid())
                loseUIAssetReference.ReleaseAsset();
        }
        
        private async Task InstantiateUserInterface(string identity)
        {
            var assetReference          = FindUserInterfaceAssetReference(identity);

            var loadedAsset = await assetReference.LoadAssetAsync<GameObject>().Task;

            var result = await Object.InstantiateAsync(loadedAsset, _userInterfaceService.Root);
            
            if (result.First().TryGetComponent(out AbstractUserInterface userInterface))
                _userInterfaceService.RegisterUserInterface(identity, userInterface);
            else 
                throw new Exception($"{identity} user interface was not found.");
        }
        
        private AssetReference FindUserInterfaceAssetReference(string identity)
        {
            if (!_projectSettings.UserInterfaces.TryGetValue(identity, out var assetReference))
                throw new Exception($"{identity} user interface was not found.");
            
            return assetReference;
        }

        private async Task WaitForUserInterfaceTweenCompleted(string identity, CancellationToken cancellationToken = default)
        {
            var userInterface = _userInterfaceService.GetUserInterface(identity);
            
            while (DOTween.IsTweening(userInterface.gameObject))
                await Task.Yield();
        }

#endregion
        
#endregion

#region Public API

        public void Bind(params string[] userInterfaceIdentities)
        {
            foreach (var identity in userInterfaceIdentities)
            {
                if (!_userInterfaceService.HasUserInterface(identity))
                    continue;
                
                switch (identity)
                {
                    case USER_INTERFACE_IDENTITIES.GAMEPLAY_PRESET_UI:
                        BindGameplayUserInterface();
                        break;
                    case USER_INTERFACE_IDENTITIES.SESSION_UI:
                        BindSessionUserInterface();
                        break;
                    default:
                        continue;
                }
            }
        }

#endregion
    }
}
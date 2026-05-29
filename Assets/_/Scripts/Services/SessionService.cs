using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _.Scripts.Extensions;
using _.Scripts.Gameplay;
using _.Scripts.Gameplay.Scriiptable_Objects;
using _.Scripts.Scriptable_Objects;
using _.Scripts.Structures;
using _.Scripts.User_Interface;
using _.Scripts.User_Interface.Elements;
using _.Scripts.Utility;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace _.Scripts.Services
{
    public class SessionService : ITickable
    {
        [Inject] private UserInterfaceService _userInterfaceService;
        [Inject] private ProjectSettings _projectSettings;
        
        private GameplayPreset       _gameplayPreset;
        private SessionBehaviour     _sessionBehaviour;
        private SessionUserInterface _sessionUserInterface;
        private bool                 _isRevealed;

        public GameplayPreset GameplayPreset => _gameplayPreset;

        public SessionService()
        {
            _gameplayPreset = null;
        }

        public void StartSession()
        {
            void InitializeSession()
            {
                _sessionBehaviour = CreateSession();

                _sessionBehaviour.Timer.OnTimerCompleted += EndSession;
                
                _sessionUserInterface.SetMineCount(_gameplayPreset.MineCount);
                _sessionUserInterface.SetSmartGridSize(_gameplayPreset.Width, _gameplayPreset.Height);
                _sessionUserInterface.SetCellClickedAction(OnCellClicked);
                
                RevealAll(false);
            }
            
            if (_sessionUserInterface == null)
            {
                RegisterSessionUserInterface().ContinueWith(_ =>
                {
                    _userInterfaceService.Show(USER_INTERFACE_IDENTITIES.SESSION_UI);

                    _userInterfaceService.Hide(USER_INTERFACE_IDENTITIES.GAMEPLAY_PRESET_UI);
                    _userInterfaceService.Hide(USER_INTERFACE_IDENTITIES.MENU_UI);

                    _sessionUserInterface =
                        _userInterfaceService.GetUserInterface<SessionUserInterface>(USER_INTERFACE_IDENTITIES
                            .SESSION_UI);

                    _sessionUserInterface.OnConfirmationUserInterfaceShowed(PauseSession);
                    _sessionUserInterface.OnConfirmationUserInterfaceHidden(RestartSession);
                    
                    _sessionUserInterface.OnCloseButtonClicked(EndSession);
                    _sessionUserInterface.OnRestartButtonClicked(RestartSession);
                    
                    InitializeSession();
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            else
            {
                InitializeSession();
            }
        }

        private void OnCellClicked(int2 identity, PointerEventData.InputButton button, MinesweeperCell cell)
        {
            var data  = _sessionBehaviour.Grid.GetNode(identity);
                    var initialFlags = data.Value.initialFlags;
                    var overwrittenFlags = data.Value.overwrittenFlags;

                    if (overwrittenFlags is SessionBehaviour.Cell.CellType.Closed or SessionBehaviour.Cell.CellType.Flag)
                    {
                        if (button is PointerEventData.InputButton.Left)
                        {
                            data.Value.overwrittenFlags = initialFlags;
                            overwrittenFlags            = initialFlags;
                            
                            _sessionBehaviour.OpenCell(identity);
                        }
                        else if (button is PointerEventData.InputButton.Right)
                        {
                            if (overwrittenFlags is SessionBehaviour.Cell.CellType.Flag)
                            {
                                data.Value.overwrittenFlags = SessionBehaviour.Cell.CellType.Closed;
                                overwrittenFlags            = SessionBehaviour.Cell.CellType.Closed;
                                
                                _sessionBehaviour.FlagCell(identity, false);
                            }
                            else
                            {
                                data.Value.overwrittenFlags = SessionBehaviour.Cell.CellType.Flag;
                                overwrittenFlags            = SessionBehaviour.Cell.CellType.Flag;

                                _sessionBehaviour.FlagCell(identity, true);
                            }
                        }
                    }

                    if (overwrittenFlags is SessionBehaviour.Cell.CellType.Mine)
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

                    if (_gameplayPreset.MineCount == _sessionBehaviour.ClosedCellIdentities.Count ||
                        _gameplayPreset.MineCount == _sessionBehaviour.FlaggedCellIdentities.Count)
                    {
                        bool isWon = true;
                        
                        if (_gameplayPreset.MineCount == _sessionBehaviour.ClosedCellIdentities.Count)
                        {
                            foreach (var closedCellIdentity in _sessionBehaviour.ClosedCellIdentities)
                            {
                                var closedCellData = _sessionBehaviour.Grid.GetNode(closedCellIdentity);

                                if (closedCellData.Value.initialFlags is not SessionBehaviour.Cell.CellType.Mine)
                                {
                                    isWon = false;
                                    break;
                                }
                            }
                        } else if (_gameplayPreset.MineCount == _sessionBehaviour.FlaggedCellIdentities.Count)
                        {
                            foreach (var flaggedCellIdentity in _sessionBehaviour.FlaggedCellIdentities)
                            {
                                var flaggedCellData = _sessionBehaviour.Grid.GetNode(flaggedCellIdentity);

                                if (flaggedCellData.Value.initialFlags is not SessionBehaviour.Cell.CellType.Mine)
                                {
                                    isWon = false;
                                    break;
                                }
                            }
                        }

                        if (isWon)
                        {
                            PauseSession();
                            RevealAll(true);
                            _userInterfaceService.Show(USER_INTERFACE_IDENTITIES.WIN_UI);

                            var winUserInterface = _userInterfaceService.GetUserInterface<WinUserInterface>(USER_INTERFACE_IDENTITIES.WIN_UI);
                            winUserInterface.SetScore((int)(_gameplayPreset.TimerSeconds -
                                _sessionBehaviour.Timer.CurrentValue + _gameplayPreset.MineCount * 5));
                            winUserInterface.OnMenuButtonClicked(EndSession);
                        }
                    }

                    if (overwrittenFlags is SessionBehaviour.Cell.CellType.Empty)
                    {
                        var neighborsIdentities = data.NeighborsIdentities;
                        
                        foreach (var neighborIdentity in neighborsIdentities)
                        {
                            var neighborData = _sessionBehaviour.Grid.GetNode(neighborIdentity);
                            
                            if (neighborData.Value.overwrittenFlags is not SessionBehaviour.Cell.CellType.Closed)
                                continue;
                            
                            var neighborMinesweeperCell = _sessionUserInterface.GetCell(neighborIdentity);
                            
                            OnCellClicked(neighborIdentity, PointerEventData.InputButton.Left, neighborMinesweeperCell);
                        }
                    }

                    switch (overwrittenFlags)
                    {
                        case SessionBehaviour.Cell.CellType.Closed:
                            cell.Close();
                            cell.EnableFlag(false);
                            break;
                        case SessionBehaviour.Cell.CellType.Flag:
                            cell.Close();
                            cell.EnableFlag(true);
                            break;
                        case SessionBehaviour.Cell.CellType.Opened:
                            cell.Open();
                            cell.EnableBomb(false);
                            cell.EnableNumber(false, 0);
                            break;
                        case SessionBehaviour.Cell.CellType.Empty:
                            cell.Open();
                            cell.EnableNumber(false, 0);
                            cell.EnableBomb(false);
                            break;
                        case SessionBehaviour.Cell.CellType.Mine:
                            cell.Open();
                            cell.EnableBomb(true);
                            cell.EnableNumber(false, 0);
                            break;
                        case SessionBehaviour.Cell.CellType.Number:
                            cell.Open();
                            cell.EnableBomb(false);
                            cell.EnableNumber(true, data.Value.minesAround);
                            break;
                    }
        }

        private void RestartSession()
        {
            StartSession();
        }

        private void PauseSession()
        {
            _sessionBehaviour.Timer.Pause();
        }

        private void ResumeSession()
        {
            _sessionBehaviour.Timer.Pause(false);
        }

        public void Tick()
        {
            if (_sessionBehaviour != null && _sessionUserInterface != null)
            {
                var totalSeconds   = _gameplayPreset.TimerSeconds;
                var currentSeconds = _sessionBehaviour.Timer.CurrentValue;
                var fillAmount     = 1.0f - currentSeconds / totalSeconds;
                
                _sessionUserInterface.SetTimerFill(fillAmount);
                _sessionUserInterface.SetTimerFillText(TimeSpan.FromSeconds(totalSeconds - currentSeconds));
                
#if UNITY_EDITOR

                TickDebug();

#endif
            }
        }

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

        private void RevealAll(bool reveal)
        {
            var nodes = _sessionBehaviour.Grid.Nodes.Values;

            foreach (var node in nodes)
            {
                var identity = node.Identity;
                var cell     = _sessionUserInterface.GetCell(identity);

                var type = reveal ? node.Value.initialFlags : node.Value.overwrittenFlags;
                
                switch (type)
                {
                    case SessionBehaviour.Cell.CellType.Closed:
                        cell.Close();
                        cell.EnableFlag(false);
                        break;
                    case SessionBehaviour.Cell.CellType.Flag:
                        cell.Close();
                        cell.EnableFlag(true);
                        break;
                    case SessionBehaviour.Cell.CellType.Opened:
                        cell.Open();
                        cell.EnableBomb(false);
                        cell.EnableNumber(false, 0);
                        break;
                    case SessionBehaviour.Cell.CellType.Empty:
                        cell.Open();
                        cell.EnableNumber(false, 0);
                        cell.EnableBomb(false);
                        break;
                    case SessionBehaviour.Cell.CellType.Mine:
                        cell.Open();
                        cell.EnableBomb(true);
                        cell.EnableNumber(false, 0);
                        break;
                    case SessionBehaviour.Cell.CellType.Number:
                        cell.Open();
                        cell.EnableBomb(false);
                        cell.EnableNumber(true, node.Value.minesAround);
                        break;
                }
            }

            _isRevealed = reveal;
        }

        private async Task RegisterSessionUserInterface()
        {
            await InstantiateUserInterface(USER_INTERFACE_IDENTITIES.SESSION_UI);
            await InstantiateUserInterface(USER_INTERFACE_IDENTITIES.WIN_UI);
            await InstantiateUserInterface(USER_INTERFACE_IDENTITIES.LOSE_UI);
        }
        
        private async Task InstantiateUserInterface(string identity)
        {
            var prefab          = FindUserInterface(identity);
            var instantiateTask = Object.InstantiateAsync(prefab, _userInterfaceService.Root);
            
            await instantiateTask;

            var result = instantiateTask.Result[0];
            _userInterfaceService.RegisterUserInterface(identity, result);
        }
        
        private AbstractUserInterface FindUserInterface(string identity)
        {
            var pair = _projectSettings.UserInterfacePairs.FirstOrDefault(x => x.Identity.Equals(identity));
            
            if (string.IsNullOrEmpty(pair.Identity))
                throw new Exception($"{identity} user interface was not found.");
            
            return pair.UserInterface;
        }

        public void EndSession()
        {
            _sessionBehaviour.Exit();

            _gameplayPreset       = null;
            _sessionBehaviour     = null;
            _sessionUserInterface = null;

            RemoveSessionUserInterface();
            
            _userInterfaceService.Show(USER_INTERFACE_IDENTITIES.MENU_UI);
        }

        private void RemoveSessionUserInterface()
        {
            _userInterfaceService.RemoveUserInterface(USER_INTERFACE_IDENTITIES.SESSION_UI, out var sessionUI);
            Object.Destroy(sessionUI.gameObject);
            
            _userInterfaceService.RemoveUserInterface(USER_INTERFACE_IDENTITIES.WIN_UI, out var winUI);
            Object.Destroy(winUI.gameObject);
            
            _userInterfaceService.RemoveUserInterface(USER_INTERFACE_IDENTITIES.LOSE_UI, out var loseUI);
            Object.Destroy(loseUI.gameObject);
        }

        public void CreateNewGameplayPreset()
        {
            var random = new Unity.Mathematics.Random((uint)System.DateTime.Now.Ticks);
            
            _gameplayPreset = ScriptableObject.CreateInstance<GameplayPreset>();

            _gameplayPreset.Width = random
                .NextInt(_projectSettings.SessionSettings.MapWidth.x, _projectSettings.SessionSettings.MapWidth.y);
            _gameplayPreset.Height = random
                .NextInt(_projectSettings.SessionSettings.MapHeight.x, _projectSettings.SessionSettings.MapHeight.y);
            _gameplayPreset.MineCount = (int)((_gameplayPreset.Width * _gameplayPreset.Height) * random
                .NextFloat(_projectSettings.SessionSettings.MineCountPercentage.x,
                    _projectSettings.SessionSettings.MineCountPercentage.y));
            _gameplayPreset.TimerSeconds = _projectSettings.SessionSettings.TimerVariants[random
                .NextInt(0, _projectSettings.SessionSettings.TimerVariants.Length)].Seconds;
        }

        public void ChooseGameplayPreset(GameplayPreset gameplayPreset)
        {
            _gameplayPreset = gameplayPreset;
        }
        
        private SessionBehaviour CreateSession()
        {
            var grid               = new GridStructure<SessionBehaviour.Cell>(
                _gameplayPreset.Width, 
                _gameplayPreset.Height, 
                () => SessionBehaviour.Cell.Default
                );
            var mineCellIdentities = CreateMineCellIdentities(grid.Nodes.Values.Select(x => x.Identity).ToArray());

            string debugString = $"Created: { mineCellIdentities.Count() } mine entries!";
            foreach (var cellIdentities in mineCellIdentities)
            {
                debugString += "\n";
                debugString += $"Mine at: {cellIdentities.x}, {cellIdentities.y}";
            }
            Debug.Log(debugString);

            foreach (var identity in mineCellIdentities)
            {
                var cell = grid.GetNode(identity).Value;
                
                cell.initialFlags = SessionBehaviour.Cell.CellType.Mine;
            }

            for (int x = 0; x < _gameplayPreset.Width; x++)
            {
                for (int y = 0; y < _gameplayPreset.Height; y++)
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

            var timer = new Timer(_gameplayPreset.TimerSeconds);
            
            return new SessionBehaviour(grid, timer);
        }

        private IEnumerable<int2> CreateMineCellIdentities(ICollection<int2> identities)
        {
            var copy      = identities.ToHashSet();
            var random    = new Unity.Mathematics.Random();
            var mineCount = _gameplayPreset.MineCount;

            for (int m = 0; m < mineCount; m++)
            {
                random.InitState((uint)UnityEngine.Random.Range(0, 999999));
                
                var identity = copy.Random(random);
                copy.Remove(identity);
                
                yield return identity;
            }
        }
    }
}
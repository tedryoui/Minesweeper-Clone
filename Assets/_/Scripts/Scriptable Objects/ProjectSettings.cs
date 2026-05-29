using System;
using System.Collections.Generic;
using _.Scripts.Attributes;
using _.Scripts.Gameplay.Scriiptable_Objects;
using _.Scripts.Services;
using Unity.Mathematics;
using UnityEngine;

namespace _.Scripts.Scriptable_Objects
{
    [CreateAssetMenu(fileName = "Project Settings", menuName = "Minesweeper/Project Settings", order = 0)]
    public class ProjectSettings : ScriptableObject
    {
        [Serializable]
        public struct GameplayPresetPair
        {
            public string         Identity;
            public GameplayPreset PresetReference;
        }

        [Serializable]
        public struct UserInterfacePair
        {
            public string                Identity;
            public AbstractUserInterface UserInterface;
        }

        [Serializable]
        public struct SessionSettingsCollection
        {
            [Serializable]
            public struct TimerPair
            {
                public string Identity;
                [TimeField]
                public float  Seconds;
            }
            
            public int2        MapWidth;
            public int2        MapHeight;
            public float2      MineCountPercentage;
            public TimerPair[] TimerVariants;
        }
        
        [SerializeField] private List<GameplayPresetPair>  _gameplayPresets;
        [SerializeField] private List<UserInterfacePair>   _userInterfacePairs;
        [SerializeField] private SessionSettingsCollection _sessionSettings;

        public IReadOnlyCollection<GameplayPresetPair> GameplayPresets    => _gameplayPresets;
        public IReadOnlyCollection<UserInterfacePair>  UserInterfacePairs => _userInterfacePairs;
        public SessionSettingsCollection               SessionSettings    => _sessionSettings;
    }
}
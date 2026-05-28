using System;
using System.Collections.Generic;
using _.Scripts.Gameplay.Scriiptable_Objects;
using _.Scripts.Services;
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
        
        [SerializeField] private List<GameplayPresetPair> _gameplayPresets;
        [SerializeField] private List<UserInterfacePair> _userInterfacePairs;

        public IReadOnlyCollection<GameplayPresetPair> GameplayPresets => _gameplayPresets;
        public IReadOnlyCollection<UserInterfacePair> UserInterfacePairs => _userInterfacePairs;
    }
}
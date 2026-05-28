using System;
using System.Collections.Generic;
using _.Scripts.Gameplay.Scriiptable_Objects;
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
        
        [SerializeField] private List<GameplayPresetPair> _gameplayPresets;

        public IReadOnlyCollection<GameplayPresetPair> GameplayPresets => _gameplayPresets;
    }
}
using System;
using System.Collections.Generic;
using _.Scripts.Gameplay.Scriiptable_Objects;
using _.Scripts.Utility;
using _.Scripts.Utility.Asset_References;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace _.Scripts.Scriptable_Objects
{
    [CreateAssetMenu(fileName = "Project Settings", menuName = "Minesweeper/Project Settings", order = 0)]
    public class ProjectSettings : ScriptableObject
    {
#region Structures

        [Serializable]
        public class SessionPreferences
        {
            public int2                          MapWidth;
            public int2                          MapHeight;
            public float2                        MineCountPercentage;
            public CacheIdentityPairArray<float> TimerVariants;
        }

#endregion

#region Fields

        [SerializeField] private int                      _volumeSteps = 5;
        [SerializeField] private AssetReferenceGameObject _volumeSourceAssetReference;
        [SerializeField] private float2                   _volumeRange;
        [SerializeField] private AssetReferenceAudioMixer _audioMixerAssetReference;
        
        [Space(10)]
        [SerializeField] private CacheIdentityPairArray<GameplayPreset>           _gameplayPresets;
        [SerializeField] private CacheIdentityPairArray<AssetReferenceGameObject> _userInterfaces;
        [SerializeField] private SessionPreferences                               _sessionSettings;

#endregion

#region Properties

        public int                      VolumeSteps                => _volumeSteps;
        public AssetReferenceGameObject VolumeSourceAssetReference => _volumeSourceAssetReference;
        public float2                   VolumeRange                => _volumeRange;
        public AssetReferenceAudioMixer AudioMixerAssetReference   => _audioMixerAssetReference;

        public IReadOnlyDictionary<string, GameplayPreset>           GameplayPresets => _gameplayPresets.Values;
        public IReadOnlyDictionary<string, AssetReferenceGameObject> UserInterfaces  => _userInterfaces.Values;
        public SessionPreferences                                    SessionSettings => _sessionSettings;

#endregion

#region Public API

        public void CacheEverything()
        {
            _gameplayPresets?.Cache();
            _userInterfaces?.Cache();
            
            _sessionSettings?.TimerVariants?.Cache();
        }

#endregion
    }
}
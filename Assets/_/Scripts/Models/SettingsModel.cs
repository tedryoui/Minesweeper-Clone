using System;
using _.Scripts.Services;
using UnityEngine;

namespace _.Scripts.Models
{
    [Serializable]
    public class SettingsModel : DataService.IModel
    {
        [SerializeField] private float _volumePercentage;
        
        public float VolumePercentage
        {
            get => _volumePercentage;
            set => _volumePercentage = value;
        }

        public SettingsModel()
        {
            _volumePercentage = 1f;
        }
    }
}
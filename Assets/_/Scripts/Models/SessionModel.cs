using System;
using _.Scripts.Services;
using Unity.Mathematics;
using UnityEngine;

namespace _.Scripts.Models
{
    [Serializable]
    public class SessionModel : DataService.IModel
    {
        [SerializeField] private int   _width;
        [SerializeField] private int   _height;
        [SerializeField] private int   _mineCount;
        [SerializeField] private float _timerSeconds;
        [SerializeField] private float _currentTimerSeconds;
        [SerializeField] private int   _steps;

        public int   Width
        {
            get => _width;
            set => _width = value;
        }

        public int   Height
        {
            get => _height;
            set => _height = value;
        }

        public int   MineCount
        {
            get => _mineCount;
            set => _mineCount = value;
        }

        public float TimerSeconds
        {
            get => _timerSeconds;
            set => _timerSeconds = value;
        }

        public float CurrentTimerSeconds
        {
            get => _currentTimerSeconds;
            set => _currentTimerSeconds = value;
        }

        public int Steps
        {
            get => _steps;
            set => _steps = value;
        }

        public SessionModel()
        {
            _width               = 0;
            _height              = 0;
            _mineCount           = 0;
            _timerSeconds        = 0;
            _currentTimerSeconds = 0;
            _steps               = 0;
        }
    }
}
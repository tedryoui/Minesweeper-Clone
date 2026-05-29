using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace _.Scripts.Structures
{
    public class Timer
    {
        private float _finalValue;
        private float _currentValue;
        private bool  _isPaused;

        private event Action _onTimerCompleted;
        private event Action _onTimerStopped;

        public float CurrentValue
        {
            get => _currentValue;
            set
            {
                var nextValue = math.clamp(value, 0f, _finalValue);
                
                _currentValue = nextValue;
            }
        }

        public event Action OnTimerCompleted
        {
            add => _onTimerCompleted += value;
            remove => _onTimerCompleted -= value;
        }

        public event Action OnTimerStopped
        {
            add => _onTimerStopped += value;
            remove => _onTimerStopped -= value;
        }

        public Timer(float finalValue)
        {
            _finalValue   = finalValue;
            _currentValue = 0f;
        }

        public Timer(float currentValue, float finalValue)
        {
            _finalValue   = finalValue;
            _currentValue = currentValue;
        }

        public async void Run(CancellationToken cancellationToken = default)
        {
            _isPaused = false;
            
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Yield();
                
                if (_isPaused)
                    continue;

                _currentValue += Time.deltaTime;

                if (_currentValue >= _finalValue)
                    break;
            }

            if (cancellationToken.IsCancellationRequested)
                _onTimerStopped?.Invoke();
            else
                _onTimerCompleted?.Invoke();
        }

        public void Pause(bool value = true)
        {
            _isPaused = value;
        }
    }
}
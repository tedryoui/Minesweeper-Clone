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

        private event Action onTimerCompleted;
        private event Action onTimerStopped;

        public float CurrentValue
        {
            get => _currentValue;
            set
            {
                var nextValue = math.clamp(value, 0f, _finalValue);
                
                _currentValue = nextValue;
            }
        }

        public Timer(float finalValue)
        {
            _finalValue = finalValue;
            _currentValue = 0f;
        }

        public Timer(float currentValue, float finalValue)
        {
            _finalValue = finalValue;
            _currentValue = currentValue;
        }

        public async void Run(CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Yield();

                _currentValue += Time.deltaTime;

                if (_currentValue >= _finalValue)
                    break;
            }

            if (cancellationToken.IsCancellationRequested)
                onTimerStopped?.Invoke();
            else
                onTimerCompleted?.Invoke();
        }
    }
}
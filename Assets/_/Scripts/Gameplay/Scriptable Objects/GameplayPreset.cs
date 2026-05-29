using _.Scripts.Attributes;
using UnityEngine;

namespace _.Scripts.Gameplay.Scriiptable_Objects
{
    [CreateAssetMenu(fileName = "Gameplay Preset", menuName = "Minesweeper/Gameplay/Gameplay Preset", order = 0)]
    public class GameplayPreset : ScriptableObject
    {
        [SerializeField] private int _width;
        [SerializeField] private int _height;
        [SerializeField] private int _mineCount;
        
        [TimeField(TimeFieldAttribute.TimeFieldType.Minutes, TimeFieldAttribute.TimeFieldType.Seconds)]
        [SerializeField] private float _timerSeconds;

        public int Width
        {
            get => _width;
            set => _width = value;
        }

        public int Height
        {
            get => _height;
            set => _height = value;
        }

        public int MineCount
        {
            get => _mineCount;
            set => _mineCount = value;
        }

        public float TimerSeconds
        {
            get => _timerSeconds;
            set => _timerSeconds = value;
        }
    }
}
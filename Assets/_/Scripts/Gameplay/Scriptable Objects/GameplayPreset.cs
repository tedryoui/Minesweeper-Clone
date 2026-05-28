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
        [SerializeField] private int _timerSeconds;
    }
}
using UnityEngine;

namespace _.Scripts.Gameplay.Scriiptable_Objects
{
    [CreateAssetMenu(fileName = "Gameplay Preset", menuName = "Minesweeper/Gameplay/Gameplay Preset", order = 0)]
    public class GameplayPreset : ScriptableObject
    {
        public int width;
        public int height;
        public int mineCount;
        public float timerSeconds;
    }
}
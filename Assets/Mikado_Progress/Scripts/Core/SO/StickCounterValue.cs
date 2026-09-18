using UnityEngine;

namespace Mikado.Core
{
    [CreateAssetMenu(fileName = "StickCounterValue", menuName = "Scriptable Objects/StickCounterValue")]
    public class StickCounterValue : ScriptableObject
    {
        public int IncrementalValue;
        public int CurrentStickCount;
        public int MinStickCount;
        public int MaxStickCount;

        public int GetStickCount()
        {
            return CurrentStickCount;
        }
        public int GetMinStickCount()
        {
            return MinStickCount;
        }
        public int GetMaxStickCount()
        {
            return MaxStickCount;
        }
        public void SetStickCount(int value)
        {
            CurrentStickCount = value;
        }

        public void IncrementCurrentStickCount()
        {
            if(CurrentStickCount >= MaxStickCount) IncrementMaxStickCount();

            CurrentStickCount += IncrementalValue;
        }

        private void IncrementMaxStickCount()
        {
            MaxStickCount += IncrementalValue;
        }
    }
}
using UnityEngine;
using UnityEngine.Serialization;

namespace Mikado.Core
{
    [CreateAssetMenu(fileName = "StickCounterValue", menuName = "Scriptable Objects/StickCounterValue")]
    public class StickCounterValue : ScriptableObject
    {
        [FormerlySerializedAs("IncrementalValue")]
        [SerializeField] private int incrementalValue;
        [FormerlySerializedAs("CurrentStickCount")]
        [SerializeField] private int currentStickCount;
        [FormerlySerializedAs("MinStickCount")]
        [SerializeField] private int minStickCount;
        [FormerlySerializedAs("MaxStickCount")]
        [SerializeField] private int maxStickCount;

        public int IncrementalValue => incrementalValue;
        public int CurrentStickCount => currentStickCount;
        public int MinStickCount => minStickCount;
        public int MaxStickCount => maxStickCount;

        public int GetStickCount()
        {
            return currentStickCount;
        }
        public int GetMinStickCount()
        {
            return minStickCount;
        }
        public int GetMaxStickCount()
        {
            return maxStickCount;
        }
        public void SetStickCount(int value)
        {
            currentStickCount = value;
        }

        public void IncrementCurrentStickCount()
        {
            if (currentStickCount >= maxStickCount) IncrementMaxStickCount();

            currentStickCount += incrementalValue;
        }

        private void IncrementMaxStickCount()
        {
            maxStickCount += incrementalValue;
        }
    }
}
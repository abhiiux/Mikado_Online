using UnityEngine;

namespace Mikado.Core
{
    [CreateAssetMenu(fileName = "StickCounterValue", menuName = "Scriptable Objects/StickCounterValue")]
    public class StickCounterValue : ScriptableObject
    {
        public int stickCount;

        public int GetStickCount()
        {
            return stickCount;
        }

        public void SetStickCount(int value)
        {
            stickCount = value;
        }
    }
}
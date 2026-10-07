using System;
using UnityEngine;

namespace Mikado.Core
{
    [CreateAssetMenu(fileName = "LivesCounterValue", menuName = "Scriptable Objects/LivesCounterValue")]
    public class LivesCounterValue : ScriptableObject
    {
        [SerializeField, Min(1)] private int maxLives = 3;

        [NonSerialized] private int currentLives;

        public int MaxLives => maxLives;
        public int CurrentLives => currentLives;
        public bool IsOutOfLives => currentLives <= 0;

        private void OnEnable() => ResetLives();

        public void ResetLives() => currentLives = maxLives;

        public void LoseLife()
        {
            if (currentLives <= 0) return;
            currentLives--;
        }
    }
}
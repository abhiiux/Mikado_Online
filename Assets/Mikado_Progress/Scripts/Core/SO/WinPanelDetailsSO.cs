using UnityEngine;

namespace Mikado.Data
{
    [CreateAssetMenu(fileName = "WinPanelDetailsSO", menuName = "Scriptable Objects/WinPanelDetailsSO")]
    public class WinPanelDetailsSO : ScriptableObject
    {
        [SerializeField] private int failedPickUp;
        [SerializeField] public int stickSelected;

        public int FailedPickUp => failedPickUp;
        public int StickSelected => stickSelected;

        private float levelStartTime;

        public void UpdateStartTime()
        {
            levelStartTime = Time.time;
        }
        public void UpdatePickup()
        {
            failedPickUp++;
        }
        public void UpdateStickSelection()
        {
            stickSelected++;
        }

        public float GetTimeSpendInSec()
        {
            return Time.time - levelStartTime;
        }

        public string GetTimeSpend()
        {
            float time = GetTimeSpendInSec();

            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);

            return $"{minutes:00}:{seconds:00}";
        }
    }
}
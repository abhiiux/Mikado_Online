using TMPro;
using UnityEngine;
using Mikado.Core;

namespace Mikado.UI
{
    public class GameplayUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text stickCounterUIField;
        [SerializeField] private StickCounterValue counterSO;

        private int targetCount;
        private int currentCount;

        void OnEnable()
        {
            GameEventBus.OnTargetCollisionDetected += HandleCounterIncrement;
        }
        void OnDisable()
        {
            GameEventBus.OnTargetCollisionDetected -= HandleCounterIncrement;
        }
        void Start()
        {
            targetCount = counterSO.CurrentStickCount;
            UpdateCounterUI();
        }

        private void HandleCounterIncrement(GameObject gameObject)
        {
            currentCount++;

            UpdateCounterUI();

            if (currentCount >= targetCount)
            {
                // Open Win Panel
                Debug.Log($" hi u win");
                GameEventBus.TriggerLevelWon();
            }
        }
        private void UpdateCounterUI()
        {
            if(stickCounterUIField == null) return;

            stickCounterUIField.text = $"{currentCount}/{targetCount}";
        }
    }
}
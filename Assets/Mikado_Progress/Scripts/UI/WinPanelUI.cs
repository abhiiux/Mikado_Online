using Mikado.Core;
using Mikado.Data;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Mikado.UI
{
    public class WinPanelUI : MonoBehaviour
    {
        [Header(" Panel Details Ref")]
        [SerializeField] private TMP_Text pickUpFail;
        [SerializeField] private TMP_Text stickSelected;
        [SerializeField] private TMP_Text timePlayed;
        [SerializeField] private CanvasGroup winPanel;

        [Header(" Panel Data ")]
        [SerializeField] private WinPanelDetailsSO panelData;
        [SerializeField] private StickCounterValue counterData;

        [Header(" NextLevel Button ")]
        [SerializeField] private Button nextLevelButton;

        [Header(" Navigation ")]
        [SerializeField] private UnityEvent pauseButton;
        [SerializeField] private UnityEvent restartButton;

        void OnEnable()
        {
            GameEventBus.OnLevelWon += HandleWinPanelOpen;
            GameEventBus.OnLevelWon += IncrementStickCounter;
            nextLevelButton.onClick.AddListener( NextLevel );
        }
        void OnDisable()
        {
            GameEventBus.OnLevelWon -= HandleWinPanelOpen;
            GameEventBus.OnLevelWon -= IncrementStickCounter;
            nextLevelButton.onClick.RemoveListener( NextLevel );
        }
        void Awake()
        {
            winPanel.alpha = 0;
        }
        private void HandleWinPanelOpen()
        {
            UpdatePanelDetails();

            winPanel.alpha = 1;          //Shows Panel

            pauseButton.Invoke();
        }

        private void UpdatePanelDetails()
        {
            if( panelData == null) return;

            pickUpFail.text = panelData.FailedPickUp.ToString();
            stickSelected.text = panelData.StickSelected.ToString();
            timePlayed.text = panelData.GetTimeSpend();
        }

        private void NextLevel()
        {
            pauseButton.Invoke(); //UnPause
            restartButton.Invoke();
        }
        private void IncrementStickCounter()
        {
            counterData.IncrementCurrentStickCount();
        }
    }
}

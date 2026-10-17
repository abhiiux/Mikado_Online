using Mikado.Core;
using Mikado.Data;
using Mikado.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Mikado.UI
{
    public class WinPanelUI : MonoBehaviour
    {
        [Header(" Panel Details Ref")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text pickUpFail;
        [SerializeField] private TMP_Text stickSelected;
        [SerializeField] private TMP_Text timePlayed;
        [SerializeField] private CanvasGroup winPanel;

        [Header(" Panel Data ")]
        [SerializeField] private string winTitle = "You Win!";
        [SerializeField] private string gameOverTitle = "Try Again";
        [SerializeField] private WinPanelDetailsSO panelData;
        [SerializeField] private StickCounterValue counterData;

        [Header(" UI Buttons ")]
        [SerializeField] private Button nextLevelButton;

        [Header(" Navigation ")]
        [SerializeField] private UnityEvent pauseButton;
        [SerializeField] private UnityEvent restartButton;

        private bool _isWin;

        void OnEnable()
        {
            GameEventBus.OnLevelWon += HandleWinPanelOpen;
            GameEventBus.OnGameOver += HandleGameOverPanelOpen;
            GameEventBus.OnLevelWon += IncrementStickCounter;
            nextLevelButton.onClick.AddListener( PrimaryButton );
        }
        void OnDisable()
        {
            GameEventBus.OnLevelWon -= HandleWinPanelOpen;
            GameEventBus.OnGameOver -= HandleGameOverPanelOpen;
            GameEventBus.OnLevelWon -= IncrementStickCounter;
            nextLevelButton.onClick.RemoveListener( PrimaryButton );
        }
        void Awake()
        {
            // Invisible must also mean non-blocking: CanvasGroup.blocksRaycasts
            // is independent of alpha, so without this the hidden panel
            // (rendered above the corner menu) swallows every click.
            SetPanelVisible( winPanel, false);
        }
        private void HandleWinPanelOpen()
        {
            _isWin = true;
            titleText.text = winTitle;
            UpdatePanelDetails();

            SetPanelVisible( winPanel, true);

            pauseButton.Invoke();
        }
        private void HandleGameOverPanelOpen()
        {
            _isWin = false;
            titleText.text = gameOverTitle;
            UpdatePanelDetails();

            SetPanelVisible( winPanel, true);

            pauseButton.Invoke();
        }

        private void UpdatePanelDetails()
        {
            if( panelData == null) return;

            pickUpFail.text = panelData.FailedPickUp.ToString();
            stickSelected.text = panelData.StickSelected.ToString();
            timePlayed.text = panelData.GetTimeSpend();
        }

        private void PrimaryButton()
        {
            pauseButton.Invoke(); //UnPause

            if( _isWin)
            {
                int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
                if( nextIndex >= SceneManager.sceneCountInBuildSettings)
                {
                    // Last scene in build order: fall back to the main menu.
                    SceneLoader.Instance.LoadScene( 0);
                }
                else
                {
                    SceneLoader.Instance.LoadNextScene();
                }
            }
            else
            {
                restartButton.Invoke();
            }
        }
        private void IncrementStickCounter()
        {
            counterData.IncrementCurrentStickCount();
        }
        private void SetPanelVisible(CanvasGroup panel, bool visible)
        {
            panel.alpha = visible ? 1f : 0f;
            panel.blocksRaycasts = visible;
            panel.interactable = visible;
        }
    }
}

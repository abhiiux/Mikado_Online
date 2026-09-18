using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mikado.Core;
using UnityEngine.SceneManagement;

namespace Mikado.Presentation
{
    public class StickCountComponent : MonoBehaviour
    {
        [SerializeField] private Button upButton;
        [SerializeField] private Button downButton;
        [SerializeField] private TMP_Text counterTextField;
        [SerializeField] private StickCounterValue counterData;

        private int valueAdded;
        private int stickCounter;
        private int minStickCounter;
        private int maxStickCounter;

        void OnEnable()
        {
            upButton.onClick.AddListener( DecreaseCounter );
            downButton.onClick.AddListener( AddCounter ); 
        }

        void OnDisable()
        {
            upButton.onClick.RemoveListener( DecreaseCounter );
            downButton.onClick.RemoveListener( AddCounter ); 
        }

        void Start()
        {
            minStickCounter = counterData.MinStickCount;
            maxStickCounter = counterData.MaxStickCount;
            valueAdded = counterData.IncrementalValue;

            stickCounter =  counterData.CurrentStickCount == 0 ? minStickCounter : counterData.MaxStickCount;
            UpdateUI();
        }
        
        public void ChangeScene()
        {
            AddValueToSo();

            SceneLoader.Instance.LoadNextScene();
        }
        
        private void AddValueToSo()
        {
            counterData.SetStickCount( stickCounter ); 
        }
        private void AddCounter()
        {
            stickCounter += valueAdded;

            UpdateUI();
        }
        private void DecreaseCounter()
        {
            stickCounter -= valueAdded;

            UpdateUI();
        }
        private void UpdateUI()
        {
            CheckCounter();

            counterTextField.text = stickCounter.ToString();        
        }
        private void UpdateButtonState(Button button, bool state)
        {
            if( button.interactable == state ) return;

            button.interactable = state;
        }
        private void CheckCounter()
        {
            int minStickCount = counterData.GetMinStickCount();
            int maxStickCount = counterData.GetMaxStickCount();

            if (stickCounter == minStickCount)
                UpdateButtonState(upButton, false);
            else if (stickCounter == maxStickCount)
                UpdateButtonState(downButton, false);
            else
            {
                UpdateButtonState(upButton, true);
                UpdateButtonState(downButton, true);
            }
        }
    }
}
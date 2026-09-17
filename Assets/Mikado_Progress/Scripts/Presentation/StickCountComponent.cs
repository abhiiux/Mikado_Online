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
        [SerializeField] private int valueAdded;
        [SerializeField] private StickCounterValue count;

        private int stickCounter;
        private readonly int minStickCounter = 5;
        private readonly int maxStickCounter = 50;

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
            stickCounter = minStickCounter;
            UpdateUI();
        }
        
        public void ChangeScene()
        {
            AddValueToSo();

            SceneLoader.Instance.LoadNextScene();
        }
        
        private void AddValueToSo()
        {
            count.SetStickCount( stickCounter ); 
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
            switch (stickCounter)
            {
                case 5:
                    UpdateButtonState(upButton, false);
                break;

                case 50:
                    UpdateButtonState(downButton, false);
                break;

                default:
                    UpdateButtonState(upButton, true);
                    UpdateButtonState(downButton, true);
                break;
            }
        }
    }
}
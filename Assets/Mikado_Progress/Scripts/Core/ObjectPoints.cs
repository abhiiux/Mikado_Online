using UnityEngine;

namespace Mikado.Core
{
    public class ObjectPoints : MonoBehaviour
    {
        public enum SticksState
        {
            Active,
            Selected,
            Disable
        }

        [SerializeField] private int _points;
        public string nameStick;
        [HideInInspector] public bool isFlagged = false;
        [HideInInspector] public bool isTarget = false;

        public SticksState currentState = SticksState.Active;//needs update
        private Rigidbody rb;
        private Renderer ownRenderer;

        public int Points
        {
            get
            {
                return _points;
            }
            set
            {
                _points = value;
            }
        }

        void OnEnable()
        {
            rb = GetComponent<Rigidbody>();
        }
        public void Init()
        {
            rb = GetComponent<Rigidbody>();
            ownRenderer = GetComponent<Renderer>();
        }
        public void SetSelection(bool value)
        {
            if(value)
            {
                UpdateState(SticksState.Selected);
            }
            else
            {
                UpdateState(SticksState.Active);
            }
        }
        private void UpdateState(SticksState newState)
        {
            if(newState == currentState) return;

            currentState = newState;

            switch (newState)
            {
                case SticksState.Active:
                    // rb.useGravity = true; 
                    isTarget = false;

                    ToggleSelectionVisual( false );
                break;                

                case SticksState.Selected:
                    // rb.useGravity = false;
                    isTarget = true;

                    ToggleSelectionVisual( true );
                break;         

                case SticksState.Disable:
                    if(gameObject.activeSelf)
                     gameObject.SetActive(false);
                break;                
            }
        }

        private void ToggleSelectionVisual(bool state)
        {
            GameEventBus.TriggerSelection( ownRenderer, state );
        }
        public void ToggleRedVisual()
        {
            GameEventBus.TriggerMovementDetection( ownRenderer );
        }

    }
}

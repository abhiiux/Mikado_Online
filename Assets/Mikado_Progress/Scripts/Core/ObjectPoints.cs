using UnityEngine;

namespace Mikado.Core
{
    public class ObjectPoints : MonoBehaviour
    {
        public enum sticksState
        {
            Active,
            Selected,
            Disable
        }

        [SerializeField] private int _points;
        public string nameStick;
        [HideInInspector] public bool isFlagged = false;
        [HideInInspector] public bool isTarget = false;

        public sticksState currentState = sticksState.Active;//needs update
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
                UpdateState(sticksState.Selected);
                Debug.Log($" object set to Selected state ");
            }
            else
            {
                UpdateState(sticksState.Active);
                Debug.Log($" object set to Un-Select state ");
            }
        }
        private void UpdateState(sticksState newState)
        {
            if(newState == currentState) return;

            currentState = newState;

            switch (newState)
            {
                case sticksState.Active:
                    rb.useGravity = true;
                    isTarget = false;

                    ToggleSelectionVisual( false );
                break;                

                case sticksState.Selected:
                    rb.useGravity = false;
                    isTarget = true;

                    ToggleSelectionVisual( true );
                break;         

                case sticksState.Disable:
                    //
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

using Mikado.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Mikado.Gameplay
{
    public class PickUpController : MonoBehaviour
    {
        [SerializeField] private InputActionReference clickAction;
        [SerializeField] private float forceMagnitude;
        [SerializeField] private Transform forceDirection;
        [SerializeField] private Button mobilePickUpButton;

        private Transform pickUpObject;

        void OnEnable()
        {
            clickAction.action.Enable();

            clickAction.action.performed += HandlePickUp;
            mobilePickUpButton.onClick.AddListener ( ApplyUpWardForce );
            GameEventBus.OnTargetChange += HandleTargetChange;
        }
        void OnDisable()
        {
            clickAction.action.performed -= HandlePickUp;
            mobilePickUpButton.onClick.RemoveListener ( ApplyUpWardForce );
            GameEventBus.OnTargetChange -= HandleTargetChange;
        }

        private void HandleTargetChange(Transform newTarget)
        {
            // null means "nothing selected" (deselect, cancelled pickup, collected, etc.) —
            // clear the cache too, or HandlePickUp keeps applying force to a stick that's
            // no longer actually selected.
            pickUpObject = newTarget;
            
        }
        private void HandlePickUp(InputAction.CallbackContext context)
        {
            ApplyUpWardForce();
        }

        private void ApplyUpWardForce()
        {
            if ( pickUpObject == null) return;
            Rigidbody rb =  pickUpObject.GetComponent<Rigidbody>();
            rb.useGravity = false;
            
            Vector3 impulseVector = forceDirection.position * forceMagnitude;

            rb.AddForce(impulseVector, ForceMode.Force);
        }
    }
}
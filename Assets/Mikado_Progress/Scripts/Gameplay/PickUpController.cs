using Mikado.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Mikado.Gameplay
{
    public class PickUpController : MonoBehaviour
    {
        [SerializeField] private InputActionReference clickAction;
        [SerializeField] private float forceMagnitude;
        [SerializeField] private Transform forceDirection;

        private Transform pickUpObject;

        void OnEnable()
        {
            clickAction.action.Enable();

            clickAction.action.performed += HandlePickUp;
            GameEventBus.OnTargetChange += HandleTargetChange;
        }
        void OnDisable()
        {
            clickAction.action.performed -= HandlePickUp;
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
            if ( pickUpObject == null) return;
            Rigidbody rb =  pickUpObject.GetComponent<Rigidbody>();
            rb.useGravity = false;
            
            Vector3 impulseVector = forceDirection.position * forceMagnitude;

            rb.AddForce(impulseVector, ForceMode.Force);
        }
    }
}
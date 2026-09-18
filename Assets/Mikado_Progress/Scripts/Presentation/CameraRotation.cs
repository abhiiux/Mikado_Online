using DG.Tweening;
using Mikado.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Mikado.Presentation
{
    public class CameraRotation : MonoBehaviour
    {
        [SerializeField] private Transform defaultTarget;
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private float heightSpeed = 5f;
        [SerializeField] private float orbitRadius = 17f;
        [SerializeField] private float minOrbitRadius = 2f;
        [SerializeField] private float maxOrbitRadius = 17f;
        [SerializeField] private float heightOffset = 5f;
        [SerializeField] private float targetChangeDuration = 0.5f;
        [SerializeField] private float movementSmoothTime = 0.1f;

        [SerializeField] private InputActionReference camRotationControls;
        [SerializeField] private InputActionReference camZoomControls;

        private bool isMoving;
        private float currentAngle = 0f;

        private Transform currentTarget;
        private Vector3 lookTarget;
        private Vector3 cameraVelocity;

        private Tween targetTween;

        private Vector2 inputDirection;
        private float zoomDirection;

        private void OnEnable()
        {
            camRotationControls.action.Enable();
            camZoomControls.action.Enable();

            camRotationControls.action.performed += OnRotationInput;
            camRotationControls.action.canceled += OnRotationInput;

            camZoomControls.action.performed += OnZoomInput;
            camZoomControls.action.canceled += OnZoomInput;

            GameEventBus.OnTargetChange += HandleTargetChange;
        }

        private void OnDisable()
        {
            camRotationControls.action.performed -= OnRotationInput;
            camRotationControls.action.canceled -= OnRotationInput;

            camZoomControls.action.performed -= OnZoomInput;
            camZoomControls.action.canceled -= OnZoomInput;

            GameEventBus.OnTargetChange -= HandleTargetChange;

            targetTween?.Kill();
        }

        private void Start()
        {
            currentTarget = defaultTarget;
            lookTarget = currentTarget.position;

            heightOffset = Mathf.Clamp(heightOffset, 1f, 8f);
            orbitRadius = Mathf.Clamp(
                orbitRadius,
                minOrbitRadius,
                maxOrbitRadius
            );

            UpdateCameraPosition();
        }

        private void Update()
        {
            if (inputDirection != Vector2.zero)
            {
                currentAngle += inputDirection.x * rotationSpeed * Time.deltaTime;

                heightOffset = Mathf.Clamp(
                    heightOffset + inputDirection.y * heightSpeed * Time.deltaTime,
                    1f,
                    8f
                );
            }

            if (zoomDirection != 0f)
            {
                orbitRadius = Mathf.Clamp(
                    orbitRadius - zoomDirection * zoomSpeed * Time.deltaTime,
                    minOrbitRadius,
                    maxOrbitRadius
                );
            }

            UpdateCameraPosition();
        }

        private void OnRotationInput(InputAction.CallbackContext context)
        {
            inputDirection = context.ReadValue<Vector2>();
            isMoving = inputDirection != Vector2.zero;
        }

        private void OnZoomInput(InputAction.CallbackContext context)
        {
            zoomDirection = context.ReadValue<float>();
        }

        private void UpdateCameraPosition()
        {
            float radians = currentAngle * Mathf.Deg2Rad;

            float x = Mathf.Sin(radians) * orbitRadius;
            float z = Mathf.Cos(radians) * orbitRadius;

            Vector3 targetPosition = lookTarget + new Vector3(
                x,
                heightOffset,
                z
            );

            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref cameraVelocity,
                movementSmoothTime
            );

            transform.LookAt(lookTarget);
        }

        private void HandleTargetChange(Transform newTarget)
        {
            if (newTarget == null)
            {
                newTarget = defaultTarget;
            }

            ChangeTargetLook(newTarget);
        }

        private void ChangeTargetLook(Transform newTarget)
        {
            targetTween?.Kill();

            targetTween = DOTween.To(
                () => lookTarget,
                value =>
                {
                    lookTarget = value;
                    currentTarget = newTarget;

                    UpdateCameraPosition();
                },
                newTarget.position,
                targetChangeDuration
            ).SetEase(Ease.InOutQuad);
        }

        public bool IsCamMoving()
        {
            return isMoving;
        }
    }
}
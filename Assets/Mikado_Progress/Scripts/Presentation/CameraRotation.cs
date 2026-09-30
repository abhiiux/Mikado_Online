using DG.Tweening;
using Mikado;
using Mikado.Core;
using Mikado.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

        [Header("Keyboard / Gamepad")]
        [SerializeField] private InputActionReference camRotationControls;
        [SerializeField] private InputActionReference camZoomControls;

        [Header("Zoom Slider (UI)")]
        [Tooltip("Slider left/bottom = zoomed out, right/top = zoomed in.")]
        [SerializeField] private Slider zoomSlider;
        [SerializeField] private Button pickUpButton;

        [Header("Settings")]
        [SerializeField] private SettingsDataSO settingsData;

        private bool isMoving;
        private bool lookOnlyActive;
        private float currentAngle;

        // Runtime orbit bounds. Widen when a look-only rebase lands the camera
        // outside the design range, so the first input never snaps the camera.
        private float boundMinRadius;
        private float boundMaxRadius;
        private float boundMinHeight = 1f;
        private float boundMaxHeight = 8f;

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

            if (zoomSlider != null)
            {
                zoomSlider.onValueChanged.AddListener(OnZoomSliderChanged);
            }

            GameEventBus.OnTargetChange += HandleTargetChange;
        }

        private void OnDisable()
        {
            camRotationControls.action.performed -= OnRotationInput;
            camRotationControls.action.canceled -= OnRotationInput;

            camZoomControls.action.performed -= OnZoomInput;
            camZoomControls.action.canceled -= OnZoomInput;

            if (zoomSlider != null)
            {
                zoomSlider.onValueChanged.RemoveListener(OnZoomSliderChanged);
            }

            GameEventBus.OnTargetChange -= HandleTargetChange;

            targetTween?.Kill();
            lookOnlyActive = false;
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

            boundMinRadius = minOrbitRadius;
            boundMaxRadius = maxOrbitRadius;
            boundMinHeight = 1f;
            boundMaxHeight = 8f;

            if (zoomSlider != null)
            {
                // Slider is always normalized 0..1
                zoomSlider.minValue = 0f;
                zoomSlider.maxValue = 1f;
                zoomSlider.wholeNumbers = false;
            }

            RefreshSliderVisibility();
            RefreshPickUpButtonVisibility();
            UpdateCameraPosition();
        }

        private void Update()
        {
            float rotSens = settingsData ? settingsData.rotationSensitivity : 1f;
            float heightSens = settingsData ? settingsData.heightSensitivity : 1f;
            float zoomSens = settingsData ? settingsData.zoomSensitivity : 1f;

            // Mobile input toggle: if off and current input comes from Touchscreen, ignore it.
            // You handle hiding the slider GameObject in UI; this stops touch from still rotating/zooming.
            Vector2 effectiveDirection = inputDirection;
            float effectiveZoom = zoomDirection;
            if (settingsData != null && !settingsData.mobileInputEnabled && IsTouchDevice())
            {
                effectiveDirection = Vector2.zero;
                effectiveZoom = 0f;
            }

            // Hold orbit/zoom input while a look-only retarget is animating,
            // otherwise the completion rebase would clobber fresh input (and vice versa).
            if (lookOnlyActive)
            {
                effectiveDirection = Vector2.zero;
                effectiveZoom = 0f;
            }

            // Keyboard / gamepad rotation
            if (effectiveDirection != Vector2.zero)
            {
                currentAngle +=
                    effectiveDirection.x * rotationSpeed * rotSens * Time.deltaTime;

                heightOffset = Mathf.Clamp(
                    heightOffset +
                    effectiveDirection.y * heightSpeed * heightSens * Time.deltaTime,
                    boundMinHeight,
                    boundMaxHeight
                );
            }

            // Keyboard / gamepad zoom (handy for editor testing).
            // Delete this block if the slider should be the only zoom control.
            if (effectiveZoom != 0f)
            {
                ApplyZoom(
                    effectiveZoom * zoomSpeed * zoomSens * Time.deltaTime
                );
            }

            UpdateCameraPosition();
        }

        private bool IsTouchDevice()
        {
            // Active control's device is Touchscreen → input came from touch delta
            var active = camRotationControls != null ? camRotationControls.action.activeControl : null;
            if (active != null && active.device is UnityEngine.InputSystem.Touchscreen) return true;
            var zoomActive = camZoomControls != null ? camZoomControls.action.activeControl : null;
            if (zoomActive != null && zoomActive.device is UnityEngine.InputSystem.Touchscreen) return true;
            return false;
        }

        private void RefreshSliderVisibility()
        {
            if (zoomSlider == null) return;
            bool show = settingsData ? settingsData.mobileInputEnabled : true;
            if (zoomSlider.gameObject.activeSelf != show)
                zoomSlider.gameObject.SetActive(show);
            if (show) SyncSliderToRadius();
        }
        private void RefreshPickUpButtonVisibility()
        {
            if(pickUpButton == null) return;

            bool show = settingsData ? settingsData.mobileInputEnabled : true;
            if (pickUpButton.gameObject.activeSelf != show)
                pickUpButton.gameObject.SetActive(show);
        }
        public void OnSettingsChanged() => RefreshSliderVisibility();

        #region Keyboard / Gamepad Input

        private void OnRotationInput(InputAction.CallbackContext context)
        {
            inputDirection = context.ReadValue<Vector2>();
            isMoving = inputDirection != Vector2.zero;
        }

        private void OnZoomInput(InputAction.CallbackContext context)
        {
            zoomDirection = context.ReadValue<float>();
        }

        #endregion

        #region Slider Zoom

        // Slider 0 = fully zoomed out (maxOrbitRadius)
        // Slider 1 = fully zoomed in  (minOrbitRadius)
        private void OnZoomSliderChanged(float value)
        {
            orbitRadius = Mathf.Lerp(maxOrbitRadius, minOrbitRadius, value);
        }

        private void SyncSliderToRadius()
        {
            if (zoomSlider == null) return;

            float t = Mathf.InverseLerp(maxOrbitRadius, minOrbitRadius, orbitRadius);

            // No-notify so this doesn't call OnZoomSliderChanged back at us
            zoomSlider.SetValueWithoutNotify(t);
        }

        #endregion

        private void ApplyZoom(float zoom)
        {
            orbitRadius = Mathf.Clamp(
                orbitRadius - zoom,
                boundMinRadius,
                boundMaxRadius
            );

            // Keep the slider in step when zooming via keyboard
            SyncSliderToRadius();
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

            // Look-only retarget: position is frozen; the tween sets it each tick.
            if (!lookOnlyActive)
            {
                transform.position = Vector3.SmoothDamp(
                    transform.position,
                    targetPosition,
                    ref cameraVelocity,
                    movementSmoothTime
                );
            }

            transform.LookAt(lookTarget);
        }

        #region Target Change

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

            bool moveCamera = settingsData == null || settingsData.moveCameraToTarget;

            if (moveCamera)
            {
                lookOnlyActive = false;

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

                return;
            }

            // Look-only: hold position, rotate toward the new target, then
            // silently rebase the orbit onto it (no jump on the next manual move).
            // Snapshot the target position at entry: the selected stick is a live
            // Rigidbody (PickUpController impulse), so re-reading newTarget.position
            // in OnComplete would bake mid-tween physics motion into the persistent
            // orbit params (heightOffset/orbitRadius jump + polluted bounds).
            Vector3 frozenPos = transform.position;
            Transform desiredTarget = newTarget;
            Vector3 desiredLook = newTarget.position;
            lookOnlyActive = true;

            targetTween = DOTween.To(
                () => lookTarget,
                value =>
                {
                    lookTarget = value;
                    currentTarget = desiredTarget;

                    transform.position = frozenPos;
                    transform.LookAt(lookTarget);
                },
                desiredLook,
                targetChangeDuration
            ).SetEase(Ease.InOutQuad).OnComplete(() =>
            {
                lookTarget = desiredLook;
                currentTarget = desiredTarget;

                Vector3 offset = frozenPos - lookTarget;

                // Exact rebase: params must reproduce frozenPos precisely, or the
                // next SmoothDamp would glide the camera. Widen bounds instead of
                // clamping, so later input keeps moving smoothly from here.
                heightOffset = offset.y;
                orbitRadius = new Vector2(offset.x, offset.z).magnitude;
                currentAngle = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;

                boundMinRadius = Mathf.Min(boundMinRadius, orbitRadius);
                boundMaxRadius = Mathf.Max(boundMaxRadius, orbitRadius);
                boundMinHeight = Mathf.Min(boundMinHeight, heightOffset);
                boundMaxHeight = Mathf.Max(boundMaxHeight, heightOffset);

                cameraVelocity = Vector3.zero;
                lookOnlyActive = false;

                SyncSliderToRadius();
                UpdateCameraPosition();
            });
        }

        #endregion

        public bool IsCamMoving()
        {
            return isMoving;
        }
    }
}
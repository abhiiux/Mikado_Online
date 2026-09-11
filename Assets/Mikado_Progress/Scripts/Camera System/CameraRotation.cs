using UnityEngine;
using UnityEngine.InputSystem;

public class CameraRotation : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] float zoomSpeed = 5f;
    [SerializeField] float heightSpeed = 5f;
    [SerializeField] float orbitRadius = 17f;
    [SerializeField] float minOrbitRadius = 2f;
    [SerializeField] float maxOrbitRadius = 17f;
    [SerializeField] private float heightOffset = 5f;
    [SerializeField] InputActionReference camRotationControls;
    [SerializeField] InputActionReference camZoomControls;

    private bool isMoving;
    private float currentAngle = 0f;
    public Vector2 inputDirection;
    public float zoomDirection;

    void OnEnable()
    {
        camRotationControls.action.Enable();
        camZoomControls.action.Enable();
        camRotationControls.action.performed += OnRotationInput;
        camRotationControls.action.canceled += OnRotationInput;
        camZoomControls.action.performed += OnZoomInput;
        camZoomControls.action.canceled += OnZoomInput;
    }

    void OnDisable()
    {
        camRotationControls.action.performed -= OnRotationInput;
        camRotationControls.action.canceled -= OnRotationInput;
        camZoomControls.action.performed -= OnZoomInput;   // was += (bug)
        camZoomControls.action.canceled -= OnZoomInput;    // was += (bug)
    }

    void Start()
    {
        heightOffset = Mathf.Clamp(heightOffset, 1f, 8f);
        orbitRadius = Mathf.Clamp(orbitRadius, minOrbitRadius, maxOrbitRadius);
        UpdateCameraPosition();
    }

    void Update()
    {
        bool needsUpdate = false;

        if (inputDirection != Vector2.zero)
        {
            currentAngle += inputDirection.x * rotationSpeed * Time.deltaTime;
            heightOffset = Mathf.Clamp(heightOffset + inputDirection.y * heightSpeed * Time.deltaTime, 1f, 8f);
            needsUpdate = true;
        }

        if (zoomDirection != 0f)
        {
            orbitRadius = Mathf.Clamp(
                orbitRadius - zoomDirection * zoomSpeed * Time.deltaTime,
                minOrbitRadius, maxOrbitRadius);
            needsUpdate = true;
        }

        if (needsUpdate)
        {
            UpdateCameraPosition();
        }
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
        float x = Mathf.Sin(currentAngle) * orbitRadius;
        float z = Mathf.Cos(currentAngle) * orbitRadius;

        transform.position = new Vector3(x, heightOffset, z) + target.position;
        transform.LookAt(target);
    }

    public bool isCamMoving()
    {
        return isMoving;
    }
}
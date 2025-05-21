using UnityEngine;
using UnityEngine.InputSystem;

public class CameraRotation : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] float orbitRadius = 5f;
    [SerializeField] float heightOffset = 2f;
    [SerializeField] InputActionReference keyboardInput;

    private float currentAngle = 0f;
    private Vector2 inputDirection;

    void OnEnable()
    {
        keyboardInput.action.Enable();
        keyboardInput.action.performed += OnKeyboardInput;
        keyboardInput.action.canceled += OnKeyboardInput;
    }

    void OnDisable()
    {
        keyboardInput.action.performed -= OnKeyboardInput;
        keyboardInput.action.canceled -= OnKeyboardInput;
    }

    void Start()
    {
        UpdateCameraPosition();
        transform.LookAt(target);
    }

    void Update()
    {
        if (inputDirection != Vector2.zero)
        {
            currentAngle += inputDirection.x * rotationSpeed * Time.deltaTime;
            UpdateCameraPosition();
        }
    }

    private void OnKeyboardInput(InputAction.CallbackContext context)
    {
        inputDirection = context.ReadValue<Vector2>();
    }

    private void UpdateCameraPosition()
    {
        float x = Mathf.Sin(currentAngle) * orbitRadius;
        float z = Mathf.Cos(currentAngle) * orbitRadius;
        
        Vector3 newPosition = new Vector3(x, heightOffset, z) + target.position;
        transform.position = newPosition;
        
        transform.LookAt(target);
    }
}

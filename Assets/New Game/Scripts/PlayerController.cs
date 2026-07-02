using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform ballPos;
    [SerializeField] private Transform alien;

    [Header("Alien Offset")]
    [SerializeField] Vector3 offset;

    [Header("Debug COM")]
    public Color gizmoColor = Color.red;
    public float sphereRadius = 0.1f;   
    public float gizmoOffset = 0.5f;

    [Header(" Ball Controll")]
    public float _movingTorque;
    public float _idleTorque;

    [Header(" Ball Controll")]
    [SerializeField] private InputAction onMove;

// ----------------------
    private Vector3 _moveInput;
    private float _ballTorque;

    void Reset()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        onMove.Enable();
        onMove.performed += OnMove;
        onMove.canceled += OnCancelled;
    }
    void OnDisable()
    {
        onMove.performed -= OnMove;
        onMove.canceled -= OnCancelled;
        onMove.Disable();
    }
    void OnDrawGizmos()
    {
        if (!rb)
            return;

        // DrawCenterOfMass();
        DrawMovementGizmo();
    }

    void FixedUpdate()
    {
        if(_moveInput.sqrMagnitude > 0.5f )
        {
            AddTorqueToObject(_moveInput);
        }
    }
    void LateUpdate()
    {
        if(ballPos != null)
        alien.position = ballPos.position + offset;
    }
    private void DrawCenterOfMass()
    {
        Vector3 worldCOM = transform.TransformPoint(rb.centerOfMass);

        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(worldCOM, sphereRadius);

        float size = sphereRadius * 3f;

        Gizmos.DrawLine(worldCOM - Vector3.right * size, worldCOM + Vector3.right * size);
        Gizmos.DrawLine(worldCOM - Vector3.up * size, worldCOM + Vector3.up * size);
        Gizmos.DrawLine(worldCOM - Vector3.forward * size, worldCOM + Vector3.forward * size);
    }

    private void DrawMovementGizmo()
    {
        if (_moveInput.sqrMagnitude < 0.001f)
            return;

        Vector3 worldCOM = rb.gameObject.transform.TransformPoint(rb.centerOfMass);
        Vector3 direction = _moveInput.normalized;
        Vector3 start = worldCOM + direction * gizmoOffset;
        float arrowLength = Mathf.Clamp(_moveInput.magnitude * 0.5f, 0.5f, 3f);
        Vector3 tip = start + direction * arrowLength;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(start, tip);
        Gizmos.DrawSphere(tip, 0.1f);
    }

    void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector3>();
        _ballTorque = _movingTorque;
    }

    void OnCancelled(InputAction.CallbackContext context)
    {
        _ballTorque = _idleTorque;
    }

    void AddTorqueToObject(Vector3 direction)
    {
        Vector3 torque = new(direction.z, 0f, -direction.x);

        rb.AddTorque(torque * _ballTorque);
        // Debug.Log($" adding torque :{torque}");
    }
}

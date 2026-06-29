using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;

    [Header("Debug COM")]
    public Color gizmoColor = Color.red;
    public float sphereRadius = 0.1f;

    [Header(" Ball Controll")]
    public float _ballTorque;

    [Header(" Ball Controll")]
    [SerializeField] private InputAction onMove;

    private Vector3 _moveInput;

    public float gizmoOffset = 0.5f;

    void Reset()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        onMove.Enable();
        onMove.performed += OnMove;
    }
    void OnDisable()
    {
        onMove.performed -= OnMove;
        onMove.Disable();
    }
    void OnDrawGizmos()
    {
        if (!rb)
            return;

        DrawCenterOfMass();
        DrawMovementGizmo();
    }

    void FixedUpdate()
    {
        if(_moveInput.sqrMagnitude > 0.1f)
        {
            AddTorqueToObject(_moveInput);
        }
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

        Vector3 worldCOM = transform.TransformPoint(rb.centerOfMass);
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
        Debug.Log($" the direction : {_moveInput}");
        AddTorqueToObject(direction: _moveInput);
    }

    void AddTorqueToObject(Vector3 direction)
    {
        Vector3 torque = new(direction.z, 0f, -direction.x);
        rb.AddTorque(torque * _ballTorque);
    }
}

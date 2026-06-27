using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class Respawner : MonoBehaviour
{
    public Transform _object;
    public Transform _spawnPoint;
    public InputAction onMove;

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

    void OnMove(InputAction.CallbackContext context)
    {
        SpawnObjectOnce(_object,_spawnPoint.position);
    }

    void SpawnObjectOnce(Transform obj, Vector3 spawnPoint)
    {
        obj.position = spawnPoint;
        obj.gameObject.SetActive(false);
        if (obj.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            obj.gameObject.SetActive(true);
        }
    }
}
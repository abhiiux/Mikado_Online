using UnityEngine;
using UnityEngine.InputSystem;

public class BalanceController : MonoBehaviour
{
    [SerializeField] private Rigidbody ballRb;

    [Header("Balance")]
    [SerializeField] private float maxBalance = 100f;
    [SerializeField] private float drainRate = 10f;
    [SerializeField] private float regenRate = 15f;
    [SerializeField] private float instabilityThreshold = 2f;
    [SerializeField] private float angularWeight = 0.5f;

    private float _currentBalance;
    private Vector3 _prevVelocity;
    
    public float BalancePercent => _currentBalance / maxBalance;

    void Start()
    {
        _currentBalance = maxBalance;
        _prevVelocity = ballRb.linearVelocity;
    }
    void Update()
    {
        if(_currentBalance > 0f)
        {
            Debug.Log($" {BalancePercent} ");
        }
        else if (_currentBalance <= 0f)
        {
            Debug.Log("Player lost balance!");
        }
    }
    void FixedUpdate()
    {
        Vector3 velocity = ballRb.linearVelocity;
        Vector3 acceleration = (velocity - _prevVelocity) / Time.fixedDeltaTime;
        _prevVelocity = velocity;

        float instability = acceleration.magnitude + ballRb.angularVelocity.magnitude * angularWeight;

        if (instability > instabilityThreshold)
        {
            _currentBalance -= drainRate * instability * Time.fixedDeltaTime;
        }
        else
        {
            _currentBalance += regenRate * Time.fixedDeltaTime;
        }

        _currentBalance = Mathf.Clamp(_currentBalance, 0f, maxBalance);

        if (_currentBalance <= 0f)
        {
            // _currentBalance = maxBalance;
            // _prevVelocity = ballRb.linearVelocity;
        }
    }
}
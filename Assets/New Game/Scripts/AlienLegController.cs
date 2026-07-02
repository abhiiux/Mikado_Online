using UnityEngine;

public class AlienLegController : MonoBehaviour
{
    [SerializeField] private Rigidbody ballRb;
    [SerializeField] private Transform leftFootTarget;
    [SerializeField] private Transform rightFootTarget;
    [SerializeField] private Transform hips;

    [SerializeField] private float stepFrequency = 2f;
    [SerializeField] private float stepHeight = 0.3f;
    [SerializeField] private float stepLength = 0.3f;
    [SerializeField] private float minSpeed = 0.1f;
    [SerializeField] private float idleSmoothTime = 5f;

    private Vector3 _leftIdlePos;
    private Vector3 _rightIdlePos;
    private float _stepProgress;

    void Start()
    {
        _leftIdlePos = leftFootTarget.localPosition;
        _rightIdlePos = rightFootTarget.localPosition;
    }

    void LateUpdate()
    {
        Vector3 velocity = ballRb.linearVelocity;
        float speed = new Vector2(velocity.x, velocity.z).magnitude;

        if (speed > minSpeed)
            AnimateWalking(speed);
        else
            ReturnToIdle();
    }

    void AnimateWalking(float speed)
    {
        Vector3 moveDir = new Vector3(ballRb.linearVelocity.x, 0f, ballRb.linearVelocity.z).normalized;
        _stepProgress += Time.deltaTime * stepFrequency * speed;

        float leftPhase = Mathf.Sin(_stepProgress);
        float rightPhase = Mathf.Sin(_stepProgress + Mathf.PI);

        Vector3 localMoveDir = hips.InverseTransformDirection(moveDir);

        leftFootTarget.localPosition = _leftIdlePos
            + localMoveDir * (leftPhase * stepLength)
            + Vector3.up * Mathf.Max(0f, leftPhase * stepHeight);

        rightFootTarget.localPosition = _rightIdlePos
            + localMoveDir * (rightPhase * stepLength)
            + Vector3.up * Mathf.Max(0f, rightPhase * stepHeight);
    }

    void ReturnToIdle()
    {
        leftFootTarget.localPosition = Vector3.Lerp(
            leftFootTarget.localPosition, _leftIdlePos, Time.deltaTime * idleSmoothTime);

        rightFootTarget.localPosition = Vector3.Lerp(
            rightFootTarget.localPosition, _rightIdlePos, Time.deltaTime * idleSmoothTime);

        _stepProgress = 0f;
    }
}

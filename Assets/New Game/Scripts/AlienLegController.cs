using UnityEngine;

public class AlienLegController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody ballRb;
    [SerializeField] private Transform leftFootTarget;
    [SerializeField] private Transform rightFootTarget;
    [SerializeField] private Transform hips;

    [Header("Walking")]
    [SerializeField] private float stepFrequency = 2f;
    [SerializeField] private float stepLength = 0.3f;
    [SerializeField] private float legSpacing = 0.3f;

    [Header("Sphere Motion (Optional)")]
    [SerializeField] private bool useSphereArcMotion = true;
    [SerializeField] private float ballRadius = 0.5f;

    [Header("Simple Motion")]
    [SerializeField] private float footLiftHeight = 0.1f;

    [Header("Idle")]
    [SerializeField] private float minSpeed = 0.1f;
    [SerializeField] private float idleSmoothTime = 5f;

    private Vector3 _leftIdlePosition;
    private Vector3 _rightIdlePosition;
    private float _stepProgress;

    private void Start()
    {
        CacheIdleFootPositions();
    }

    private void OnEnable()
    {
        _stepProgress = 0f;
        CacheIdleFootPositions();
    }

    private void LateUpdate()
    {
        float speed = GetHorizontalSpeed();

        if (speed > minSpeed)
        {
            AnimateWalking(speed);
            return;
        }

        ReturnFeetToIdle();
    }

    private void CacheIdleFootPositions()
    {
        _leftIdlePosition = leftFootTarget.localPosition;
        _rightIdlePosition = rightFootTarget.localPosition;
    }

    private float GetHorizontalSpeed()
    {
        Vector3 velocity = ballRb.linearVelocity;
        velocity.y = 0f;

        return velocity.magnitude;
    }

    private void AnimateWalking(float speed)
    {
        AdvanceStepCycle(speed);

        Vector3 moveDirection = GetLocalMovementDirection();

        Vector3 leftOffset = CalculateLeftFootOffset(moveDirection);
        Vector3 rightOffset = CalculateRightFootOffset(moveDirection);

        ApplyFootOffsets(leftOffset, rightOffset);
    }

    private void AdvanceStepCycle(float speed)
    {
        _stepProgress += Time.deltaTime * stepFrequency * speed;
    }

    private Vector3 GetLocalMovementDirection()
    {
        Vector3 velocity = ballRb.linearVelocity;
        velocity.y = 0f;

        return hips.InverseTransformDirection(velocity.normalized);
    }

    private Vector3 CalculateLeftFootOffset(Vector3 moveDirection)
    {
        return CalculateFootOffset(
            GetLeftPhase(),
            moveDirection,
            -hips.forward * legSpacing
        );
    }

    private Vector3 CalculateRightFootOffset(Vector3 moveDirection)
    {
        return CalculateFootOffset(
            GetRightPhase(),
            moveDirection,
            hips.forward * legSpacing
        );
    }

    private float GetLeftPhase()
    {
        return Mathf.Sin(_stepProgress);
    }

    private float GetRightPhase()
    {
        return Mathf.Sin(_stepProgress + Mathf.PI);
    }

    private Vector3 CalculateFootOffset(
        float phase,
        Vector3 moveDirection,
        Vector3 lateralOffset)
    {
        Vector3 horizontalMovement;
        Vector3 verticalMovement;

        if (useSphereArcMotion)
        {
            float arcLength = CalculateArcLength(phase);
            float angle = ConvertArcLengthToAngle(arcLength);

            horizontalMovement =
                CalculateSphereHorizontalMovement(moveDirection, angle);

            verticalMovement =
                CalculateSphereVerticalMovement(angle);
        }
        else
        {
            horizontalMovement =
                CalculateSimpleHorizontalMovement(moveDirection, phase);

            verticalMovement =
                CalculateSimpleVerticalMovement(phase);
        }

        return horizontalMovement
             + verticalMovement
             + lateralOffset;
    }

    private float CalculateArcLength(float phase)
    {
        return phase * stepLength;
    }

    private float ConvertArcLengthToAngle(float arcLength)
    {
        return arcLength / ballRadius;
    }

    private Vector3 CalculateSphereHorizontalMovement(
        Vector3 direction,
        float angle)
    {
        float distance = ballRadius * Mathf.Sin(angle);
        return direction * distance;
    }

    private Vector3 CalculateSphereVerticalMovement(float angle)
    {
        float height = ballRadius * (1f - Mathf.Cos(angle));
        return -Vector3.up * height;
    }

    private Vector3 CalculateSimpleHorizontalMovement(
        Vector3 direction,
        float phase)
    {
        return direction * (phase * stepLength);
    }

    private Vector3 CalculateSimpleVerticalMovement(float phase)
    {
        float height = Mathf.Max(0f, phase) * footLiftHeight;
        return Vector3.up * height;
    }

    private void ApplyFootOffsets(
        Vector3 leftOffset,
        Vector3 rightOffset)
    {
        leftFootTarget.localPosition =
            _leftIdlePosition + leftOffset;

        rightFootTarget.localPosition =
            _rightIdlePosition + rightOffset;
    }

    private void ReturnFeetToIdle()
    {
        leftFootTarget.localPosition =
            SmoothTowardsIdle(
                leftFootTarget.localPosition,
                _leftIdlePosition
            );

        rightFootTarget.localPosition =
            SmoothTowardsIdle(
                rightFootTarget.localPosition,
                _rightIdlePosition
            );

        ResetStepCycle();
    }

    private Vector3 SmoothTowardsIdle(
        Vector3 current,
        Vector3 target)
    {
        return Vector3.Lerp(
            current,
            target,
            Time.deltaTime * idleSmoothTime
        );
    }

    private void ResetStepCycle()
    {
        _stepProgress = 0f;
    }
}
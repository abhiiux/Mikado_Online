using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new(0f, 4f, -6f);
    [SerializeField] private float positionDamping = 5f;

    private Vector3 _smoothVelocity;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = target.position + offset;
        transform.position = Vector3.SmoothDamp(
            transform.position, targetPos,
            ref _smoothVelocity, 1f / positionDamping
        );

        transform.LookAt(target);
    }
}


using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new(0f, 4f, -6f);
    [SerializeField] private float positionDamping = 5f;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = target.position + offset;
        transform.position = Vector3.Lerp(
            transform.position, targetPos,
            positionDamping * Time.deltaTime
        );
        transform.LookAt(target);
    }
}

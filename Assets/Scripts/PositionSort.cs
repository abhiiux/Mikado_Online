using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PositionSort : MonoBehaviour
{
    [Header("Circle Settings")]
    public float radius = 5f; // Radius of the circle
    public bool sortOnStart = true;
    public bool orderByZ = false; // Sort by Z position before arranging
    private List<Transform> children
    {
        get
        {
            List<Transform> childList = new List<Transform>();
            foreach (Transform child in transform)
            {
                childList.Add(child);
            }
            return childList;
        }
    }

    void Start()
    {
        // for (int i = 0; i < children.Count; i++)
        // {
        //     children[i].position += new Vector3(i, 0, 0);
        // }
        ArrangeInCircle();
    }

    public void ArrangeInCircle()
    {
        var sortedChildren = children;

        // Optional: Sort by Z position first
        if (orderByZ)
        {
            sortedChildren = sortedChildren.OrderBy(t => t.position.z).ToList();
        }

        float angleStep = 360f / sortedChildren.Count;

        for (int i = 0; i < sortedChildren.Count; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPosition = new Vector3(
                Mathf.Cos(angle) * radius,
                0,
                Mathf.Sin(angle) * radius
            );

            sortedChildren[i].position = transform.position + newPosition;
        }
    }

}

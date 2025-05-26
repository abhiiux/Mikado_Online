using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PositionSort : MonoBehaviour
{
    [Header("Circle Settings")]
    public float radius = 5f; 
    public bool sortOnStart = true;
    public bool randomRotation = true;
    public float minRotation = 0f;     
    public float maxRotation = 360f;   
    
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
        if(sortOnStart) ArrangeInCircle();
    }

    public void ArrangeInCircle()
    {
        var sortedChildren = children;

        float angleStep = 360f / sortedChildren.Count;
        float startAngle = randomRotation ? Random.Range(minRotation, maxRotation) : 0f;

        for (int i = 0; i < sortedChildren.Count; i++)
        {
            float angle = (i * angleStep + startAngle) * Mathf.Deg2Rad;
            Vector3 newPosition = new Vector3(
                Mathf.Cos(angle) * radius,
                sortedChildren[i].localPosition.y, 
                Mathf.Sin(angle) * radius
            );

            sortedChildren[i].position = transform.position + newPosition;
        }
    }
}
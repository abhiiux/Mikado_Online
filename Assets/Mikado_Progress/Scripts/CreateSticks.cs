using System.Collections.Generic;
using UnityEngine;

public class CreateSticks : MonoBehaviour
{
    [SerializeField] private int noOfSticks; 
    [SerializeField] private GameObject prefabStick; 

    [Header("Circle Settings")]
    [SerializeField] float radius = 1f; 
    [SerializeField] bool sortOnStart = true;
    [SerializeField] bool randomRotation = true;
    [SerializeField] float minRotation = 0f;     
    [SerializeField] float maxRotation = 100f;   

    private Transform parentObject;
    private List<Transform> childrens = new List<Transform>();
    private StickCheck stickCheck;

    void Start()
    {
        parentObject = GetComponent<Transform>();
        stickCheck = GetComponent<StickCheck>();

        CreateSticksOnCall();
    }

    private void CreateSticksOnCall()
    {
        for (int i = 0; i < noOfSticks; i++)
        {
            GameObject t =  Instantiate( prefabStick, parentObject );
            childrens.Add( t.transform );
        }

        ArrangeInCircle();

        stickCheck.Init( childrens );
    }

    public void ArrangeInCircle()
    {
        var sortedChildren = childrens;

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
    }}

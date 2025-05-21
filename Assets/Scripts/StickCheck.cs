using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StickCheck : MonoBehaviour
{
    [SerializeField] bool isLog;
    [SerializeField] TMP_Text text;
    [SerializeField] GameObject cube;
    // private float distanceMoved;
    private Transform[] childTransfrom
    {
        get
        {
            Transform[] alltransfroms = GetComponentsInChildren<Transform>();
            List<Transform> childTransforms = new List<Transform>();

            foreach (Transform t in alltransfroms)
            {
                if (t != transform)
                {
                    childTransforms.Add(t);
                }
            }
            return childTransforms.ToArray();
        }
    }
    private Dictionary<Transform, Vector3> position = new Dictionary<Transform, Vector3>();

    
    void Start()
    {
        StartCoroutine(StorePositions());
    }
    public IEnumerator StorePositions()
    {
        yield return new WaitForSeconds(3f);

        foreach (var item in childTransfrom)
        {
            position.Add(item, item.transform.position);
        }
        Log("Position stored ");
        // cube.SetActive(true);
    }

    public bool DetectStickMove(GameObject selectedStick)
    {
        foreach (var stick in position.Keys)
        {
            if (stick.gameObject.name == selectedStick.name)
                continue;

            float distanceMoved = Vector3.Distance(
                position[stick],
                stick.transform.position
            );

            Debug.Log($"Distance moved for {stick.name}: {distanceMoved}");

            if (distanceMoved > 0.1f)
            {
                Log("Movement Detected!");
                return true;
            }
        }

    Log("No Movement");
    return false;
    }
    public void OnStickCollected(GameObject stick)
    {
        position.Remove(stick.gameObject.transform); 
    }
    public void Finished()
    {
        Debug.Log("Button press detected");
        int obj = 0;
        foreach (Transform item in position.Keys)
        {
            if (item.gameObject.activeInHierarchy)
            {
                obj++;
                Debug.Log($" an item name {item} is active ");
            }
        }
        if (obj <= 1)
        {
            Log("You Won!");
        }
    }

    private void Log(string message)
    {
        if (isLog)
        {
            // Debug.Log(message);
            text.text = message;
        }
    }
}

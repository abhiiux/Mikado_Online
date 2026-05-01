using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class StickCheck : MonoBehaviour
{
    [SerializeField] ShaderControls shaderControls;
    [SerializeField] TMP_Text noOfSticks;
    [SerializeField] bool isLog;
    [SerializeField] float gamestartTime;
    [SerializeField] TMP_Text text;
    [SerializeField] float moveThreshold;
    private bool isposTake;
    private int stickCount;

    [Header("Circle Arrange Settings")]
    public float radius = 5f;
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
    private Dictionary<Transform, Vector3> position = new Dictionary<Transform, Vector3>();


    void Start()
    {
        StartCoroutine(StartGame());
    }
    public IEnumerator StartGame()
    {
        Log("please wait until sticks are settle");
        yield return new WaitForSeconds(gamestartTime);
        foreach (Transform item in children)
        {
            position.Add(item, item.transform.position);
        }  
        isposTake = true;
        noOfSticks.text = children.Count.ToString();
        Log("Position stored "+ children.Count);
        Log("Goo!");
    }

    public void DetectStickMove(GameObject selectedStick)
    {
        List<GameObject> sticksToUpdate = new List<GameObject>();
        foreach (var stick in position.Keys)
        {
            if (stick.gameObject == selectedStick)
                continue;

            float distanceMoved = Vector3.Distance(
                position[stick],
                stick.transform.position);     // Comparing positions to detect any movement!

            if (distanceMoved > moveThreshold)
            {
                sticksToUpdate.Add(stick.gameObject);              //Storing new position
                shaderControls.DamageGlow(sticksToUpdate);
                Log("Movement Detected!");
                Debug.Log($"Distance moved for {stick.name}: {distanceMoved}");

                Renderer renderer = selectedStick.GetComponent<Renderer>();
                ObjectPoints obj = selectedStick.GetComponent<ObjectPoints>();

                renderer.material.color = Color.black;
                obj.isFlagged = true;

            }
        }
    foreach (GameObject storedsticks in sticksToUpdate)    // Updating new position
    {
        position[storedsticks.transform] = storedsticks.transform.position;
    }
    Log("No Movement");
    }

    public void OnStickCollected(GameObject stick)
    {
        position.Remove(stick.gameObject.transform);
    }
    
    public void ArrangeInCircle()
    {
        var sortedChildren = children;

        if (sortedChildren.Count == 0) return;

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

    // private void HitBlink(Renderer renderer)
    // {
    //     renderer.material.SetFloat("_colorIntensity", 1f);
    //     renderer.material.SetFloat("_colorIntensity", 0f);
    //     // renderer.
    // }
    public bool GetStatus()
    {
        return isposTake;
    }
    private void Log(string message)
    {
        if (isLog)
        {
            text.text = message;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(StickCheck))]
public class StickCheckEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        StickCheck stickCheck = (StickCheck)target;

        GUILayout.Space(10);
        if (GUILayout.Button("Arrange In Circle"))
        {
            Undo.RecordObjects(
                stickCheck.transform.Cast<Transform>().Select(t => (Object)t).ToArray(),
                "Arrange Sticks In Circle"
            );
            stickCheck.ArrangeInCircle();
        }
    }
}
#endif
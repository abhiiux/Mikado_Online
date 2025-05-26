using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StickCheck : MonoBehaviour
{
    [SerializeField] TMP_Text noOfSticks;
    [SerializeField] bool isLog;
    [SerializeField] float time;
    [SerializeField] TMP_Text text;
    [SerializeField] float moveThreshold;
    private bool isposTake;
    private int stickCount;

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
        yield return new WaitForSeconds(time);
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
        List<Transform> sticksToUpdate = new List<Transform>();
        foreach (var stick in position.Keys)
        {
            if (stick.gameObject == selectedStick)
                continue;

            float distanceMoved = Vector3.Distance(
                position[stick],
                stick.transform.position);     // Comparing positions to detect any movement!

            if (distanceMoved > moveThreshold)
            {
                Log("Movement Detected!");
                Debug.Log($"Distance moved for {stick.name}: {distanceMoved}");

                Renderer renderer = selectedStick.GetComponent<Renderer>();
                ObjectPoints obj = selectedStick.GetComponent<ObjectPoints>();

                renderer.material.color = Color.black;
                obj.isFlagged = true;

                sticksToUpdate.Add(stick);              //Storing new position
            }
        }
    foreach (Transform stick in sticksToUpdate)    // Updating new position
    {
        position[stick] = stick.transform.position;
    }
    Log("No Movement");
    }
    public void OnStickCollected(GameObject stick)
    {
        position.Remove(stick.gameObject.transform); 
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

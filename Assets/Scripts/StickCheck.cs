using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.ShaderGraph;
using UnityEngine;

public class StickCheck : MonoBehaviour
{
    [SerializeField] bool isLog;
    [SerializeField] float time;
    [SerializeField] TMP_Text text;
    [SerializeField] float moveThreshold;
    private bool isposTake;

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
        StartCoroutine(StorePositions());
    }
    public IEnumerator StorePositions()
    {        
        yield return new WaitForSeconds(time);

        foreach (Transform item in children)
        {
            position.Add(item, item.transform.position);
        }
        isposTake = true;
        Log("Position stored "+ children.Count+" and bool is "+isposTake);
    }

    public void DetectStickMove(GameObject selectedStick)
    {
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
                // Renderer moverenderer = stick.GetComponent<Renderer>();
                // HitBlink(moverenderer);
                renderer.material.color = Color.black;
                int pointsLeft = obj.Points;
                obj.Points = 0;
            }
        }
    Log("No Movement");
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
        if (obj == 0)
        {
            Log("You Won!");
        }
        else
        {
            Log(" Nah u Lose!");
        }
    }
    private void HitBlink(Renderer renderer)
    {
        renderer.material.SetFloat("_colorIntensity", 1f);
        renderer.material.SetFloat("_colorIntensity", 0f);
        // renderer.
    }
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

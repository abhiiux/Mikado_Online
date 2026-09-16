using System.Collections;
using System.Collections.Generic;
using Mikado.Core;
// using Mikado.Presentation;
using TMPro;
using UnityEngine;

namespace Mikado.Gameplay
{
    public class StickCheck : MonoBehaviour
    {
        // [SerializeField] ShaderControls shaderControls;
    [SerializeField] TMP_Text noOfSticks;
    [SerializeField] bool isLog;
    [SerializeField] float gamestartTime;
    [SerializeField] TMP_Text text;
    [SerializeField] float moveThreshold;
    private bool isposTake;
    private int stickCount;

    // private List<Transform> children
    // {
    //     get
    //     {
    //         List<Transform> childList = new List<Transform>();
    //         foreach (Transform child in transform)
    //         {
    //             childList.Add(child);
    //         }
    //         return childList;
    //     }
    // }
    private List<Transform> children;
    private Dictionary<Transform, Vector3> position = new Dictionary<Transform, Vector3>();


    void OnEnable()
    {
        GameEventBus.OnTargetCollisionDetected += MovementDetection;
    }
    void OnDisable()
    {
        GameEventBus.OnTargetCollisionDetected -= MovementDetection;
    }
    public void Init(List<Transform> newChildren)
    {
        children = newChildren;

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
    private void MovementDetection(GameObject stick)
    {
        DetectStickMove(stick);
        OnStickCollected(stick);
    }
    private void DetectStickMove(GameObject selectedStick)
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
                // shaderControls.DamageGlow(sticksToUpdate);
                Log("Movement Detected!");
                Debug.Log($"Distance moved for {stick.name}: {distanceMoved}");

                Renderer renderer = selectedStick.GetComponent<Renderer>();
                // ObjectPoints obj = selectedStick.GetComponent<ObjectPoints>();

                renderer.material.color = Color.black;
                // obj.isFlagged = true;

            }
        }

        foreach (GameObject storedsticks in sticksToUpdate)    // Updating new position
        {
            position[storedsticks.transform] = storedsticks.transform.position;
        }
    }

    private void OnStickCollected(GameObject stick)
    {
        position.Remove(stick.gameObject.transform);
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
}

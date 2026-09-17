using System.Collections;
using System.Collections.Generic;
using Mikado.Core;
using TMPro;
using UnityEngine;

namespace Mikado.Gameplay
{
    public class StickCheck : MonoBehaviour
    {
    [SerializeField] TMP_Text noOfSticks;
    [SerializeField] bool isLog;
    [SerializeField] float gamestartTime;
    [SerializeField] TMP_Text text;
    [SerializeField] float moveThreshold;

    private bool isposTake;
    private int stickCount;
    private List<Transform> children;
    private List<ObjectPoints> childrenScripts = new List<ObjectPoints>();
    private Dictionary<Transform, Vector3> position = new Dictionary<Transform, Vector3>();
    private int _lastSelectedIndex = -1;


    void OnEnable()
    {
        GameEventBus.OnTargetCollisionDetected += MovementDetection;
        GameEventBus.OnTargetChange            += ChangeObjectState;
    }
    void OnDisable()
    {
        GameEventBus.OnTargetCollisionDetected -= MovementDetection;
        GameEventBus.OnTargetChange            -= ChangeObjectState;
    }
    public void Init(List<Transform> newChildren)
    {
        children = newChildren;
        childrenScripts.Clear();
        position.Clear();
        _lastSelectedIndex = -1;

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

        InitScripts();
        Log("Position stored "+ children.Count);
        Log("Goo!");
    }
    private void InitScripts()
    {
        foreach (var item in children)
        {
            childrenScripts.Add( item.GetComponent<ObjectPoints>() );
        }

        foreach (var item in childrenScripts)
        {
            item.Init();
        }
    }
    private void ChangeObjectState(Transform selectedTransform)
    {
        if(selectedTransform == null) return; // D1: intentionally ignore - TakeThis disables stick

        if (children == null || childrenScripts == null || childrenScripts.Count == 0) return;

        int index = children.IndexOf(selectedTransform);
        if (index < 0)
        {
            Debug.LogWarning($"[StickCheck] Selected transform not found in children.");
            return;
        }
        if (index >= childrenScripts.Count) return; // parallel-array desync guard

        if (index == _lastSelectedIndex) return; // same stick spam -> no-op (UpdateState is idempotent)

        if (_lastSelectedIndex >= 0 && _lastSelectedIndex < childrenScripts.Count)
        {
            var prev = childrenScripts[_lastSelectedIndex];
            if (prev != null) prev.SetSelection(false);
        }

        var next = childrenScripts[index];
        if (next != null)
        {
            next.SetSelection(true);
            // Debug.Log($" index's name is {next.nameStick}");
        }
        _lastSelectedIndex = index;
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
                Log("Movement Detected!");
                Debug.Log($"Distance moved for {stick.name}: {distanceMoved}");
            }
        }

        foreach (GameObject storedsticks in sticksToUpdate)    // Updating new position
        {
            position[storedsticks.transform] = storedsticks.transform.position;
        }
    }

    private void OnStickCollected(GameObject stick)
    {
        // D2: invalidate cached index if collected stick was selected (saves deselecting disabled object)
        if (children != null && _lastSelectedIndex >= 0 && _lastSelectedIndex < children.Count)
        {
            if (children[_lastSelectedIndex] == stick.transform)
                _lastSelectedIndex = -1;
        }
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

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
    [SerializeField, Tooltip("Consecutive over-threshold checks required before a move counts as illegal. " +
        "At a 50Hz fixed timestep, 5 ~= 0.1s of sustained movement. Filters out single-tick noise spikes.")]
    private int debounceTicks = 5;

    private bool isposTake;
    private int stickCount;
    private List<Transform> children;
    private List<ObjectPoints> childrenScripts = new List<ObjectPoints>();
    // Active-set membership source. After the live-read fix, values are stale — only keys
    // matter (which sticks are still uncollected). See CaptureBaseline for the live read.
    private Dictionary<Transform, Vector3> position = new Dictionary<Transform, Vector3>();
    private int _lastSelectedIndex = -1;

    // Continuous monitoring window: opens on selection (OnTargetChange with a real
    // transform), stays open every FixedUpdate, and closes when CollisionChecker
    // reports the stick reached the collision trigger (OnTargetCollisionDetected).
    private bool isMonitoring;
    private Transform monitoredTarget;
    private ObjectPoints monitoredObjectPoints; // the currently-selected stick's script, so we can force-deselect it

    // Frozen snapshot taken at the START of the current attempt. Unlike 'position',
    // this is never rewritten mid-attempt, so slow cumulative drift across many ticks
    // is measured against where a stick started the attempt, not its last checked tick.
    private Dictionary<Transform, Vector3> attemptBaseline;

    // Debounce bookkeeping: consecutive checks (within the current attempt) a given
    // stick has been found over moveThreshold. Reset to 0 the moment it drops back under.
    private Dictionary<Transform, int> overThresholdStreak = new Dictionary<Transform, int>();

    // Deferred baseline capture — avoids Update-vs-Physics torn sample by capturing
    // in the next FixedUpdate tick. No settle wait, just 0.02s physics sync.
    private bool pendingBaselineCapture;
    // Diagnostic: velocity at capture time to quantify carry-over momentum false positives.
    private Dictionary<Transform, Vector3> baselineVelocity = new Dictionary<Transform, Vector3>();

    void OnEnable()
    {
        GameEventBus.OnTargetCollisionDetected += MovementDetection;
        GameEventBus.OnTargetChange            += ChangeObjectState;
        GameEventBus.OnTargetChange            += HandleMonitoringTargetChange;
    }
    void OnDisable()
    {
        GameEventBus.OnTargetCollisionDetected -= MovementDetection;
        GameEventBus.OnTargetChange            -= ChangeObjectState;
        GameEventBus.OnTargetChange            -= HandleMonitoringTargetChange;
    }
    public void Init(List<Transform> newChildren)
    {
        children = newChildren;
        childrenScripts.Clear();
        position.Clear();
        _lastSelectedIndex = -1;
        attemptBaseline = null;
        overThresholdStreak.Clear();
        pendingBaselineCapture = false;
        baselineVelocity.Clear();

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
        if(selectedTransform == null)
        {
            // D1: intentionally don't deselect visuals here — callers that fire null
            // either already handled the actual deselect themselves (DeselectMonitoredStick)
            // or the stick's about to be disabled anyway (TakeThis). But always drop the
            // cached index so it can't go stale if some future caller doesn't.
            _lastSelectedIndex = -1;
            return;
        }

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
    // Opens/closes the monitoring window. Fires on every OnTargetChange, including
    // the selection click (real transform) and any deselection (null, e.g. from
    // ChangeObjectState's own re-selection flow or CollisionChecker's deselect-on-collect).
    private void HandleMonitoringTargetChange(Transform selectedTransform)
    {
        if (selectedTransform == null)
        {
            if (isMonitoring) EndAttempt(); // player abandoned the attempt without a collision
            isMonitoring = false;
            pendingBaselineCapture = false;
            monitoredTarget = null;
            monitoredObjectPoints = null;
            return;
        }

        monitoredTarget = selectedTransform;
        monitoredObjectPoints = GetObjectPointsFor(selectedTransform);
        isMonitoring = true;
        // Defer baseline capture to next FixedUpdate to get a physics-synced position
        // (avoids Update-vs-Physics torn sample). No settle wait — just 0.02s.
        pendingBaselineCapture = true;
    }

    // Same lookup pattern ChangeObjectState uses (parallel children/childrenScripts lists).
    private ObjectPoints GetObjectPointsFor(Transform t)
    {
        if (children == null || childrenScripts == null) return null;
        int idx = children.IndexOf(t);
        if (idx < 0 || idx >= childrenScripts.Count) return null;
        return childrenScripts[idx];
    }

    // Captures a LIVE snapshot of every active stick's current position. Uses
    // position.Keys only for membership (which sticks are still uncollected); values
    // are read live from Transform.position so idle-gap settle drift is not
    // misattributed to the new attempt. Also snapshots velocity for diagnostics.
    private void CaptureBaseline()
    {
        attemptBaseline = new Dictionary<Transform, Vector3>(position.Count);
        baselineVelocity.Clear();
        foreach (var stick in position.Keys)
        {
            if (stick == null) continue;
            attemptBaseline[stick] = stick.position; // live read — keys are membership, values are fresh
            // Diagnostic: capture velocity to quantify carry-over momentum false positives.
            var rb = stick.GetComponent<Rigidbody>();
            baselineVelocity[stick] = rb != null ? rb.linearVelocity : Vector3.zero;
        }
        overThresholdStreak.Clear();

        // Diagnostic: log max velocity at capture to tune carry-over handling without blocking selection.
        float maxVel = 0f;
        foreach (var v in baselineVelocity.Values) maxVel = Mathf.Max(maxVel, v.magnitude);
        if (maxVel > 0.05f)
            Debug.Log($"[StickCheck] CaptureBaseline maxVel={maxVel:F3} (carry-over risk — {attemptBaseline.Count} sticks)");
    }

    // Legacy BeginAttempt kept for external callers if any — forwards to CaptureBaseline.
    private void BeginAttempt() => CaptureBaseline();

    // Attempt is over (collected cleanly, or abandoned). With the live-read fix,
    // the sync-back loop (position[stick]=stick.position) is dead weight — baseline
    // no longer copies position values — so we just clear attempt state.
    // Position keys still define membership; values remain stale intentionally until
    // eventual HashSet migration. Keep idempotent for double EndAttempt via
    // DeselectMonitoredStick -> TriggerTargetChange(null) reentrancy.
    private void EndAttempt()
    {
        // Former sync loop removed: foreach (var stick in position.Keys) position[stick]=stick.position;
        attemptBaseline = null;
        overThresholdStreak.Clear();
        baselineVelocity.Clear();
        pendingBaselineCapture = false;
    }

    void FixedUpdate()
    {
        if (!isposTake || monitoredTarget == null) return;

        // Physics-synced deferred capture — 1 tick delay, no settle wait.
        if (pendingBaselineCapture)
        {
            if (!isMonitoring) { pendingBaselineCapture = false; return; }
            CaptureBaseline();
            pendingBaselineCapture = false;
            return; // don't detect on the same tick we captured — need at least one delta
        }

        if (!isMonitoring) return;
        DetectStickMove(monitoredTarget.gameObject);
    }

    private void MovementDetection(GameObject stick)
    {
        // Final check at the exact moment CollisionChecker reports the trigger.
        // If baseline hasn't been captured yet (rare: trigger fired before next FixedUpdate),
        // capture now so the final check isn't skipped.
        if (pendingBaselineCapture && isMonitoring && attemptBaseline == null)
        {
            CaptureBaseline();
            pendingBaselineCapture = false;
        }
        DetectStickMove(stick);
        EndAttempt(); // with live-read, just clears state — no sync-back
        isMonitoring = false;
        monitoredTarget = null;
        monitoredObjectPoints = null;
        OnStickCollected(stick);
    }

    // Compares every (non-picked-up) stick against the FROZEN attempt-start snapshot,
    // not against a running snapshot — so slow drift across many ticks accumulates
    // instead of being masked. A move only counts as illegal once it has stayed over
    // moveThreshold for 'debounceTicks' consecutive checks, so a single noisy spike
    // (physics jitter) doesn't get logged as a cheat.
    private void DetectStickMove(GameObject selectedStick)
    {
        if (attemptBaseline == null) return; // no active attempt to check against

        foreach (var kvp in attemptBaseline)
        {
            Transform stick = kvp.Key;
            if (stick == null) continue;             // stick collected/destroyed mid-attempt
            if (stick.gameObject == selectedStick) continue; // never flag the stick being picked up

            float distanceMoved = Vector3.Distance(kvp.Value, stick.position);

            if (distanceMoved > moveThreshold)
            {
                int streak = overThresholdStreak.TryGetValue(stick, out var s) ? s + 1 : 1;
                overThresholdStreak[stick] = streak;

                if (streak == debounceTicks) // fire once, the moment it's been sustained long enough
                {
                    Log("Movement Detected!");
                    Debug.Log($"Distance moved for {stick.name}: {distanceMoved} (sustained {debounceTicks} checks)");
                    DeselectMonitoredStick(); // cancel this pickup attempt — the player caused an illegal move
                    ObjectPoints objectPoints = stick.GetComponent<ObjectPoints>();
                    if (objectPoints != null)
                    {
                        objectPoints.ToggleRedVisual();
                    }
                    return;
                }
            }
            else
            {
                overThresholdStreak[stick] = 0; // needs sustained movement, not a single spike
            }
        }
    }

    // Called when a bystander stick's movement has been confirmed illegal (sustained
    // past the debounce window). Forces the currently-held stick to deselect —
    // same effect as ObjectPoints.SetSelection(false) via normal deselection — and
    // closes out this attempt so future ticks don't keep scanning against it.
    private void DeselectMonitoredStick()
    {
        if (monitoredObjectPoints != null)
        {
            monitoredObjectPoints.SetSelection(false);
        }

        // Keep _lastSelectedIndex in sync so the next real selection doesn't try to
        // deselect a stick that's already been force-deselected here.
        if (children != null && monitoredTarget != null)
        {
            int idx = children.IndexOf(monitoredTarget);
            if (idx >= 0 && idx == _lastSelectedIndex) _lastSelectedIndex = -1;
        }

        // Direct SetSelection(false) above only reaches ShaderControls (via OnStickSelected).
        // Broadcast the deselect too, so everyone else watching OnTargetChange — PickUpController
        // (clears its cached pickUpObject), CameraRotation (resets to default target), etc. —
        // also learns this pickup attempt was cancelled. Same convention CollisionChecker uses.
        GameEventBus.TriggerTargetChange(null);

        EndAttempt();
        isMonitoring = false;
        monitoredTarget = null;
        monitoredObjectPoints = null;
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
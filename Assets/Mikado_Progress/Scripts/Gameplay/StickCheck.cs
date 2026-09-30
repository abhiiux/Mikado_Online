using System.Collections;
using System.Collections.Generic;
using Mikado.Core;
using TMPro;
using UnityEngine;

namespace Mikado.Gameplay
{
    public class StickCheck : MonoBehaviour
    {
    [SerializeField] bool isLog;
    [SerializeField] float gamestartTime;
    [SerializeField] TMP_Text text;
    [SerializeField] float moveThreshold;
    [SerializeField, Tooltip("Consecutive over-threshold checks required before a move counts as illegal. " +
        "At a 50Hz fixed timestep, 5 ~= 0.1s of sustained movement. Filters out single-tick noise spikes.")]
    private int debounceTicks = 5;
    [SerializeField, Tooltip("Rotation threshold in degrees. Catches tip swing of long sticks " +
        "that barely translate at the center.")]
    private float angleThresholdDeg = 5f;

    private bool isposTake;

    // ID-based registry (Phase 1). Entries stay for the whole game even after
    // collection (SetActive(false)), mirroring the old parallel-list behaviour —
    // activeIds is the membership set, registry is the lookup.
    private sealed class StickRecord
    {
        public Transform transform;
        public ObjectPoints points;
        public Rigidbody rb;
    }

    private enum TurnPhase { Idle, BaselinePending, Monitoring }

    private Dictionary<int, StickRecord> registry = new Dictionary<int, StickRecord>();
    private Dictionary<Transform, int> transformToId = new Dictionary<Transform, int>();
    private HashSet<int> activeIds = new HashSet<int>();

    // Legacy parallel lists kept populated for external callers that may still
    // index them, but no longer the source of truth for detection.
    private List<Transform> children;
    private List<ObjectPoints> childrenScripts = new List<ObjectPoints>();
    private int _lastSelectedId = -1;

    // Continuous monitoring window: opens on selection (OnTargetChange with a real
    // transform), stays open every FixedUpdate, and closes when CollisionChecker
    // reports the stick reached the collision trigger (OnTargetCollisionDetected).
    private TurnPhase phase = TurnPhase.Idle;
    private Transform monitoredTarget;
    private int monitoredStickId = -1;
    private ObjectPoints monitoredObjectPoints; // the currently-selected stick's script, so we can force-deselect it

    // Frozen snapshot taken at the START of the current attempt. Never rewritten
    // mid-attempt, so slow cumulative drift across many ticks is measured against
    // where a stick started the attempt, not its last checked tick.
    private Dictionary<int, StickSnapshot> attemptBaseline;

    // Time-based debounce (Phase 0 pure class). Sustain window derived from
    // debounceTicks so existing inspector tuning carries over.
    private SustainedMovementTracker tracker = new SustainedMovementTracker(0.1f);

    // Diagnostic: velocity at capture time to quantify carry-over momentum false positives.
    private Dictionary<int, Vector3> baselineVelocity = new Dictionary<int, Vector3>();

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
        registry.Clear();
        transformToId.Clear();
        activeIds.Clear();
        _lastSelectedId = -1;
        attemptBaseline = null;
        tracker.Clear();
        phase = TurnPhase.Idle;
        monitoredTarget = null;
        monitoredStickId = -1;
        monitoredObjectPoints = null;
        baselineVelocity.Clear();

        // Build ID registry. Prefer StickId assigned by CreateSticks; fall back
        // to spawn index so direct Init() callers still work.
        for (int i = 0; i < newChildren.Count; i++)
        {
            Transform t = newChildren[i];
            if (t == null) continue;
            ObjectPoints pts = t.GetComponent<ObjectPoints>();
            int id = (pts != null && pts.StickId >= 0) ? pts.StickId : i;
            if (pts != null && pts.StickId < 0) pts.StickId = id;
            if (registry.ContainsKey(id))
            {
                Debug.LogWarning($"[StickCheck] Duplicate StickId {id}, falling back to index {i}.");
                id = i;
                if (pts != null) pts.StickId = id;
            }
            if (registry.ContainsKey(id)) continue; // still colliding — skip
            var rb = t.GetComponent<Rigidbody>();
            registry[id] = new StickRecord { transform = t, points = pts, rb = rb };
            transformToId[t] = id;
        }

        tracker.SetSustain(SustainSeconds());

        StartCoroutine(StartGame());
    }

    private float SustainSeconds()
    {
        float step = Time.fixedDeltaTime > 0f ? Time.fixedDeltaTime : 0.02f;
        return Mathf.Max(0f, debounceTicks * step);
    }

    public IEnumerator StartGame()
    {
        Log("please wait until sticks are settle");
        yield return new WaitForSecondsRealtime(gamestartTime);
        foreach (var id in registry.Keys)
        {
            activeIds.Add(id);
        }
        isposTake = true;

        InitScripts();
        Log("Position stored "+ registry.Count);
        Log("Goo!");
    }
    private void InitScripts()
    {
        childrenScripts.Clear();
        // Keep legacy list in spawn order for any external indexer.
        if (children != null)
        {
            foreach (var t in children)
            {
                ObjectPoints p = null;
                if (t != null && transformToId.TryGetValue(t, out int id) &&
                    registry.TryGetValue(id, out var rec))
                    p = rec.points;
                else if (t != null)
                    p = t.GetComponent<ObjectPoints>();
                childrenScripts.Add(p);
            }
        }
        foreach (var rec in registry.Values)
        {
            rec.points?.Init();
        }
    }
    private void ChangeObjectState(Transform selectedTransform)
    {
        if(selectedTransform == null)
        {
            // D1: intentionally don't deselect visuals here — callers that fire null
            // either already handled the actual deselect themselves (DeselectMonitoredStick)
            // or the stick's about to be disabled anyway (TakeThis). But always drop the
            // cached id so it can't go stale if some future caller doesn't.
            _lastSelectedId = -1;
            return;
        }

        if (!TryGetStickId(selectedTransform, out int id)) return;
        if (!registry.TryGetValue(id, out var rec) || rec.points == null) return;
        if (!activeIds.Contains(id)) return; // collected sticks can't be re-selected

        if (id == _lastSelectedId) return; // same stick spam -> no-op (UpdateState is idempotent)

        if (_lastSelectedId >= 0 && registry.TryGetValue(_lastSelectedId, out var prevRec))
        {
            prevRec.points?.SetSelection(false);
        }

        rec.points.SetSelection(true);
        _lastSelectedId = id;
    }
    // Opens/closes the monitoring window. Fires on every OnTargetChange, including
    // the selection click (real transform) and any deselection (null, e.g. from
    // ChangeObjectState's own re-selection flow or CollisionChecker's deselect-on-collect).
    private void HandleMonitoringTargetChange(Transform selectedTransform)
    {
        if (selectedTransform == null)
        {
            if (phase == TurnPhase.Monitoring) EndAttempt(); // player abandoned the attempt without a collision
            phase = TurnPhase.Idle;
            monitoredTarget = null;
            monitoredStickId = -1;
            monitoredObjectPoints = null;
            return;
        }

        if (!TryGetStickId(selectedTransform, out int id)) return;
        if (!registry.TryGetValue(id, out var rec)) return;

        monitoredTarget = selectedTransform;
        monitoredStickId = id;
        monitoredObjectPoints = rec.points;
        phase = TurnPhase.BaselinePending;
        // Defer baseline capture to next FixedUpdate to get a physics-synced position
        // (avoids Update-vs-Physics torn sample). No settle wait — just 0.02s.
    }

    private bool TryGetStickId(Transform t, out int id)
    {
        if (t != null && transformToId.TryGetValue(t, out id))
            return true;
        var pts = t != null ? t.GetComponent<ObjectPoints>() : null;
        if (pts != null && pts.StickId >= 0 && registry.ContainsKey(pts.StickId))
        {
            id = pts.StickId;
            return true;
        }
        Debug.LogWarning($"[StickCheck] Selected transform not found in registry.");
        id = -1;
        return false;
    }

    // Same lookup pattern ChangeObjectState uses (registry by id).
    private ObjectPoints GetObjectPointsFor(Transform t)
    {
        if (TryGetStickId(t, out int id) && registry.TryGetValue(id, out var rec))
            return rec.points;
        return null;
    }

    // Captures a LIVE snapshot of every active stick's current pose. Uses
    // activeIds for membership (which sticks are still uncollected); poses
    // are read live from Transform so idle-gap settle drift is not
    // misattributed to the new attempt. Also snapshots velocity for diagnostics.
    private void CaptureBaseline()
    {
        attemptBaseline = new Dictionary<int, StickSnapshot>(activeIds.Count);
        baselineVelocity.Clear();
        foreach (int id in activeIds)
        {
            if (!registry.TryGetValue(id, out var rec) || rec.transform == null) continue;
            attemptBaseline[id] = new StickSnapshot
            {
                stickId = id,
                position = rec.transform.position,
                rotation = rec.transform.rotation
            };
            baselineVelocity[id] = rec.rb != null ? rec.rb.linearVelocity : Vector3.zero;
        }
        tracker.SetSustain(SustainSeconds());
        tracker.Clear();
    }

    // Legacy BeginAttempt kept for external callers if any — forwards to CaptureBaseline.
    private void BeginAttempt() => CaptureBaseline();

    // Attempt is over (collected cleanly, or abandoned). Just clears attempt state.
    // Keep idempotent for double EndAttempt via
    // DeselectMonitoredStick -> TriggerTargetChange(null) reentrancy.
    private void EndAttempt()
    {
        attemptBaseline = null;
        tracker.Clear();
        baselineVelocity.Clear();
    }

    void FixedUpdate()
    {
        if (!isposTake || monitoredTarget == null) return;

        // Physics-synced deferred capture — 1 tick delay, no settle wait.
        if (phase == TurnPhase.BaselinePending)
        {
            CaptureBaseline();
            phase = TurnPhase.Monitoring;
            return; // don't detect on the same tick we captured — need at least one delta
        }

        if (phase != TurnPhase.Monitoring) return;
        DetectStickMove(monitoredTarget.gameObject);
    }

    private void MovementDetection(GameObject stick)
    {
        // Final check at the exact moment CollisionChecker reports the trigger.
        // If baseline hasn't been captured yet (rare: trigger fired before next FixedUpdate),
        // capture now so the final check isn't skipped.
        if (phase == TurnPhase.BaselinePending && attemptBaseline == null)
        {
            CaptureBaseline();
            phase = TurnPhase.Monitoring;
        }
        DetectStickMove(stick);
        EndAttempt(); // just clears state — no sync-back
        phase = TurnPhase.Idle;
        monitoredTarget = null;
        monitoredStickId = -1;
        monitoredObjectPoints = null;
        OnStickCollected(stick);
    }

    // Compares every (non-picked-up) stick against the FROZEN attempt-start snapshot,
    // not against a running snapshot — so slow drift across many ticks accumulates
    // instead of being masked. A move only counts as illegal once it has stayed over
    // threshold for the sustain window, so a single noisy spike
    // (physics jitter) doesn't get logged as a cheat.
    private void DetectStickMove(GameObject selectedStick)
    {
        if (attemptBaseline == null) return; // no active attempt to check against

        int selectedId = -1;
        if (selectedStick != null)
            transformToId.TryGetValue(selectedStick.transform, out selectedId);

        var currentById = new Dictionary<int, StickRecordPose>(attemptBaseline.Count);
        foreach (var kvp in attemptBaseline)
        {
            int id = kvp.Key;
            if (!registry.TryGetValue(id, out var rec) || rec.transform == null) continue;
            currentById[id] = new StickRecordPose
            {
                position = rec.transform.position,
                rotation = rec.transform.rotation
            };
        }

        List<int> moved = StickMotionRule.FindMoved(
            attemptBaseline, currentById, selectedId, moveThreshold, angleThresholdDeg);

        List<int> newlyConfirmed = tracker.Update(moved, Time.fixedTime);
        if (newlyConfirmed.Count == 0) return;

        // Fire once on the first confirmed stick; the rest get cleared with EndAttempt.
        int fouledId = newlyConfirmed[0];
        DeselectMonitoredStick(); // cancel this pickup attempt — the player caused an illegal move
        if (registry.TryGetValue(fouledId, out var fouledRec))
        {
            fouledRec.points?.ToggleRedVisual();
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
            if (monitoredTarget.TryGetComponent<Rigidbody>(out var rb)) {
                rb.useGravity = true;
                rb.WakeUp();
            }
        }

        // Keep _lastSelectedId in sync so the next real selection doesn't try to
        // deselect a stick that's already been force-deselected here.
        if (monitoredStickId >= 0 && monitoredStickId == _lastSelectedId) _lastSelectedId = -1;

        // Direct SetSelection(false) above only reaches ShaderControls (via OnStickSelected).
        // Broadcast the deselect too, so everyone else watching OnTargetChange — PickUpController
        // (clears its cached pickUpObject), CameraRotation (resets to default target), etc. —
        // also learns this pickup attempt was cancelled. Same convention CollisionChecker uses.
        GameEventBus.TriggerTargetChange(null);

        EndAttempt();
        phase = TurnPhase.Idle;
        monitoredTarget = null;
        monitoredStickId = -1;
        monitoredObjectPoints = null;
    }

    private void OnStickCollected(GameObject stick)
    {
        // D2: invalidate cached id if collected stick was selected (saves deselecting disabled object)
        if (stick != null && transformToId.TryGetValue(stick.transform, out int id))
        {
            if (id == _lastSelectedId)
                _lastSelectedId = -1;
            activeIds.Remove(id);
            attemptBaseline?.Remove(id);
            tracker.Remove(id);
        }
    }

    public bool GetStatus()
    {
        return isposTake;
    }
    private void Log(string message)
    {
        if (isLog)
        {
            text.text = $"<b>{message}</b>";
        }
    }
    }
}

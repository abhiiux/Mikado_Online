using System.Collections.Generic;
using UnityEngine;

namespace Mikado.Core
{
    /// <summary>
    /// Network-friendly snapshot of one stick. No Transform refs — just an ID + pose,
    /// so the same data can be captured locally today and sent over RPCs tomorrow.
    /// </summary>
    [System.Serializable]
    public struct StickSnapshot
    {
        public int stickId;
        public Vector3 position;
        public Quaternion rotation;
    }

    /// <summary>
    /// Pure movement rule: position + rotation threshold test. No MonoBehaviour,
    /// no Time, no physics — EditMode-testable and host/guest shareable.
    /// </summary>
    public static class StickMotionRule
    {
        public static bool IsMoved(
            in StickSnapshot baseline,
            in StickSnapshot current,
            float posThreshold,
            float angleThresholdDeg)
        {
            Vector3 delta = current.position - baseline.position;
            if (delta.sqrMagnitude > posThreshold * posThreshold)
                return true;

            if (angleThresholdDeg > 0f &&
                Quaternion.Angle(baseline.rotation, current.rotation) > angleThresholdDeg)
                return true;

            return false;
        }

        public static List<int> FindMoved(
            IReadOnlyDictionary<int, StickSnapshot> baseline,
            IReadOnlyDictionary<int, StickRecordPose> currentById,
            int selectedId,
            float posThreshold,
            float angleThresholdDeg)
        {
            var moved = new List<int>();
            foreach (var kvp in baseline)
            {
                int id = kvp.Key;
                if (id == selectedId) continue; // never self-flag the stick being picked up
                if (!currentById.TryGetValue(id, out var cur)) continue; // collected mid-attempt
                var b = kvp.Value;
                var c = new StickSnapshot { stickId = id, position = cur.position, rotation = cur.rotation };
                if (IsMoved(in b, in c, posThreshold, angleThresholdDeg))
                    moved.Add(id);
            }
            return moved;
        }
    }

    /// <summary>Minimal pose carrier so callers don't need to build full snapshots.</summary>
    public struct StickRecordPose
    {
        public Vector3 position;
        public Quaternion rotation;
    }
}

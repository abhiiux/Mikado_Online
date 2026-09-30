using System.Collections.Generic;
using UnityEngine;

namespace Mikado.Core
{
    /// <summary>
    /// Time-based debounce: a stick only counts as illegally moved once it has stayed
    /// over-threshold for sustainSeconds continuously. Pure (caller passes nowTime),
    /// so EditMode-testable and independent of FixedUpdate tick rate.
    /// Replaces the old tick-count debounce (debounceTicks at 50Hz ~= 0.1s).
    /// </summary>
    public class SustainedMovementTracker
    {
        private readonly Dictionary<int, float> firstOverTime = new Dictionary<int, float>();
        private readonly HashSet<int> confirmed = new HashSet<int>();

        [SerializeField] private float sustainSeconds = 0.1f;

        public SustainedMovementTracker(float sustainSeconds = 0.1f)
        {
            this.sustainSeconds = Mathf.Max(0f, sustainSeconds);
        }

        public void SetSustain(float seconds) => sustainSeconds = Mathf.Max(0f, seconds);

        public void Clear()
        {
            firstOverTime.Clear();
            confirmed.Clear();
        }

        public void Remove(int stickId)
        {
            firstOverTime.Remove(stickId);
            confirmed.Remove(stickId);
        }

        /// <summary>
        /// Returns IDs that became confirmed on THIS call (fire-once semantics).
        /// Call once per physics tick with the current over-threshold set.
        /// </summary>
        public List<int> Update(IEnumerable<int> movedIds, float nowTime)
        {
            var movedSet = movedIds is HashSet<int> hs ? hs : new HashSet<int>(movedIds);
            var newlyConfirmed = new List<int>();

            foreach (int id in movedSet)
            {
                if (confirmed.Contains(id)) continue;
                if (!firstOverTime.TryGetValue(id, out float first))
                {
                    firstOverTime[id] = nowTime;
                    first = nowTime;
                }
                if (nowTime - first >= sustainSeconds)
                {
                    confirmed.Add(id);
                    newlyConfirmed.Add(id);
                }
            }

            // Reset streak the moment a stick drops back under threshold.
            var toReset = new List<int>();
            foreach (int id in firstOverTime.Keys)
            {
                if (!movedSet.Contains(id) && !confirmed.Contains(id))
                    toReset.Add(id);
            }
            foreach (int id in toReset)
                firstOverTime.Remove(id);

            return newlyConfirmed;
        }
    }
}

using System;
using UnityEngine;

namespace Mikado.Core
{
    /// <summary>
    /// CENTRAL EVENT BUS: The single point of contact for all game-wide communication[cite: 7].
    /// Uses native Unity runtime initialization to guarantee zero race conditions during scene boot.
    /// </summary>
    public static class GameEventBus
    {
        public static event Action<Transform> OnTargetChange;
        public static void TriggerTargetChange(Transform transform) => OnTargetChange?.Invoke(transform);
        public static event Action<GameObject> OnTargetCollisionDetected;
        public static void TriggerMovementDetected(GameObject Obj) => OnTargetCollisionDetected?.Invoke(Obj);

        public static event Action<int, bool> OnCollision;
        public static void TriggerCollision(int i, bool state) => OnCollision?.Invoke(i, state);
        public static event Action<Renderer, bool> OnStickSelected;
        public static void TriggerSelection(Renderer renderer,bool state) => OnStickSelected?.Invoke(renderer, state);


        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        // public static void ResetAllListeners()
        // {
        // }
    }
}


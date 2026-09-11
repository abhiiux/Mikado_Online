using System;
using UnityEngine;

/// <summary>
/// CENTRAL EVENT BUS: The single point of contact for all game-wide communication[cite: 7].
/// Uses native Unity runtime initialization to guarantee zero race conditions during scene boot.
/// </summary>
public static class GameEventBus
{

    public static event Action<Transform> OnTargetChange;
    public static void TriggerTargetChange(Transform transform) => OnTargetChange?.Invoke(transform);

    public static event Action<int,bool> OnCollision;
    public static void TriggerCollision(int i, bool state) => OnCollision?.Invoke(i, state);
    // private static void ResetCachedState()
    // {

    // }

    // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    // public static void ResetAllListeners()
    // {

    // }
}


using System;
using UnityEngine;

public static class GameEventManager 
{
    public static event Action StickSpawner;

    public static void OnStickSpawnerTriggered() => StickSpawner?.Invoke();
}

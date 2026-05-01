using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] Transform stickParent;
    [SerializeField] GameObject stickPrefab;
    [SerializeField] int stickCount;


    private void OnEnable() => GameEventManager.StickSpawner += SpawnSticks;
    private void OnDisable() => GameEventManager.StickSpawner -= SpawnSticks;


    private void SpawnSticks()
    {
        for (int i = 0; i < stickCount; i++)
        {
            Instantiate(stickPrefab, stickParent.position, Quaternion.identity);
        }
    }
    
}

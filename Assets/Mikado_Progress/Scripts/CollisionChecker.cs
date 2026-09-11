using System;
using UnityEngine;

public class CollisionChecker : MonoBehaviour
{

    void OnCollisionEnter(Collision collision)
    {
        ObjectPoints obj = collision.gameObject.GetComponent<ObjectPoints>();

        if(obj.isTarget)
        {
            GameEventBus.TriggerTargetChange(null);   
        }

        if (obj.isFlagged)
        {
            int score = obj.Points;
            GameEventBus.TriggerCollision(score, false);
            TakeThis(collision.gameObject);
        }
        else
        {
            int score = obj.Points;
            GameEventBus.TriggerCollision(score, true);
            TakeThis(collision.gameObject);
        }
    }
    public void TakeThis(GameObject obj)
    {
        obj.SetActive(false);
    }

}

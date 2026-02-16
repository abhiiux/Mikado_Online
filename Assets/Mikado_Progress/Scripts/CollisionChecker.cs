using System;
using UnityEngine;

public class CollisionChecker : MonoBehaviour
{
    public static event Action<int,bool> OnCollision;


    void OnCollisionEnter(Collision collision)
    {
        ObjectPoints obj = collision.gameObject.GetComponent<ObjectPoints>();

        if (obj.isFlagged)
        {
            int score = obj.Points;
            OnCollision(score, false);
            TakeThis(collision.gameObject);
        }
        else
        {
            int score = obj.Points;
            OnCollision(score, true);
            TakeThis(collision.gameObject);
        }
    }
    public void TakeThis(GameObject obj)
    {
        obj.SetActive(false);
    }

}

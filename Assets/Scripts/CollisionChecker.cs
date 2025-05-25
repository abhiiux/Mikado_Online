using System;
using UnityEngine;

public class CollisionChecker : MonoBehaviour
{
    public static event Action<int,string> OnCollision;


    void OnCollisionEnter(Collision collision)
    {
        ObjectPoints obj = collision.gameObject.GetComponent<ObjectPoints>();

        int score = obj.Points;
        OnCollision(score, collision.gameObject.name);
        TakeThis(collision.gameObject);
    }
    public void TakeThis(GameObject obj)
    {
        obj.SetActive(false);
    }

}

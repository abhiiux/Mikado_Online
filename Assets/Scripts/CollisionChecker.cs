using System;
using UnityEngine;

public class CollisionChecker : MonoBehaviour
{
    public static event Action<int> OnCollision;
    

    void OnCollisionEnter(Collision collision)
    {
        string tag = collision.gameObject.tag;

        switch (tag)
        {
            case "Red":
                OnCollision(10);
                TakeThis(collision.gameObject);
                break;

            case "Green":
                OnCollision(5);
                TakeThis(collision.gameObject);
                break;

            case "White":
                OnCollision(15);
                TakeThis(collision.gameObject);
                break;

            case "Yellow":
                OnCollision(1);
                TakeThis(collision.gameObject);
                break;

            default:
                OnCollision(0);
                TakeThis(collision.gameObject);
                break;
        }
    }
    public void TakeThis(GameObject obj)
    {
        obj.SetActive(false);
    }

}

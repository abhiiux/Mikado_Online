using System;
using UnityEngine;

public class CollisionChecker : MonoBehaviour
{
    public static event Action<int,string> OnCollision;
    

    void OnCollisionEnter(Collision collision)
    {
        string tag = collision.gameObject.tag.ToLower();

        switch (tag)
        {
            case "red":
                OnCollision(10,"red");
                TakeThis(collision.gameObject);
                break;

            case "green":
                OnCollision(5,"green");
                TakeThis(collision.gameObject);
                break;

            case "white":
                OnCollision(15,"white");
                TakeThis(collision.gameObject);
                break;

            case "yellow":
                OnCollision(1,"yellow");
                TakeThis(collision.gameObject);
                break;

            case "finish":
                OnCollision(0,"defaultValue");
                TakeThis(collision.gameObject);
                break;
        }
    }
    public void TakeThis(GameObject obj)
    {
        obj.SetActive(false);
    }

}

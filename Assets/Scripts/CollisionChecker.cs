using UnityEngine;

public class CollisionChecker : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bin"))
        {
            TakeThis(this.gameObject);
        }
    }
    public void TakeThis(GameObject obj)
    {
        obj.SetActive(false);
    }

}

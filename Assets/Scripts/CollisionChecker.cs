using UnityEngine;

public class CollisionChecker : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        // IBin destroy = collision.gameObject.GetComponent<IBin>();
        // destroy.TakeThis(this.gameObject);

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

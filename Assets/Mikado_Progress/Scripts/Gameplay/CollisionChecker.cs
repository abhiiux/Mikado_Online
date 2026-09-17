using System;
using Mikado.Core;
using UnityEngine;

namespace Mikado.Gameplay
{
    public class CollisionChecker : MonoBehaviour
    {
        void OnTriggerEnter(Collider other)
        {
            ObjectPoints obj = other.gameObject.GetComponent<ObjectPoints>();

            if(obj.isTarget)
            {
                GameEventBus.TriggerTargetChange(null);
                GameEventBus.TriggerCollisionDetected(other.gameObject);
                obj.isFlagged = true;
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            ObjectPoints obj = collision.gameObject.GetComponent<ObjectPoints>();        

            if(obj.isFlagged)
            {
                TakeThis(obj.gameObject);
            }
            else
            {
                obj.isFlagged = true;
            }

            // if (obj.isFlagged)
            // {
            //     int score = obj.Points;
            //     GameEventBus.TriggerCollision(score, false);
            //     TakeThis(collision.gameObject);
            // }
            // else
            // {
            //     int score = obj.Points;
            //     GameEventBus.TriggerCollision(score, true);
            //     TakeThis(collision.gameObject);
            // }
        }
        public void TakeThis(GameObject obj)
        {
            obj.SetActive(false);
        }

    }
}

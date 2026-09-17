using System.Collections.Generic;
using Mikado.Core;
using UnityEngine;

namespace Mikado.Gameplay
{
    public class CreateSticks : MonoBehaviour
    {
        [SerializeField] private int noOfSticks; 
        [SerializeField] private StickCounterValue stickCounterValue;
        [SerializeField] private GameObject prefabStick; 

        [Header("Circle Settings")]
        [SerializeField] float radius = 1f; 
        [SerializeField] float radiusVariance = 1f; 
        [SerializeField] bool sortOnStart = true;
        [SerializeField] bool randomRotation = true;
        [SerializeField] float minRotation = 0f;     
        [SerializeField] float maxRotation = 100f;   

        [Header("Per Stick Settings")]
        [SerializeField] float heightVariance = 1f; 
        [SerializeField] float tiltVariance = 1f; 
        
        private Transform parentObject;
        private List<Transform> childrens = new List<Transform>();
        private StickCheck stickCheck;

        void Start()
        {
            parentObject = GetComponent<Transform>();
            stickCheck = GetComponent<StickCheck>();
            
            noOfSticks = stickCounterValue.GetStickCount();
            CreateSticksOnCall();
        }

        private void CreateSticksOnCall()
        {
            for (int i = 0; i < noOfSticks; i++)
            {
                GameObject t =  Instantiate( prefabStick, parentObject );
                childrens.Add( t.transform );
            }

            ArrangeInCircle();

            stickCheck.Init( childrens );
        }

        public void ArrangeInCircle()
        {
            var sortedChildren = childrens;

            float angleStep = 360f / sortedChildren.Count;
            float startAngle = randomRotation ? Random.Range(minRotation, maxRotation) : 0f;

            for (int i = 0; i < sortedChildren.Count; i++)
            {
                float angle = (i * angleStep + startAngle) * Mathf.Deg2Rad;

                // small per-stick jitter so they don't land in a perfect ring
                float radiusJitter = Random.Range(-radiusVariance, radiusVariance);
                float heightJitter = Random.Range(0f, heightVariance);

                Vector3 newPosition = new Vector3(
                    Mathf.Cos(angle) * (radius + radiusJitter),
                    sortedChildren[i].localPosition.y + heightJitter,
                    Mathf.Sin(angle) * (radius + radiusJitter)
                );

                sortedChildren[i].position = transform.TransformPoint(newPosition);

                // each stick gets its own random facing before falling
                if (randomRotation)
                {
                    float stickYaw = Random.Range(0f, 360f);
                    float stickTilt = Random.Range(-tiltVariance, tiltVariance); // slight lean, optional
                    sortedChildren[i].rotation = Quaternion.Euler(stickTilt, stickYaw, stickTilt);
                }
            }
        }
    }
}

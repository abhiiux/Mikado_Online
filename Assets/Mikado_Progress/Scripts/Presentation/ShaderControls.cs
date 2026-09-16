using System.Collections;
using System.Collections.Generic;
using Mikado.Core;
using UnityEngine;

namespace Mikado.Presentation
{
    public class ShaderControls : MonoBehaviour
    {
        private string outLineTogglePropery = "_OutlineWidth";
        private MaterialPropertyBlock propertyBlock;

        void OnEnable()
        {
            propertyBlock = new MaterialPropertyBlock();     

            GameEventBus.OnStickSelected +=  ToggleSelectionState;
        }
        void OnDisable()
        {
            GameEventBus.OnStickSelected -=  ToggleSelectionState;
        }

        public void ToggleSelectionState(Renderer stick, bool state)
        {
            stick.GetPropertyBlock(propertyBlock);

            float value = propertyBlock.GetFloat(outLineTogglePropery);
            propertyBlock.SetFloat(outLineTogglePropery, state? 1f : 0f);

            stick.SetPropertyBlock(propertyBlock);
        }
        

        public void DamageGlow(List<GameObject> value)
        {
            StartCoroutine(StartGlow(value));
        }
        
        private IEnumerator StartGlow(List<GameObject> sticks)
        {
            foreach (GameObject item in sticks)
            {
                Renderer renderer = item.GetComponent<Renderer>();
                renderer.material.SetFloat("_damageColor", 1f);
                renderer.material.SetFloat("_blinkRate", 15f);
            }

            yield return new WaitForSeconds(2f);

            foreach (GameObject item in sticks)
            {
                Renderer renderer = item.GetComponent<Renderer>();
                renderer.material.SetFloat("_damageColor", 0f);
                renderer.material.SetFloat("_blinkRate", 0f);
            }
        }

    }
}

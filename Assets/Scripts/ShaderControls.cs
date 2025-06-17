using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderControls : MonoBehaviour
{
    public void SelectionGlow(Renderer stick)
    {
        stick.material.SetFloat("_selectionColor", 1f);
        stick.material.SetFloat("_blinkRate", 5f);
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

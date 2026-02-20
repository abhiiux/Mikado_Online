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
    public void SelectionOutline(Renderer stick, float value)
    {
        stick.material.SetFloat("_OutlineWidth", value);
    }

    public void SelectionOutlineColor(Renderer stick, Color color)
    {
        Material mat = stick.material;
        if (mat.HasProperty("_OutlineColor"))
        {
            mat.SetColor("_OutlineColor", color);
            return;
        }

        if (mat.HasProperty("_RimColor"))
        {
            mat.SetColor("_RimColor", color);
            return;
        }

        if (mat.HasProperty("_EdgeColor"))
        {
            mat.SetColor("_EdgeColor", color);
        }
    }

    public bool TryGetOutlineColor(Renderer stick, out Color color)
    {
        Material mat = stick.material;
        if (mat.HasProperty("_OutlineColor"))
        {
            color = mat.GetColor("_OutlineColor");
            return true;
        }

        if (mat.HasProperty("_RimColor"))
        {
            color = mat.GetColor("_RimColor");
            return true;
        }

        if (mat.HasProperty("_EdgeColor"))
        {
            color = mat.GetColor("_EdgeColor");
            return true;
        }

        color = Color.white;
        return false;
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

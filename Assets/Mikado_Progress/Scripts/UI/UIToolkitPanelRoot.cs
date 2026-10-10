using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mikado.UI
{
    internal static class UIToolkitPanelRoot
    {
        internal static VisualElement GetPanelRoot(Component source)
        {
            PanelRenderer renderer = source.GetComponent<PanelRenderer>();
            if (renderer == null)
                return null;

            PropertyInfo rootProperty = typeof(PanelRenderer).GetProperty(
                "rootVisualElement",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return rootProperty?.GetValue(renderer) as VisualElement;
        }
    }
}

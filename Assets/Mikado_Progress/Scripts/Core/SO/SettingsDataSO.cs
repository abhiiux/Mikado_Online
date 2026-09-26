using UnityEngine;

namespace Mikado
{
    [CreateAssetMenu(fileName = "SettingsDataSO", menuName = "Scriptable Objects/SettingsDataSO")]
    public class SettingsDataSO : ScriptableObject
    {
        [Header("Input")]
        [Tooltip("When off, mobile controls (pick-up button, touch camera) are disabled and zoom slider is hidden.")]
        public bool mobileInputEnabled = true;

        [Header("Camera Sensitivity (multipliers)")]
        [Range(0.1f, 3f)] public float rotationSensitivity = 1f;
        [Range(0.1f, 3f)] public float zoomSensitivity = 1f;
        [Range(0.1f, 3f)] public float heightSensitivity = 1f;

        private void OnValidate()
        {
            rotationSensitivity = Mathf.Clamp(rotationSensitivity, 0.1f, 3f);
            zoomSensitivity = Mathf.Clamp(zoomSensitivity, 0.1f, 3f);
            heightSensitivity = Mathf.Clamp(heightSensitivity, 0.1f, 3f);
        }
    }
}

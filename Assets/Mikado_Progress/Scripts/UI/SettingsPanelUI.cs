using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mikado;

namespace Mikado.UI
{
    /// <summary>
    /// MainMenu-only settings panel. Buffered until Apply.
    /// Hierarchy (screenshot):
    ///   SettingsPanel/Content
    ///     MobileInputToggle/Toggle (with Background/Label)
    ///     Zoom Sensitivity Slider/TextParent/Text (TMP) + Slider
    ///     Rotation Sensitivity Slider/TextParent/Text (TMP) + Slider
    ///     Height Sensitivity Slider/TextParent/Text (TMP) + Slider
    /// Plus Apply and Close buttons (footer).
    /// Decoupled: only mutates SettingsDataSO in-memory; Gameplay CameraRotation reads SO directly.
    /// </summary>
    public class SettingsPanelUI : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private SettingsDataSO settingsData;

        [Header("Panel Root")]
        [Tooltip("SettingsPanel GO (or Content). Toggled by Show/Hide.")]
        [SerializeField] private GameObject panelRoot;

        [Header("Mobile Input Toggle")]
        [Tooltip("SettingsPanel/Content/MobileInputToggle/Toggle")]
        [SerializeField] private Toggle mobileToggle;

        [Header("Sensitivity - Sliders")]
        [Tooltip("SettingsPanel/Content/Zoom Sensitivity Slider/Slider")]
        [SerializeField] private Slider zoomSlider;
        [Tooltip("SettingsPanel/Content/Rotation Sensitivity Slider/Slider")]
        [SerializeField] private Slider rotationSlider;
        [Tooltip("SettingsPanel/Content/Height Sensitivity Slider/Slider")]
        [SerializeField] private Slider heightSlider;

        [Header("Sensitivity - Value Labels")]
        [Tooltip("SettingsPanel/Content/Zoom Sensitivity Slider/TextParent/Text (TMP)")]
        [SerializeField] private TMP_Text zoomValue;
        [Tooltip("SettingsPanel/Content/Rotation Sensitivity Slider/TextParent/Text (TMP)")]
        [SerializeField] private TMP_Text rotationValue;
        [Tooltip("SettingsPanel/Content/Height Sensitivity Slider/TextParent/Text (TMP)")]
        [SerializeField] private TMP_Text heightValue;

        [Header("Actions")]
        [SerializeField] private Button applyButton;
        [SerializeField] private Button closeButton;

        // Pending buffer — applied to SO only on Apply
        private bool pendingMobile;
        private float pendingZoom;
        private float pendingRotation;
        private float pendingHeight;
        private bool hasPending;
        private bool suppressEvents;

        private void OnEnable()
        {
            Bind();
            LoadFromSO();
        }

        private void OnDisable()
        {
            Unbind();
        }

        private void Bind()
        {
            if (mobileToggle != null)
                mobileToggle.onValueChanged.AddListener(OnMobileToggled);
            if (zoomSlider != null)
                zoomSlider.onValueChanged.AddListener(OnZoomPending);
            if (rotationSlider != null)
                rotationSlider.onValueChanged.AddListener(OnRotationPending);
            if (heightSlider != null)
                heightSlider.onValueChanged.AddListener(OnHeightPending);
            if (applyButton != null)
                applyButton.onClick.AddListener(OnApply);
            if (closeButton != null)
                closeButton.onClick.AddListener(OnClose);
        }

        private void Unbind()
        {
            if (mobileToggle != null)
                mobileToggle.onValueChanged.RemoveListener(OnMobileToggled);
            if (zoomSlider != null)
                zoomSlider.onValueChanged.RemoveListener(OnZoomPending);
            if (rotationSlider != null)
                rotationSlider.onValueChanged.RemoveListener(OnRotationPending);
            if (heightSlider != null)
                heightSlider.onValueChanged.RemoveListener(OnHeightPending);
            if (applyButton != null)
                applyButton.onClick.RemoveListener(OnApply);
            if (closeButton != null)
                closeButton.onClick.RemoveListener(OnClose);
        }

        private void LoadFromSO()
        {
            if (settingsData == null) return;

            suppressEvents = true;

            pendingMobile = settingsData.mobileInputEnabled;
            pendingZoom = settingsData.zoomSensitivity;
            pendingRotation = settingsData.rotationSensitivity;
            pendingHeight = settingsData.heightSensitivity;

            if (mobileToggle != null)
                mobileToggle.SetIsOnWithoutNotify(pendingMobile);

            if (zoomSlider != null)
            {
                zoomSlider.minValue = 0.1f;
                zoomSlider.maxValue = 3f;
                zoomSlider.wholeNumbers = false;
                zoomSlider.SetValueWithoutNotify(pendingZoom);
            }
            if (rotationSlider != null)
            {
                rotationSlider.minValue = 0.1f;
                rotationSlider.maxValue = 3f;
                rotationSlider.wholeNumbers = false;
                rotationSlider.SetValueWithoutNotify(pendingRotation);
            }
            if (heightSlider != null)
            {
                heightSlider.minValue = 0.1f;
                heightSlider.maxValue = 3f;
                heightSlider.wholeNumbers = false;
                heightSlider.SetValueWithoutNotify(pendingHeight);
            }

            RefreshLabels();
            hasPending = false;
            SetApplyInteractable(false);

            suppressEvents = false;
        }

        private void RefreshLabels()
        {
            if (zoomValue != null) zoomValue.text = $"{pendingZoom:F1}x";
            if (rotationValue != null) rotationValue.text = $"{pendingRotation:F1}x";
            if (heightValue != null) heightValue.text = $"{pendingHeight:F1}x";
        }

        private void SetApplyInteractable(bool state)
        {
            if (applyButton != null)
                applyButton.interactable = state;
        }

        private void OnMobileToggled(bool value)
        {
            if (suppressEvents) return;
            pendingMobile = value;
            hasPending = true;
            SetApplyInteractable(true);
        }

        private void OnZoomPending(float value)
        {
            if (suppressEvents) return;
            pendingZoom = Mathf.Clamp(value, 0.1f, 3f);
            if (zoomValue != null) zoomValue.text = $"{pendingZoom:F1}x";
            hasPending = true;
            SetApplyInteractable(true);
        }

        private void OnRotationPending(float value)
        {
            if (suppressEvents) return;
            pendingRotation = Mathf.Clamp(value, 0.1f, 3f);
            if (rotationValue != null) rotationValue.text = $"{pendingRotation:F1}x";
            hasPending = true;
            SetApplyInteractable(true);
        }

        private void OnHeightPending(float value)
        {
            if (suppressEvents) return;
            pendingHeight = Mathf.Clamp(value, 0.1f, 3f);
            if (heightValue != null) heightValue.text = $"{pendingHeight:F1}x";
            hasPending = true;
            SetApplyInteractable(true);
        }

        private void OnApply()
        {
            if (settingsData == null) return;

            // Pending → SO in-memory (no PlayerPrefs, no SetDirty)
            settingsData.mobileInputEnabled = pendingMobile;
            settingsData.zoomSensitivity = pendingZoom;
            settingsData.rotationSensitivity = pendingRotation;
            settingsData.heightSensitivity = pendingHeight;

            hasPending = false;
            SetApplyInteractable(false);
            HidePanel();
        }

        private void OnClose()
        {
            // Discard pending changes
            if (hasPending)
                LoadFromSO();

            HidePanel();
        }

        public void ShowPanel()
        {
            var root = panelRoot != null ? panelRoot : gameObject;
            root.SetActive(true);
            LoadFromSO();
        }

        public void HidePanel()
        {
            var root = panelRoot != null ? panelRoot : gameObject;
            root.SetActive(false);
        }

        public void TogglePanel()
        {
            var root = panelRoot != null ? panelRoot : gameObject;
            if (root.activeSelf)
                OnClose();
            else
                ShowPanel();
        }
    }
}

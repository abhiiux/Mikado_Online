using Mikado.Core;
using Mikado.Presentation;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mikado.UI
{
    // [RequireComponent(typeof(UIDocument))]
    public sealed class MainMenuUIToolkit : MonoBehaviour
    {
        [SerializeField] private StickCounterValue counterData;
        [SerializeField] private SettingsDataSO settingsData;
        [SerializeField] private GameObject legacyUGUI;

        private Label counterLabel;
        private Button decreaseButton;
        private Button increaseButton;
        private VisualElement settingsPanel;
        private Button applyButton;
        private Toggle mobileToggle;
        private Toggle cameraFollowToggle;
        private Slider zoomSlider;
        private Slider rotationSlider;
        private Slider heightSlider;
        private Label zoomValue;
        private Label rotationValue;
        private Label heightValue;
        private int stickCount;

        private void OnEnable()
        {
            VisualElement root = UIToolkitPanelRoot.GetPanelRoot(this);
            if (root == null)
                return;

            counterLabel = root.Q<Label>("counter-value");

            settingsPanel = root.Q<VisualElement>("settings-panel");
            applyButton = root.Q<Button>("apply-button");
            mobileToggle = root.Q<Toggle>("mobile-toggle");
            cameraFollowToggle = root.Q<Toggle>("camera-follow-toggle");
            zoomSlider = root.Q<Slider>("zoom-slider");
            rotationSlider = root.Q<Slider>("rotation-slider");
            heightSlider = root.Q<Slider>("height-slider");
            zoomValue = root.Q<Label>("zoom-value");
            rotationValue = root.Q<Label>("rotation-value");
            heightValue = root.Q<Label>("height-value");

            root.Q<Button>("play-button").clicked += Play;
            root.Q<Button>("quit-button").clicked += Quit;
            root.Q<Button>("settings-button").clicked += ShowSettings;
            root.Q<Button>("close-button").clicked += CloseSettings;

            applyButton.clicked += ApplySettings;
            mobileToggle.RegisterValueChangedCallback(OnSettingChanged);
            cameraFollowToggle.RegisterValueChangedCallback(OnSettingChanged);
            zoomSlider.RegisterValueChangedCallback(OnSliderChanged);
            rotationSlider.RegisterValueChangedCallback(OnSliderChanged);
            heightSlider.RegisterValueChangedCallback(OnSliderChanged);

            if (legacyUGUI != null)
                legacyUGUI.SetActive(false);

            stickCount = counterData != null ? counterData.MaxStickCount : 0;
            // RefreshCounter();
            LoadSettings();
            settingsPanel.style.display = DisplayStyle.None;
        }

        private void Play()
        {
            if (counterData != null)
                counterData.SetStickCount(stickCount);
            if (SceneLoader.Instance != null)
                SceneLoader.Instance.LoadNextScene();
        }

        private static void Quit() => Application.Quit();

        private void DecreaseCounter()
        {
            if (counterData == null) return;
            stickCount = Mathf.Max(counterData.MinStickCount, stickCount - counterData.IncrementalValue);
            RefreshCounter();
        }

        private void IncreaseCounter()
        {
            if (counterData == null) return;
            stickCount = Mathf.Min(counterData.MaxStickCount, stickCount + counterData.IncrementalValue);
            RefreshCounter();
        }

        private void RefreshCounter()
        {
            counterLabel.text = stickCount.ToString();
            if (counterData == null) return;
            decreaseButton.SetEnabled(stickCount > counterData.MinStickCount);
            increaseButton.SetEnabled(stickCount < counterData.MaxStickCount);
        }

        private void ShowSettings()
        {
            LoadSettings();
            settingsPanel.style.display = DisplayStyle.Flex;
        }

        private void CloseSettings()
        {
            LoadSettings();
            settingsPanel.style.display = DisplayStyle.None;
        }

        private void LoadSettings()
        {
            if (settingsData == null) return;
            mobileToggle.SetValueWithoutNotify(settingsData.mobileInputEnabled);
            cameraFollowToggle.SetValueWithoutNotify(settingsData.moveCameraToTarget);
            zoomSlider.SetValueWithoutNotify(settingsData.zoomSensitivity);
            rotationSlider.SetValueWithoutNotify(settingsData.rotationSensitivity);
            heightSlider.SetValueWithoutNotify(settingsData.heightSensitivity);
            RefreshSettingLabels();
            applyButton.SetEnabled(false);
        }

        private void ApplySettings()
        {
            if (settingsData == null) return;
            settingsData.mobileInputEnabled = mobileToggle.value;
            settingsData.moveCameraToTarget = cameraFollowToggle.value;
            settingsData.zoomSensitivity = zoomSlider.value;
            settingsData.rotationSensitivity = rotationSlider.value;
            settingsData.heightSensitivity = heightSlider.value;
            applyButton.SetEnabled(false);
            settingsPanel.style.display = DisplayStyle.None;
        }

        private void OnSettingChanged(ChangeEvent<bool> evt) => applyButton.SetEnabled(true);

        private void OnSliderChanged(ChangeEvent<float> evt)
        {
            RefreshSettingLabels();
            applyButton.SetEnabled(true);
        }

        private void RefreshSettingLabels()
        {
            zoomValue.text = $"{zoomSlider.value:F1}x";
            rotationValue.text = $"{rotationSlider.value:F1}x";
            heightValue.text = $"{heightSlider.value:F1}x";
        }
    }
}

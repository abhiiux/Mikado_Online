using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mikado.UI
{
    // [RequireComponent(typeof(UIDocument))]
    public sealed class UIToolkitButtonInteractions : MonoBehaviour
    {
        [SerializeField] private float hoverOffsetX = -12f;
        [SerializeField] private float hoverScale = 1.03f;
        [SerializeField] private float pressedScale = 0.97f;
        [SerializeField, Range(0f, 1f)] private float pressedOpacity = 0.82f;
        [SerializeField] private string[] scopeElementNames = new string[0];

        // Matches HoverMove's smoothTime so the slide feels the same as the old uGUI buttons.
        private const float hoverMoveSmoothTime = 0.15f;
        private const float pressFeedbackDuration = 0.08f;

        private readonly List<Button> registeredButtons = new();


        private void OnEnable()
        {
            VisualElement root = UIToolkitPanelRoot.GetPanelRoot(this);
            if (root == null)
                return;

            foreach (Button button in QueryScopedButtons(root))
            {
                Register(button);
            }
        }

        private IEnumerable<Button> QueryScopedButtons(VisualElement root)
        {
            if (scopeElementNames == null || scopeElementNames.Length == 0)
                return root.Query<Button>().ToList();

            List<Button> scopedButtons = new();
            foreach (string elementName in scopeElementNames)
            {
                if (string.IsNullOrWhiteSpace(elementName))
                    continue;

                VisualElement container = root.Q<VisualElement>(elementName);
                if (container == null)
                {
                    Debug.LogWarning(
                        $"{nameof(UIToolkitButtonInteractions)}: no VisualElement named '{elementName}' found.",
                        this);
                    continue;
                }

                foreach (Button button in container.Query<Button>().ToList())
                {
                    if (!scopedButtons.Contains(button))
                        scopedButtons.Add(button);
                }
            }

            return scopedButtons;
        }

        private void OnDisable()
        {
            foreach (Button button in registeredButtons)
                Unregister(button);

            registeredButtons.Clear();
        }

        private void Register(Button button)
        {
            Reset(button);
            ApplyTransitions(button);
            button.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            button.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            button.RegisterCallback<PointerDownEvent>(OnPointerDown);
            button.RegisterCallback<PointerUpEvent>(OnPointerUp);
            button.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
            registeredButtons.Add(button);
        }

        private void Unregister(Button button)
        {
            button.UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
            button.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
            button.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            button.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            button.UnregisterCallback<PointerCancelEvent>(OnPointerCancel);
            Reset(button);
            ClearTransitions(button);
        }

        private static void ApplyTransitions(Button button)
        {
            button.style.transitionProperty = new List<StylePropertyName>
            {
                new("translate"),
                new("scale"),
                new("opacity")
            };
            button.style.transitionDuration = new List<TimeValue>
            {
                new(hoverMoveSmoothTime, TimeUnit.Second),
                new(pressFeedbackDuration, TimeUnit.Second),
                new(pressFeedbackDuration, TimeUnit.Second)
            };
            button.style.transitionTimingFunction = new List<EasingFunction>
            {
                new(EasingMode.EaseInOut),
                new(EasingMode.EaseOut),
                new(EasingMode.EaseOut)
            };
        }

        private static void ClearTransitions(Button button)
        {
            button.style.transitionProperty = new List<StylePropertyName>();
            button.style.transitionDuration = new List<TimeValue>();
            button.style.transitionTimingFunction = new List<EasingFunction>();
        }

        private void OnPointerEnter(PointerEnterEvent evt)
        {
            if (evt.currentTarget is not Button button) return;
            button.style.translate = new Translate(hoverOffsetX, 0);
            button.style.scale = new Scale(Vector2.one * hoverScale);
        }

        private void OnPointerLeave(PointerLeaveEvent evt)
        {
            if (evt.currentTarget is Button button)
                Reset(button);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.currentTarget is not Button button) return;
            button.style.scale = new Scale(Vector2.one * pressedScale);
            button.style.opacity = pressedOpacity;
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (evt.currentTarget is not Button button) return;
            button.style.scale = new Scale(Vector2.one * hoverScale);
            button.style.opacity = 1f;
        }

        private void OnPointerCancel(PointerCancelEvent evt)
        {
            if (evt.currentTarget is Button button)
                Reset(button);
        }

        private static void Reset(Button button)
        {
            button.style.translate = new Translate(0, 0);
            button.style.scale = new Scale(Vector2.one);
            button.style.opacity = 1f;
        }
    }
}

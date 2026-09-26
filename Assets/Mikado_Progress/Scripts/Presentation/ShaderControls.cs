using System.Collections;
using Mikado.Core;
using UnityEngine;

namespace Mikado.Presentation
{
    public class ShaderControls : MonoBehaviour
    {
        private static readonly int OutLineColorProperty =
            Shader.PropertyToID("_OutlineColor");

        private static readonly int OutlineToggleProperty =
            Shader.PropertyToID("_OutlineWidth");

        private MaterialPropertyBlock propertyBlock;

        private void OnEnable()
        {
            propertyBlock = new MaterialPropertyBlock();

            GameEventBus.OnStickSelected += ToggleSelectionState;
            GameEventBus.OnStickMovementDetected += DamageGlow;
        }

        private void OnDisable()
        {
            GameEventBus.OnStickSelected -= ToggleSelectionState;
            GameEventBus.OnStickMovementDetected -= DamageGlow;
        }

        public void ToggleSelectionState(Renderer stick, bool state)
        {
            stick.GetPropertyBlock(propertyBlock);

            propertyBlock.SetFloat(
                OutlineToggleProperty,
                state ? 1f : 0f
            );

            stick.SetPropertyBlock(propertyBlock);
        }

        public void DamageGlow(Renderer stick)
        {
            StartCoroutine(StartGlow(stick));
        }

        private IEnumerator StartGlow(Renderer stick)
        {
            stick.GetPropertyBlock(propertyBlock);

            // Damage state
            propertyBlock.SetColor(OutLineColorProperty, Color.red);
            propertyBlock.SetFloat(OutlineToggleProperty, 1f);
            stick.SetPropertyBlock(propertyBlock);

            yield return new WaitForSeconds(2f);

            // Normal state
            propertyBlock.SetColor(OutLineColorProperty, Color.white);
            propertyBlock.SetFloat(OutlineToggleProperty, 0f);
            stick.SetPropertyBlock(propertyBlock);
        }
    }
}
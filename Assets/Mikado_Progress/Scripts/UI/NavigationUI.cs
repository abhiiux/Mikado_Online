using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Mikado.Presentation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Mikado.UI
{
    /// <summary>
    /// Scene navigation actions + vertical accordion corner menu.
    /// </summary>
    public class NavigationUI : MonoBehaviour
    {

        [Header("Corner Accordion Menu")]
        [Tooltip("Persistently visible toggle in the corner.")]
        [SerializeField] private Button toggleButton;
        [Tooltip("Child icon of the toggle; rotates 0 <-> -90 degrees.")]
        [SerializeField] private RectTransform toggleIcon;
        [Tooltip("Empty origin that Pause/Refresh/Back are positioned from.")]
        [SerializeField] private RectTransform buttonsHolder;
        [Tooltip("Full-screen invisible raycast target (Canvas-level sibling behind the menu). Collapses the menu when clicked.")]
        [SerializeField] private GameObject clickAwayBlocker;
        [Tooltip("In order: Pause, Refresh, Back.")]
        [SerializeField] private List<RectTransform> itemRects = new List<RectTransform>();
        [SerializeField] private List<CanvasGroup> itemGroups = new List<CanvasGroup>();

        [Header("Accordion Layout")]
        [SerializeField] private float buttonHeight = 120f;
        [SerializeField] private float spacing = 12f;
        [SerializeField] private float topOffset = 132f;
        [SerializeField] private float expandDuration = 0.22f;
        [SerializeField] private float staggerDelay = 0.04f;
        [SerializeField] private float collapsedScale = 0.5f;
        [Tooltip("Prefer DOTween; when false the unscaled-time coroutine path runs instead.")]
        [SerializeField] private bool useDotween = true;

        private bool isOpen;
        private int animVersion;
        private Sequence activeSeq;
        private Coroutine animRoutine;

        public bool IsOpen => isOpen;

        private bool paused;


        private void Awake()
        {
            ApplyCollapsedPoseImmediate();
        }

        private void OnDestroy()
        {
            animVersion++;
            if (activeSeq != null)
            {
                activeSeq.Kill(false);
                activeSeq = null;
            }
        }

#region  Button Actions
        public void Toggle()
        {
            if (isOpen) Close();
            else Open();

            Debug.Log($" hey  ");
        }

        public void PauseButton()
        {
            paused = !paused;
            Time.timeScale = paused ? 0f : 1f;
        }
        public void BackButton()
        {
            int index = SceneManager.GetActiveScene().buildIndex - 1;
            SceneLoader.Instance.LoadScene( index );
        }
        public void RestartButton()
        {
            Time.timeScale = 1f;
            int index = SceneManager.GetActiveScene().buildIndex;
            SceneLoader.Instance.LoadScene( index );
        }
        public void Open()
        {
            int version = BeginNewAnimation();
            isOpen = true;
            SetBlockerActive(true);

            if (useDotween) PlayOpenDotween(version);
        }

        public void Close()
        {
            int version = BeginNewAnimation();
            isOpen = false;

            // Cut raycasts instantly so collapsing buttons can't be misclicked.
            for (int i = 0; i < ItemCount(); i++)
            {
                var group = itemGroups[i];
                if (group == null) continue;
                group.blocksRaycasts = false;
                group.interactable = false;
            }

            if (useDotween) PlayCloseDotween(version);
        }
#endregion
        private float TargetY(int index) => -(topOffset + index * (buttonHeight + spacing));

        private int ItemCount()
        {
            if (itemRects == null || itemGroups == null) return 0;
            return Mathf.Min(itemRects.Count, itemGroups.Count);
        }

        private void SetBlockerActive(bool active)
        {
            if (clickAwayBlocker != null && clickAwayBlocker.activeSelf != active)
                clickAwayBlocker.SetActive(active);
        }

        /// <summary>Kills any running animation and returns the new version token.</summary>
        private int BeginNewAnimation()
        {
            animVersion++;
            if (activeSeq != null)
            {
                activeSeq.Kill(false);
                activeSeq = null;
            }
            if (animRoutine != null)
            {
                StopCoroutine(animRoutine);
                animRoutine = null;
            }
            return animVersion;
        }

        private void ApplyCollapsedPoseImmediate()
        {
            BeginNewAnimation();
            isOpen = false;
            if (toggleIcon != null)
                toggleIcon.localEulerAngles = Vector3.zero;
            for (int i = 0; i < ItemCount(); i++)
            {
                var rect = itemRects[i];
                var group = itemGroups[i];
                if (rect != null)
                {
                    rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, 0f);
                    rect.localScale = Vector3.one * collapsedScale;
                }
                if (group != null)
                {
                    group.alpha = 0f;
                    group.blocksRaycasts = false;
                    group.interactable = false;
                }
            }
            SetBlockerActive(false);
        }

        #region DOTween path (unscaled: SetUpdate(true))

        private void PlayOpenDotween(int version)
        {
            int count = ItemCount();
            activeSeq = DOTween.Sequence().SetUpdate(true);

            if (toggleIcon != null)
                activeSeq.Join(toggleIcon
                    .DORotate(new Vector3(0f, 0f, -90f), expandDuration)
                    .SetEase(Ease.OutQuad)
                    .SetUpdate(true));

            for (int i = 0; i < count; i++)
            {
                int idx = i;
                var rect = itemRects[idx];
                var group = itemGroups[idx];
                if (rect == null || group == null) continue;

                float at = idx * staggerDelay;
                Vector2 posTarget = new Vector2(rect.anchoredPosition.x, TargetY(idx));
                activeSeq.Insert(at, DOTween.To(
                        () => rect.anchoredPosition,
                        x => rect.anchoredPosition = x,
                        posTarget, expandDuration)
                    .SetEase(Ease.OutQuad)
                    .SetUpdate(true));
                // Alpha uses OutQuad (not OutBack) so it never overshoots above 1.
                activeSeq.Insert(at, DOTween.To(
                        () => group.alpha,
                        x => group.alpha = x,
                        1f, expandDuration)
                    .SetEase(Ease.OutQuad)
                    .SetUpdate(true));
                activeSeq.Insert(at, rect
                    .DOScale(Vector3.one, expandDuration)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true));
                activeSeq.InsertCallback(at + expandDuration, () =>
                {
                    if (version != animVersion) return;
                    group.blocksRaycasts = true;
                    group.interactable = true;
                });
            }

            activeSeq.OnComplete(() =>
            {
                if (version == animVersion) activeSeq = null;
            });
        }

        private void PlayCloseDotween(int version)
        {
            int count = ItemCount();
            activeSeq = DOTween.Sequence().SetUpdate(true);

            if (toggleIcon != null)
                activeSeq.Join(toggleIcon
                    .DORotate(Vector3.zero, expandDuration)
                    .SetEase(Ease.OutQuad)
                    .SetUpdate(true));

            // Reverse stagger: bottom button collapses first.
            for (int i = 0; i < count; i++)
            {
                int idx = i;
                var rect = itemRects[idx];
                var group = itemGroups[idx];
                if (rect == null || group == null) continue;

                float at = (count - 1 - idx) * staggerDelay;
                activeSeq.Insert(at, DOTween.To(
                        () => rect.anchoredPosition,
                        x => rect.anchoredPosition = x,
                        new Vector2(rect.anchoredPosition.x, 0f), expandDuration)
                    .SetEase(Ease.InQuad)
                    .SetUpdate(true));
                activeSeq.Insert(at, DOTween.To(
                        () => group.alpha,
                        x => group.alpha = x,
                        0f, expandDuration)
                    .SetEase(Ease.InQuad)
                    .SetUpdate(true));
                activeSeq.Insert(at, rect
                    .DOScale(Vector3.one * collapsedScale, expandDuration)
                    .SetEase(Ease.InQuad)
                    .SetUpdate(true));
            }

            activeSeq.OnComplete(() =>
            {
                if (version != animVersion) return;
                SetBlockerActive(false);
                activeSeq = null;
            });
        }

        #endregion

    }
}


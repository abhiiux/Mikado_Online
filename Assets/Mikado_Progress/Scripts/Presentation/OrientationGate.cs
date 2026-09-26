using UnityEngine;

namespace Mikado.Presentation
{
    /// <summary>
    /// Landscape-only helper.
    ///  - Native Android/iOS: locks the app to landscape.
    ///  - WebGL / mobile browsers (can't be reliably locked): shows a
    ///    "Please rotate your device" overlay while the screen is portrait.
    ///
    /// Assign a full-screen UI panel (with your own message text) to
    /// rotateOverlay. Does NOT touch Time.timeScale, so the stick-settling
    /// timer in StickCheck keeps running.
    /// </summary>
    public class OrientationGate : MonoBehaviour
    {
        [SerializeField] private GameObject rotateOverlay;

        [Tooltip("Only show the overlay on mobile devices (desktop browsers are left alone).")]
        [SerializeField] private bool onlyOnMobile = true;

        /// <summary>True while the portrait overlay is showing.</summary>
        public static bool IsBlocked { get; private set; }

        private bool lastPortrait;

        private void Awake()
        {
#if !UNITY_WEBGL
            Screen.orientation = ScreenOrientation.AutoRotation;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
#endif
            // Force the first Refresh() to apply
            lastPortrait = !IsPortrait();
            Refresh();
        }

        private void OnDisable()
        {
            IsBlocked = false;
        }

        private void Update()
        {
            Refresh();
        }

        private void Refresh()
        {
            bool portrait = IsPortrait();
            if (portrait == lastPortrait) return;

            lastPortrait = portrait;

            bool shouldGate =
                portrait &&
                (!onlyOnMobile || Application.isMobilePlatform || Application.isEditor);

            IsBlocked = shouldGate;

            if (rotateOverlay != null)
            {
                rotateOverlay.SetActive(shouldGate);
            }
        }

        private static bool IsPortrait()
        {
            return Screen.height > Screen.width;
        }
    }
}
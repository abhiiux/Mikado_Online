using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Profiling;

namespace Mikado.debugging
{
#if DEVELOPMENT_BUILD || UNITY_EDITOR
    public class FPSCounter : MonoBehaviour
    {
        public enum ScreenCorner
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }

        [Header("Sampling")]
        [SerializeField] private float fpsUpdateInterval = 0.5f;
        [SerializeField] private int memorySampleEveryNFrames = 30;

        [Header("Layout")]
        [SerializeField] private int fontSize = 28;
        [SerializeField] private Vector2 screenPadding = new Vector2(16f, 16f);
        [SerializeField] private ScreenCorner screenCorner = ScreenCorner.TopLeft;
        [SerializeField] private float cornerTapZoneSize = 120f;

        [Header("Start State")]
        [SerializeField] private bool visibleOnStart = true;

        private bool _visible;
        private float _accumTime;
        private int _accumFrames;
        private float _currentFps;
        private float _currentFrameTimeMs;
        private long _lastMemoryBytes;
        private int _frameCounter;

        private readonly StringBuilder _sb = new StringBuilder(128);
        private string _cachedDisplayText = string.Empty;

        private GUIStyle _labelStyle;
        private GUIStyle _boxStyle;
        private Texture2D _boxTexture;
        private bool _stylesBuilt;

        private void Awake()
        {
            GameObject existingObject = GameObject.FindWithTag(gameObject.tag);

            if (existingObject != null && existingObject != gameObject)
            {
                Destroy(gameObject);
            }

            DontDestroyOnLoad(gameObject);
            _visible = visibleOnStart;
        }

        private void Update()
        {
            HandleToggleInput();

            if (!_visible)
                return;

            _accumTime += Time.unscaledDeltaTime;
            _accumFrames++;
            _frameCounter++;

            if (_accumTime >= fpsUpdateInterval)
            {
                _currentFps = _accumFrames / _accumTime;
                _currentFrameTimeMs = (_accumTime / _accumFrames) * 1000f;

                _accumTime = 0f;
                _accumFrames = 0;

                if (_frameCounter % memorySampleEveryNFrames == 0)
                {
                    _lastMemoryBytes = Profiler.GetTotalAllocatedMemoryLong();
                }

                RebuildDisplayText();
            }
        }

        private void HandleToggleInput()
        {
            if (Keyboard.current != null &&
                Keyboard.current.pKey.wasPressedThisFrame)
            {
                _visible = !_visible;
                return;
            }

            if (Touchscreen.current != null)
            {
                var touch = Touchscreen.current.primaryTouch;

                if (touch.press.wasPressedThisFrame)
                {
                    Vector2 pos = touch.position.ReadValue();

                    if (IsInSelectedCorner(pos))
                    {
                        _visible = !_visible;
                    }
                }
            }
        }

        private bool IsInSelectedCorner(Vector2 position)
        {
            bool horizontalMatch =
                screenCorner == ScreenCorner.TopLeft ||
                screenCorner == ScreenCorner.BottomLeft
                    ? position.x <= cornerTapZoneSize
                    : position.x >= Screen.width - cornerTapZoneSize;

            bool verticalMatch =
                screenCorner == ScreenCorner.TopLeft ||
                screenCorner == ScreenCorner.TopRight
                    ? position.y >= Screen.height - cornerTapZoneSize
                    : position.y <= cornerTapZoneSize;

            return horizontalMatch && verticalMatch;
        }

        private void RebuildDisplayText()
        {
            _sb.Clear();
            _sb.Append("FPS: ").Append(_currentFps.ToString("F1"));
            _sb.Append("\nFrame: ").Append(_currentFrameTimeMs.ToString("F2")).Append(" ms");
            _sb.Append("\nMem: ").Append((_lastMemoryBytes / 1048576f).ToString("F1")).Append(" MB");

            _cachedDisplayText = _sb.ToString();
        }

        private void EnsureStylesBuilt()
        {
            if (_stylesBuilt)
                return;

            _boxTexture = new Texture2D(1, 1);
            _boxTexture.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.6f));
            _boxTexture.Apply();

            _boxStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = _boxTexture }
            };

            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize,
                normal = { textColor = Color.white },
                alignment = TextAnchor.UpperLeft
            };

            _stylesBuilt = true;
        }

        private void OnGUI()
        {
            if (!_visible)
                return;

            EnsureStylesBuilt();

            float boxWidth = 260f;
            float boxHeight = (fontSize + 6f) * 3f + 16f;

            Rect boxRect = GetBoxRect(boxWidth, boxHeight);

            GUI.Box(boxRect, GUIContent.none, _boxStyle);

            Rect labelRect = new Rect(
                boxRect.x + 12f,
                boxRect.y + 8f,
                boxRect.width - 24f,
                boxRect.height - 16f);

            GUI.Label(labelRect, _cachedDisplayText, _labelStyle);
        }

        private Rect GetBoxRect(float width, float height)
        {
            float x = screenPadding.x;
            float y = screenPadding.y;

            switch (screenCorner)
            {
                case ScreenCorner.TopLeft:
                    break;

                case ScreenCorner.TopRight:
                    x = Screen.width - width - screenPadding.x;
                    break;

                case ScreenCorner.BottomLeft:
                    y = Screen.height - height - screenPadding.y;
                    break;

                case ScreenCorner.BottomRight:
                    x = Screen.width - width - screenPadding.x;
                    y = Screen.height - height - screenPadding.y;
                    break;
            }

            return new Rect(x, y, width, height);
        }

        private void OnDestroy()
        {
            if (_boxTexture != null)
            {
                Destroy(_boxTexture);
            }
        }
    }
#endif
}
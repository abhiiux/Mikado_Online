using DG.Tweening;
using TMPro;
using UnityEngine;
using Mikado.Core;

namespace Mikado.UI
{
    public class GameplayUI : MonoBehaviour
    {
        [Header("Sticks Counter Ref")]
        [SerializeField] private TMP_Text stickCounterUIField;
        [SerializeField] private StickCounterValue counterSO;
        [Header("Lives Counter Ref")]
        [SerializeField] private GameObject[] hearts;
        [SerializeField] private LivesCounterValue livesSO;
        [Header("Heart Animation")]
        [SerializeField] private float heartHideDuration = 0.35f;
        [SerializeField] private Ease heartHideEase = Ease.InBack;

        private int targetCount;
        private int currentCount;
        private Tween[] heartTweens;
        private Vector3[] heartBaseScales;

        void Awake()
        {
            heartTweens = new Tween[hearts.Length];
            heartBaseScales = new Vector3[hearts.Length];

            for (int i = 0; i < hearts.Length; i++)
            {
                if (hearts[i] == null) continue;
                heartBaseScales[i] = hearts[i].transform.localScale;
            }
        }
        void OnEnable()
        {
            GameEventBus.OnTargetCollisionDetected += HandleCounterIncrement;
            GameEventBus.OnLifeLost += HandleLifeLost;
        }
        void OnDisable()
        {
            GameEventBus.OnTargetCollisionDetected -= HandleCounterIncrement;
            GameEventBus.OnLifeLost -= HandleLifeLost;

            KillHeartTweens();
        }
        void Start()
        {
            targetCount = counterSO.CurrentStickCount;
            UpdateCounterUI();
            SetHeartsInstant(livesSO.CurrentLives);
        }

        private void HandleCounterIncrement(GameObject stick)
        {
            currentCount++;

            UpdateCounterUI();

            if (currentCount >= targetCount)
            {
                // Open Win Panel
                GameEventBus.TriggerLevelWon();
            }
        }
        private void UpdateCounterUI()
        {
            if(stickCounterUIField == null) return;

            stickCounterUIField.text = $"{currentCount}/{targetCount}";
        }

        private void HandleLifeLost(int livesLeft)
        {
            for (int i = livesLeft; i < hearts.Length; i++)
            {
                if (hearts[i] == null || !hearts[i].activeSelf) continue;
                AnimateHeartHide(i);
            }
        }
        private void AnimateHeartHide(int index)
        {
            heartTweens[index]?.Kill();

            heartTweens[index] = hearts[index].transform
                .DOScale(Vector3.zero, heartHideDuration)
                .SetEase(heartHideEase)
                .SetUpdate(true)
                .OnComplete(() => hearts[index].SetActive(false));
        }
        private void SetHeartsInstant(int livesLeft)
        {
            for (int i = 0; i < hearts.Length; i++)
            {
                if (hearts[i] == null) continue;

                heartTweens[i]?.Kill();
                hearts[i].transform.localScale = heartBaseScales[i];
                hearts[i].SetActive(i < livesLeft);
            }
        }
        private void KillHeartTweens()
        {
            if (heartTweens == null) return;

            for (int i = 0; i < heartTweens.Length; i++)
                heartTweens[i]?.Kill();
        }
    }
}
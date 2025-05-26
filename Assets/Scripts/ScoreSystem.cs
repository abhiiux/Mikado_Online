using TMPro;
using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    [SerializeField] TMP_Text scoreUI;
    [SerializeField] GameObject winUI;
    [SerializeField] GameObject lostUI;
    [SerializeField] int scoretoWin = 25;
    [SerializeField] int totalavailableScore = 31;
    private int currentScore;


    void OnEnable()
    {
        CollisionChecker.OnCollision += Checker;
    }
    void OnDisable()
    {
        CollisionChecker.OnCollision -= Checker;
    }

    public void Checker(int score, bool value)
    {
        switch (value)
        {
            case true:
                currentScore += score;
                scoreUI.text = $"{currentScore.ToString()} / {scoretoWin}";
                if (currentScore >= scoretoWin)
                {
                    Debug.Log($"current score is {currentScore}");
                    winUI.SetActive(true);
                }
                break;

            case false:

                totalavailableScore -= score;
                if (totalavailableScore < scoretoWin)
                {
                    Debug.Log($" total available score is {totalavailableScore}");
                    lostUI.SetActive(true);
                }
                break;
        }
    }

}

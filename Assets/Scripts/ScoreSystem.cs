using TMPro;
using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    [SerializeField] TMP_Text scoreUI;
    [SerializeField] GameObject winUI;
    [SerializeField] GameObject lostUI;
    private int currentScore;
    private int scoretoWin = 25;
    private int totalavailableScore = 31;


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
                scoreUI.text = currentScore.ToString();
                if (currentScore >= scoretoWin)
                {
                    Debug.Log($"current score is {currentScore}");
                    winUI.SetActive(true);
                }                break;

            case false:

                totalavailableScore -= score;
                if (totalavailableScore < scoretoWin)
                {
                    Debug.Log($" total available score is {totalavailableScore}");
                    lostUI.SetActive(true);
                }                break;
        }
    }

}

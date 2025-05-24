using TMPro;
using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    [SerializeField] TMP_Text scoreUI;
    private int currentScore;
    private int scoretoWin = 25;
    private int totalavailableScore = 25;


    void OnEnable()
    {
        CollisionChecker.OnCollision += Checker;
    }
    void OnDisable()
    {
        CollisionChecker.OnCollision -= Checker;
    }

    public void Checker(int score)
    {
        currentScore += score;
        totalavailableScore -= score;
        scoreUI.text = currentScore.ToString();

        if (currentScore >= scoretoWin)
        {
            Debug.Log(" YOU WIN!");
        }
        else if (totalavailableScore < scoretoWin)
        {
            Debug.Log(" YOU Losssee!");
        }
    }
}

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

    public void Checker(int score, string print)
    {
        currentScore += score;
        scoreUI.text = currentScore.ToString();

        if (currentScore >= scoretoWin)
        {
            Debug.Log(" YOU WIN!" + print);
            winUI.SetActive(true);
        }
        else if (totalavailableScore < scoretoWin)
        {
            Debug.Log(" YOU Losssee!" + totalavailableScore + " " + print);
            lostUI.SetActive(true);
        }
    }

    public void SetValue(int num)
    {
        totalavailableScore -= num;
        Debug.Log(" total points left in the board is : " + totalavailableScore);
    }
}

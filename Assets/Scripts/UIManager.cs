using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    // [SerializeField] Button pauseButton;
    // [SerializeField] Button restartButton;
    [SerializeField] TMP_Text debugUI;
    private bool paused = false;

    public void PauseButton()
    {
        if (paused)
        {
            Debug.Log(" un Paused");
            Time.timeScale = 1f;
            paused = false;
        }
        else
        {
            Debug.Log(" Pauseee");
            Time.timeScale = 0f;
            paused = true;
        }
    }

    public void RestartButton()
    {
        Debug.Log(" Restart");
        if (paused)
        {
            debugUI.text = " Un Pause Scene";
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    public void NextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}

using Mikado.Presentation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mikado.UI
{
    public class NavigationUI : MonoBehaviour
    {
        private bool paused;

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
    }
}

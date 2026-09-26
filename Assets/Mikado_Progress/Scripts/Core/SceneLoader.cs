using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mikado.Presentation
{
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void LoadScene(int buildIndex)
        {
            if (buildIndex < 0 || buildIndex >= SceneManager.sceneCountInBuildSettings)
            {
                Debug.LogError($"<b>[SceneLoader]</b> Build index {buildIndex} is out of range! (Total scenes: {SceneManager.sceneCountInBuildSettings})");
                return;
            }
            SceneManager.LoadScene(buildIndex);
        }
        public void LoadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("<b>[SceneLoader]</b> Scene name cannot be null or empty!");
                return;
            }

            SceneManager.LoadScene(sceneName);
        }

        public void LoadNextScene()
        {
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            LoadScene( nextSceneIndex );
        }
    }
}
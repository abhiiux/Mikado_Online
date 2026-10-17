using Mikado.Presentation;
using UnityEngine;

namespace Mikado.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        public void Play()
        {
            SceneLoader.Instance.LoadNextScene();
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}

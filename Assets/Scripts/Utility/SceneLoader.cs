using UnityEngine.SceneManagement;

namespace Utility
{
    public class SceneLoader
    {
        public static void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public static void ReloadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public static void NextScene()
        {
            int currentScene = SceneManager.GetActiveScene().buildIndex + 1;
            SceneManager.LoadScene(currentScene < SceneManager.sceneCountInBuildSettings ? currentScene : 0);
        }
    }
}
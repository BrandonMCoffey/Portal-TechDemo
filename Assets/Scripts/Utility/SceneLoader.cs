using UnityEngine.SceneManagement;

namespace Assets.Scripts.Utility {
    public class SceneLoader {
        public static void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public static void ReloadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
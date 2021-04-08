using UnityEngine.SceneManagement;

namespace Assets.Scripts {
    public class SceneLoader {
        public static void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
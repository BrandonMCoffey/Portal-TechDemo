using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts {
    public class MainMenuController : MonoBehaviour {
        [SerializeField] private Text _highScoreTextView = null;
        [SerializeField] private AudioClip _startingSong = null;

        private void Start()
        {
            int highScore = PlayerPrefs.GetInt("HighScore");
            _highScoreTextView.text = "High Score: " + highScore.ToString();

            if (_startingSong != null) {
                if (!AudioManager.Instance.IsPlaying()) {
                    AudioManager.Instance.PlaySong(_startingSong);
                }
            } else {
                Debug.Log("Warning: No Starting Song on Main Menu Controller");
            }
        }

        public void ResetData()
        {
            PlayerPrefs.SetInt("HighScore", 0);
        }

        public void LoadScene(string sceneName)
        {
            SceneLoader.LoadScene(sceneName);
        }

        public void ExitApplication()
        {
            Application.Quit();
        }
    }
}
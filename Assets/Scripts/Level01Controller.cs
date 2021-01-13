using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Assets.Scripts {
    public class Level01Controller : MonoBehaviour {
        [SerializeField] private Text _currentScoreTextView = null;
        private int _currentScore;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                ExitLevel();
            }
        }

        public void IncreaseScore(int amount)
        {
            _currentScore += amount;
            if (_currentScoreTextView != null) {
                _currentScoreTextView.text = "Score: " + _currentScore.ToString();
            } else {
                Debug.Log("Warning: Level01 Controller is missing the Current Score Text View");
            }
        }

        public void ExitLevel()
        {
            int highScore = PlayerPrefs.GetInt("HighScore");
            if (_currentScore > highScore) {
                PlayerPrefs.SetInt("HighScore", _currentScore);
            }

            SceneManager.LoadScene("MainMenu");
        }
    }
}
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts {
    public class Level01Controller : MonoBehaviour {
        [SerializeField] private Text _currentScoreTextView = null;
        private int _currentScore;

        [SerializeField] private GameObject _pauseMenu = null;
        private bool _isPaused;

        private void Start()
        {
            Pause(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                Pause(!_isPaused);
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

        public void Pause(bool action)
        {
            _pauseMenu.SetActive(action);
            Time.timeScale = action ? 0 : 1;
            Cursor.lockState = action ? CursorLockMode.None : CursorLockMode.Locked;
            _isPaused = action;
        }

        public void ExitLevel()
        {
            int highScore = PlayerPrefs.GetInt("HighScore");
            if (_currentScore > highScore) {
                PlayerPrefs.SetInt("HighScore", _currentScore);
            }

            SceneLoader.LoadScene("MainMenu");
        }
    }
}
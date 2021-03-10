using Assets.Scripts.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts {
    public class Level01Controller : MonoBehaviour {
        [SerializeField] private GameEvent _pauseEvent = null;
        [SerializeField] private GameEvent _unpauseEvent = null;
        [SerializeField] private BoolVariable _playerHasControl = null;
        [SerializeField] private FloatVariable _currentScore = null;

        private bool _isPaused;

        private void Awake()
        {
            if (_pauseEvent == null) Debug.Log("[" + GetType().Name + "] Pause Event missing on " + name);
            if (_unpauseEvent == null) Debug.Log("[" + GetType().Name + "] Unpause Event missing on " + name);
            if (_playerHasControl == null) Debug.Log("[" + GetType().Name + "] Player Has Control Bool Variable missing on " + name);
            if (_currentScore == null) Debug.Log("[" + GetType().Name + "] Current Score missing on " + name);
        }

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

        public void Pause(bool action)
        {
            if (action) {
                _pauseEvent.Raise();
                _playerHasControl.SetValue(false);
            } else {
                _unpauseEvent.Raise();
                _playerHasControl.SetValue(true);
            }
            // Time.timeScale = action ? 0 : 1;
            Cursor.visible = action;
            Cursor.lockState = action ? CursorLockMode.None : CursorLockMode.Locked;
            _isPaused = action;
        }

        public void ExitLevel()
        {
            int highScore = PlayerPrefs.GetInt("HighScore");
            if (_currentScore.Value > highScore) {
                PlayerPrefs.SetInt("HighScore", Mathf.FloorToInt(_currentScore.Value));
            }

            SceneLoader.LoadScene("MainMenu");
        }
    }
}
using Assets.Scripts.GameEvents.Logic;
using Assets.Scripts.Utility;
using UnityEngine;

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
#if UNITY_EDITOR
            Application.targetFrameRate = 60;
#endif
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                if (_isPaused) {
                    _unpauseEvent.Raise();
                } else {
                    _pauseEvent.Raise();
                }
            }
            if (Input.GetKeyDown(KeyCode.Q)) {
                OnCollect();
            }
        }

        public void OnCollect()
        {
            _currentScore.ApplyChange(1);
        }

        public void Pause(bool action)
        {
            _playerHasControl.SetValue(!action);
            // Time.timeScale = action ? 0 : 1;
            Cursor.visible = action;
            Cursor.lockState = action ? CursorLockMode.None : CursorLockMode.Locked;
            _isPaused = action;
        }

        public void OnRespawn()
        {
            SceneLoader.ReloadScene();
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
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.GameEvents.Logic;
using Assets.Scripts.Utility;
using UnityEngine;

namespace Assets.Scripts {
    public class Level01Controller : MonoBehaviour {
        [SerializeField] private SavingSystem.ScoreStrings _scoreString = SavingSystem.ScoreStrings.Level01;

        [SerializeField] private GameEvent _pauseEvent = null;
        [SerializeField] private GameEvent _unpauseEvent = null;
        [SerializeField] private BoolVariable _playerHasControl = null;
        [SerializeField] private FloatVariable _currentTime = null;
        [SerializeField] private FloatVariable _cheatScore;
        [SerializeField] private List<FloatVariable> _currentScores = null;

        private bool _isPaused;

        private void Awake()
        {
            if (_pauseEvent == null) Debug.Log("[" + GetType().Name + "] Pause Event missing on " + name);
            if (_unpauseEvent == null) Debug.Log("[" + GetType().Name + "] Unpause Event missing on " + name);
            if (_playerHasControl == null) Debug.Log("[" + GetType().Name + "] Player Has Control Bool Variable missing on " + name);
            if (_currentScores == null) Debug.Log("[" + GetType().Name + "] Current Scores missing on " + name);
            if (_currentTime == null) Debug.Log("[" + GetType().Name + "] Current Time missing on " + name);
        }

        private void OnEnable()
        {
            Pause(false);
            Application.targetFrameRate = 120;
            foreach (var score in _currentScores) {
                score.SetValue(0);
            }
            _currentTime.SetValue(0);
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
                _cheatScore.ApplyChange(1);
            }
            if (!_isPaused) _currentTime.ApplyChange(Time.deltaTime);
        }

        public void Pause(bool action)
        {
            _playerHasControl.SetValue(!action);
            // Time.timeScale = action ? 0 : 1;
            Cursor.visible = action;
            Cursor.lockState = action ? CursorLockMode.None : CursorLockMode.Locked;
            _isPaused = action;
        }

        public void ResetTime()
        {
            _currentTime.SetValue(0);
        }

        public void OnRespawn()
        {
            SceneLoader.ReloadScene();
        }

        public void Win()
        {
            int highScore = SavingSystem.GetScore(_scoreString);
            float total = _currentScores.Sum(scores => scores.Value);
            total += _cheatScore.Value;
            if (total > highScore) {
                SavingSystem.SetScore(_scoreString, Mathf.FloorToInt(total));
            }

            float bestTime = SavingSystem.GetTime(_scoreString);
            if (_currentTime.Value < bestTime || bestTime == 0) {
                SavingSystem.SetTime(_scoreString, _currentTime.Value);
            }

            Pause(true);
        }

        public void NextLevel()
        {
            SceneLoader.NextScene();
        }

        public void ExitLevel()
        {
            foreach (var score in _currentScores) {
                score.SetValue(0);
            }
            _currentTime.SetValue(0);
            SceneLoader.LoadScene("MainMenu");
        }
    }
}
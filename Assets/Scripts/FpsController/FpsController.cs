using System;
using UnityEngine;

namespace Assets.Scripts.FpsController {
    [RequireComponent(typeof(FpsSetup), typeof(FpsInput))]
    public class FpsController : MonoBehaviour {
        private FpsSetup _setup;
        private FpsInput _input;
        private FpsMotor _motor;
        private FpsCamera _camera;

        private bool _isPaused;
        private bool _isAlive;

        private void Awake()
        {
            _setup = GetComponent<FpsSetup>();
            _input = GetComponent<FpsInput>();
            _motor = GetComponent<FpsMotor>();
            _camera = GetComponent<FpsCamera>();
        }

        private void OnEnable()
        {
            _input.PauseInput += ActivatePauseMenu;
        }

        private void OnDisable()
        {
            _input.PauseInput -= ActivatePauseMenu;
        }

        private void Start()
        {
            ActivatePauseMenu(false);
        }

        private void ActivatePauseMenu()
        {
            ActivatePauseMenu(!_isPaused);
        }

        private void ActivatePauseMenu(bool activate)
        {
            _isPaused = activate;
            if (_setup.PauseMenu != null) {
                _setup.PauseMenu.SetActive(_isPaused);
            } else {
                Debug.Log("Warning: No Pause Menu connected to Player HUD");
            }

            if (_setup.LockCursorToGame) {
                Cursor.lockState = _isPaused ? CursorLockMode.Locked : CursorLockMode.None;
            }

            if (_setup.PauseStopsTime) {
                Time.timeScale = _isPaused ? 0 : 1;
            }
        }
    }
}
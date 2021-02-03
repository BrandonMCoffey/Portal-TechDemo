using UnityEngine;

namespace Assets.Scripts {
    public class PlayerCamera : MonoBehaviour {
        [SerializeField] private float _mouseSensitivity = 100f;
        [SerializeField] private Transform _player = null;
        private float _xRotation;

        [HideInInspector] public bool _isAlive = true;

        private void Start()
        {
            if (_player == null) {
                Debug.Log("Warning: Camera Controller is missing Player transform");
            }
        }

        private void Update()
        {
            if (!_isAlive) return;

            float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity * Time.deltaTime;

            _xRotation -= mouseY;
            _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
            _player.Rotate(Vector3.up * mouseX);
        }
    }
}
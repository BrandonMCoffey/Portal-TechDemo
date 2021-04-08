using Assets.Scripts.Utility;
using UnityEngine;

namespace Assets.Scripts.Player {
    public class PlayerLook : MonoBehaviour {
        [Header("Camera Settings")]
        [SerializeField] private float _xMouseSensitivity = 10;
        [SerializeField] private float _yMouseSensitivity = 10;
        [SerializeField] private Vector2 _upDownClamp = new Vector2(-40, 85);
        [SerializeField] private float _rotationSmoothTime = 0.1f;

        [Header("References")]
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private TransformVariable _transform = null;
        [SerializeField] private BoolVariable _playerHasControl = null;

        [Header("Other")]
        public float Yaw;
        public float Pitch;

        [HideInInspector] public float SmoothYaw;
        [HideInInspector] public float SmoothPitch;

        private float _yawSmoothV;
        private float _pitchSmoothV;

        private void Awake()
        {
            if (_transform == null) Debug.Log("[" + GetType().Name + "] Transform Variable missing on " + name);
            if (_playerHasControl == null) Debug.Log("[" + GetType().Name + "] Player Has Control Bool Variable missing on " + name);
        }

        private void Start()
        {
            if (_playerCamera == null) _playerCamera = Camera.main;
            Yaw = transform.eulerAngles.y;
            Pitch = _playerCamera.transform.localEulerAngles.x;
            SmoothYaw = Yaw;
            SmoothPitch = Pitch;
        }

        private void Update()
        {
            if (_playerHasControl != null && !_playerHasControl.Value) return;

            float mX = Input.GetAxisRaw("Mouse X");
            float mY = Input.GetAxisRaw("Mouse Y");

            // FIXME: Hack to stop camera swinging down at start
            float mMag = Mathf.Sqrt(mX * mX + mY * mY);
            if (mMag > 5) {
                mX = 0;
                mY = 0;
            }

            Yaw += mX * _xMouseSensitivity;
            Pitch -= mY * _yMouseSensitivity;
            Pitch = Mathf.Clamp(Pitch, _upDownClamp.x, _upDownClamp.y);
            SmoothPitch = Mathf.SmoothDampAngle(SmoothPitch, Pitch, ref _pitchSmoothV, _rotationSmoothTime);
            SmoothYaw = Mathf.SmoothDampAngle(SmoothYaw, Yaw, ref _yawSmoothV, _rotationSmoothTime);

            Vector3 rotX = Vector3.right * SmoothPitch;
            Vector3 rotY = Vector3.up * SmoothYaw;

            transform.eulerAngles = rotY;
            _playerCamera.transform.localEulerAngles = rotX;

            if (_transform != null) {
                _transform.Rotation = Quaternion.Euler(rotX + rotY);
                _transform.Forward = transform.forward;
            }
        }
    }
}
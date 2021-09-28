using UnityEngine;
using Utility;
using Utility.GameEvents.Logic;
using Utility.References;

namespace Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovementCC : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float _walkSpeed = 5;
        [SerializeField] private float _walkAcceleration = 7;
        [SerializeField] private float _walkDeacceleration = 10;
        [SerializeField] private float _runSpeed = 7;
        [SerializeField] private float _runAcceleration = 14;
        [SerializeField] private float _runDeacceleration = 9;
        [SerializeField] private float _airAcceleration = 2;
        [SerializeField] private float _airDeacceleration = 2;
        [SerializeField] private float _jumpSpeed = 8;

        [Header("Camera Settings")]
        [SerializeField] private float _xMouseSensitivity = 10;
        [SerializeField] private float _yMouseSensitivity = 10;
        [SerializeField] private Vector2 _upDownClamp = new Vector2(-65, 75);
        [SerializeField] private float _rotationSmoothTime = 0.1f;

        [Header("World Settings")]
        [SerializeField] private float _gravity = 20;
        [SerializeField] private float _friction = 6;
        [SerializeField] private float _minimumY = -12;
        [SerializeField] private TransformReference _respawnPosition = null;
        [SerializeField] private GameEvent _onAnomalyReset = null;

        [Header("References")]
        [SerializeField] private Transform _cameraTransform = null;
        [SerializeField] private TransformVariable _transform = null;
        [SerializeField] private BoolVariable _playerHasControl = null;

        [Header("Other")]
        public float Yaw;
        public float Pitch;

        [HideInInspector] public float SmoothYaw;
        [HideInInspector] public float SmoothPitch;

        private float _yawSmoothV;
        private float _pitchSmoothV;

        private CharacterController _controller;

        private bool _isRunning;
        private float _forwardMove;
        private float _rightMove;

        [HideInInspector] public Vector3 Velocity = Vector3.zero;

        private bool _wishJump;

        private void Awake()
        {
            if (_cameraTransform == null) Debug.Log("[" + GetType().Name + "] Camera Transform missing on " + name);
            if (_transform == null) Debug.Log("[" + GetType().Name + "] Transform Variable missing on " + name);
            if (_playerHasControl == null) Debug.Log("[" + GetType().Name + "] Player Has Control Bool Variable missing on " + name);
        }

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
            if (_transform != null) _transform.Position = transform.position;
            Yaw = transform.eulerAngles.y;
            Pitch = _cameraTransform.localEulerAngles.x;
            SmoothYaw = Yaw;
            SmoothPitch = Pitch;
        }

        private void Update()
        {
            if (_playerHasControl != null) {
                if (!_playerHasControl.Value) return;
            }

            float mX = Input.GetAxisRaw("Mouse X") * _xMouseSensitivity * 0.02f;
            float mY = Input.GetAxisRaw("Mouse Y") * _yMouseSensitivity * 0.02f;

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
            if (_cameraTransform != null) _cameraTransform.localEulerAngles = rotX;

            if (_transform != null) {
                _transform.Rotation = Quaternion.Euler(rotX + rotY);
                _transform.Forward = transform.forward;
            }

            SetMovementDirection();
            QueueJump();
            if (_controller.isGrounded) {
                GroundMove();
            } else if (!_controller.isGrounded) {
                AirMove();
            }
            _controller.Move(Velocity * Time.deltaTime);

            if (transform.position.y < _minimumY) {
                transform.position = _respawnPosition.Position;
                transform.rotation = _respawnPosition.Rotation;
                Yaw = 0;
                SmoothYaw = 0;
                Pitch = 0;
                SmoothPitch = 0;
                if (_onAnomalyReset != null) _onAnomalyReset.Raise();
            }

            if (_transform != null) _transform.Position = transform.position;
        }

        private void SetMovementDirection()
        {
            _forwardMove = Input.GetAxisRaw("Vertical");
            _rightMove = Input.GetAxisRaw("Horizontal");

            if (Input.GetButtonDown("Run") && !_isRunning) _isRunning = true;
            if (Input.GetButtonUp("Run")) _isRunning = false;

            if (!_isRunning) _rightMove *= 0.5f;
        }

        private void QueueJump()
        {
            if (Input.GetButtonDown("Jump") && !_wishJump) _wishJump = true;
            if (Input.GetButtonUp("Jump")) _wishJump = false;
        }

        private void GroundMove()
        {
            ApplyFriction(_wishJump ? 0 : 1.0f);

            var wishDir = new Vector3(_rightMove, 0, _forwardMove);
            wishDir = transform.TransformDirection(wishDir);
            wishDir.Normalize();

            var wishSpeed = wishDir.magnitude;
            wishSpeed *= _isRunning ? _runSpeed : _walkSpeed;

            Accelerate(wishDir, wishSpeed, _isRunning ? _runAcceleration : _walkAcceleration);

            Velocity.y = -_gravity * Time.deltaTime;

            if (_wishJump) {
                Velocity.y = _jumpSpeed;
                _wishJump = false;
            }
        }

        private void AirMove()
        {
            Vector3 wishDir = new Vector3(0, 0, _forwardMove);
            wishDir = transform.TransformDirection(wishDir);

            float wishSpeed = wishDir.magnitude;
            wishDir.Normalize();

            wishSpeed *= _isRunning ? _runSpeed : _walkSpeed;

            float accel = Vector3.Dot(Velocity, wishDir) < 0 ? _airDeacceleration : _airAcceleration;

            Accelerate(wishDir, wishSpeed, accel);

            Velocity.y -= _gravity * Time.deltaTime;
        }

        private void ApplyFriction(float t)
        {
            Vector3 vec = Velocity;

            vec.y = 0.0f;
            float speed = vec.magnitude;
            float drop = 0.0f;

            if (_controller.isGrounded) {
                float deceleration = _isRunning ? _runDeacceleration : _walkDeacceleration;
                float control = speed < deceleration ? deceleration : speed;
                drop = control * _friction * Time.deltaTime * t;
            }

            float newSpeed = speed - drop;
            if (newSpeed < 0)
                newSpeed = 0;
            if (speed > 0)
                newSpeed /= speed;

            Velocity.x *= newSpeed;
            Velocity.z *= newSpeed;
        }

        private void Accelerate(Vector3 wishDir, float wishSpeed, float accel)
        {
            float currentSpeed = Vector3.Dot(Velocity, wishDir);
            float addSpeed = wishSpeed - currentSpeed;
            if (addSpeed <= 0) return;
            float accelSpeed = accel * Time.deltaTime * wishSpeed;
            if (accelSpeed > addSpeed) accelSpeed = addSpeed;

            Velocity.x += accelSpeed * wishDir.x;
            Velocity.z += accelSpeed * wishDir.z;

            float speed = _isRunning ? _runSpeed : _walkSpeed;
            if (Velocity.magnitude > speed + 2) {
                float ySpeed = Velocity.y;
                Velocity.Normalize();
                Velocity.x *= speed + 2;
                Velocity.y = ySpeed;
                Velocity.z *= speed + 2;
            }
        }
    }
}
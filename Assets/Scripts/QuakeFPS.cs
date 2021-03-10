using Assets.Scripts.Utility;
using UnityEngine;

namespace Assets.Scripts {
    [RequireComponent(typeof(CharacterController))]
    public class QuakeFPS : MonoBehaviour {
        [Header("Movement Settings")]
        [SerializeField] private float _moveSpeed = 7;               // Ground move speed
        [SerializeField] private float _runAcceleration = 14;        // Ground accel
        [SerializeField] private float _runDeacceleration = 10;      // Deacceleration that occurs when running on the ground
        [SerializeField] private float _airAcceleration = 2;         // Air accel
        [SerializeField] private float _airDeacceleration = 2;       // Deacceleration experienced when opposite strafing
        [SerializeField] private float _airControl = 0.3f;           // How precise air control is
        [SerializeField] private float _sideStrafeAcceleration = 50; // How fast acceleration occurs to get up to sideStrafeSpeed when
        [SerializeField] private float _sideStrafeSpeed = 1;         // What the max speed to generate when side strafing
        [SerializeField] private float _jumpSpeed = 8;               // The speed at which the character's up axis gains when hitting jump

        [Header("Camera Settings")]
        [SerializeField] private float _xMouseSensitivity = 30;
        [SerializeField] private float _yMouseSensitivity = 30;
        [SerializeField] private float _upDownClamp = 80;

        [Header("Frame-Based Settings")]
        [SerializeField] private float _gravity = 20;
        [SerializeField] private float _friction = 6; //Ground friction

        [Header("References")]
        [SerializeField] private TransformVariable _transform = null;
        [SerializeField] private BoolVariable _playerHasControl = null;

        private CharacterController _controller;

        private float _forwardMove;
        private float _rightMove;
        private float _rotX;
        private float _rotY;

        private Vector3 _playerVelocity = Vector3.zero;
        private float _playerTopVelocity;

        private bool _wishJump;

        private void Awake()
        {
            if (_transform == null) Debug.Log("[" + GetType().Name + "] Transform Variable missing on " + name);
            if (_playerHasControl == null) Debug.Log("[" + GetType().Name + "] Player Has Control Bool Variable missing on " + name);
        }

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
            if (_transform != null) _transform.Position = transform.position;
        }

        private void Update()
        {
            if (_playerHasControl != null) {
                if (!_playerHasControl.Value) return;
            }
            // Rotations
            _rotX -= Input.GetAxisRaw("Mouse Y") * _xMouseSensitivity * 0.02f;
            _rotY += Input.GetAxisRaw("Mouse X") * _yMouseSensitivity * 0.02f;

            _rotX = Mathf.Clamp(_rotX, -_upDownClamp, _upDownClamp);

            transform.rotation = Quaternion.Euler(0, _rotY, 0);
            if (_transform != null) _transform.Rotation = Quaternion.Euler(_rotX, _rotY, 0);
            _transform.Forward = transform.forward;

            // Movement
            SetMovementDirection();
            QueueJump();
            if (_controller.isGrounded) {
                GroundMove();
            } else if (!_controller.isGrounded) {
                AirMove();
            }

            _controller.Move(_playerVelocity * Time.deltaTime);

            // Maximum Velocity
            Vector3 vel = _playerVelocity;
            vel.y = 0.0f;
            if (vel.magnitude > _playerTopVelocity) {
                _playerTopVelocity = vel.magnitude;
            }

            // Move the camera after the player has been moved because otherwise the camera will clip the player if going fast enough and will always be 1 frame behind.
            if (_transform != null) _transform.Position = transform.position;
        }

        private void SetMovementDirection()
        {
            _forwardMove = Input.GetAxisRaw("Vertical");
            _rightMove = Input.GetAxisRaw("Horizontal");
        }

        private void QueueJump()
        {
            if (Input.GetButtonDown("Jump") && !_wishJump) _wishJump = true;
            if (Input.GetButtonUp("Jump")) _wishJump = false;
        }

        private void GroundMove()
        {
            // Do not apply friction if the player is queueing up the next jump
            ApplyFriction(_wishJump ? 0 : 1.0f);

            var wishDir = new Vector3(_rightMove, 0, _forwardMove);
            wishDir = transform.TransformDirection(wishDir);
            wishDir.Normalize();

            var wishSpeed = wishDir.magnitude;
            wishSpeed *= _moveSpeed;

            Accelerate(wishDir, wishSpeed, _runAcceleration);

            // Reset the gravity velocity
            _playerVelocity.y = -_gravity * Time.deltaTime;

            if (_wishJump) {
                _playerVelocity.y = _jumpSpeed;
                _wishJump = false;
            }
        }

        private void AirMove()
        {
            Vector3 wishDir = new Vector3(_rightMove, 0, _forwardMove);
            wishDir = transform.TransformDirection(wishDir);

            float wishSpeed = wishDir.magnitude;
            wishSpeed *= _moveSpeed;

            wishDir.Normalize();

            // CPM: Air control
            float wishSpeed2 = wishSpeed;
            float accel = Vector3.Dot(_playerVelocity, wishDir) < 0 ? _airDeacceleration : _airAcceleration;
            // If the player is ONLY strafing left or right
            if (_forwardMove == 0 && _rightMove != 0) {
                if (wishSpeed > _sideStrafeSpeed) {
                    wishSpeed = _sideStrafeSpeed;
                }
                accel = _sideStrafeAcceleration;
            }

            Accelerate(wishDir, wishSpeed, accel);
            if (_airControl > 0) AirControl(wishDir, wishSpeed2);
            // !CPM: Air control

            // Apply gravity
            _playerVelocity.y -= _gravity * Time.deltaTime;
        }

        // Air control occurs when the player is in the air, it allows players to move side to side much faster rather than being 'sluggish' when it comes to cornering.
        private void AirControl(Vector3 wishDir, float wishSpeed)
        {
            // Can't control movement if not moving forward or backward
            if (Mathf.Abs(_forwardMove) < 0.001f || Mathf.Abs(wishSpeed) < 0.001f) return;
            float zSpeed = _playerVelocity.y;
            _playerVelocity.y = 0;
            /* Next two lines are equivalent to idTech's VectorNormalize() */
            float speed = _playerVelocity.magnitude;
            _playerVelocity.Normalize();

            float dot = Vector3.Dot(_playerVelocity, wishDir);
            float k = 32;
            k *= _airControl * dot * dot * Time.deltaTime;

            // Change direction while slowing down
            if (dot > 0) {
                _playerVelocity.x = _playerVelocity.x * speed + wishDir.x * k;
                _playerVelocity.y = _playerVelocity.y * speed + wishDir.y * k;
                _playerVelocity.z = _playerVelocity.z * speed + wishDir.z * k;

                _playerVelocity.Normalize();
            }

            _playerVelocity.x *= speed;
            _playerVelocity.y = zSpeed; // Note this line
            _playerVelocity.z *= speed;
        }

        //Applies friction to the player, called in both the air and on the ground
        private void ApplyFriction(float t)
        {
            Vector3 vec = _playerVelocity; // Equivalent to: VectorCopy();

            vec.y = 0.0f;
            float speed = vec.magnitude;
            float drop = 0.0f;

            /* Only if the player is on the ground then apply friction */
            if (_controller.isGrounded) {
                float control = speed < _runDeacceleration ? _runDeacceleration : speed;
                drop = control * _friction * Time.deltaTime * t;
            }

            float newSpeed = speed - drop;
            if (newSpeed < 0)
                newSpeed = 0;
            if (speed > 0)
                newSpeed /= speed;

            _playerVelocity.x *= newSpeed;
            _playerVelocity.z *= newSpeed;
        }

        private void Accelerate(Vector3 wishDir, float wishSpeed, float accel)
        {
            float currentSpeed = Vector3.Dot(_playerVelocity, wishDir);
            float addSpeed = wishSpeed - currentSpeed;
            if (addSpeed <= 0) return;
            float accelSpeed = accel * Time.deltaTime * wishSpeed;
            if (accelSpeed > addSpeed) accelSpeed = addSpeed;

            _playerVelocity.x += accelSpeed * wishDir.x;
            _playerVelocity.z += accelSpeed * wishDir.z;
        }
    }
}
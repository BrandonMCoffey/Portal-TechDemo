using System;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Assets.Scripts {
    [RequireComponent(typeof(Rigidbody))]
    public class OldFPSMotor : MonoBehaviour {
        public event Action Land = delegate { };

        [SerializeField] private Camera _camera = null;
        [SerializeField] private float _cameraAngleLimit = 70;
        [SerializeField] private GroundDetector _groundDetector = null;

        private Vector3 _movementThisFrame = Vector3.zero;
        private float _turnAmountThisFrame;
        private float _lookAmountThisFrame;
        private float _currentCameraRotationX;
        private bool _isGrounded;

        private Rigidbody _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            _groundDetector.GroundDetected += OnGroundDetected;
            _groundDetector.GroundVanished += OnGroundVanished;
        }

        private void OnDisable()
        {
            _groundDetector.GroundDetected -= OnGroundDetected;
            _groundDetector.GroundVanished -= OnGroundVanished;
        }

        private void FixedUpdate()
        {
            ApplyMovement(_movementThisFrame);
            ApplyTurn(_turnAmountThisFrame);
            ApplyLook(_lookAmountThisFrame);
        }

        private void ApplyMovement(Vector3 moveVector)
        {
            if (moveVector == Vector3.zero) return;

            _rigidbody.MovePosition(_rigidbody.position + moveVector);
            _movementThisFrame = Vector3.zero;
        }

        private void ApplyTurn(float rotateAmount)
        {
            if (rotateAmount == 0) return;

            Quaternion newRotation = Quaternion.Euler(0, rotateAmount, 0);
            _rigidbody.MoveRotation(_rigidbody.rotation * newRotation);
            _turnAmountThisFrame = 0;
        }

        private void ApplyLook(float lookAmount)
        {
            if (lookAmount == 0) return;

            _currentCameraRotationX -= lookAmount;
            _currentCameraRotationX = Mathf.Clamp(_currentCameraRotationX, -_cameraAngleLimit, _cameraAngleLimit);
            _camera.transform.localEulerAngles = new Vector3(_currentCameraRotationX, 0, 0);
            _lookAmountThisFrame = 0;
        }

        private void OnGroundDetected()
        {
            _isGrounded = true;
            Land?.Invoke();
        }

        private void OnGroundVanished()
        {
            _isGrounded = false;
        }

        public void Move(Vector3 requestedMovement)
        {
            _movementThisFrame = requestedMovement;
        }

        public void Turn(float turnAmount)
        {
            _turnAmountThisFrame = turnAmount;
        }

        public void Look(float lookAmount)
        {
            _lookAmountThisFrame = lookAmount;
        }

        public void Jump(float jumpForce)
        {
            if (!_isGrounded) return;
            _rigidbody.AddForce(Vector3.up * jumpForce * 100);
        }
    }
}
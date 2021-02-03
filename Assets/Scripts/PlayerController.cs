using UnityEngine;

namespace Assets.Scripts {
    [RequireComponent(typeof(OldFPSInput))]
    [RequireComponent(typeof(OldFPSMotor))]
    public class PlayerController : MonoBehaviour {
        private OldFPSInput _input;
        private OldFPSMotor _motor;

        [SerializeField] private float _moveSpeed = 0.1f;
        [SerializeField] private float _turnSpeed = 6f;
        [SerializeField] private float _jumpStrength = 7f;

        private void Awake()
        {
            _input = GetComponent<OldFPSInput>();
            _motor = GetComponent<OldFPSMotor>();
        }

        private void OnEnable()
        {
            _input.MoveInput += OnMove;
            _input.RotateInput += OnRotate;
            _input.JumpInput += OnJump;
        }

        private void OnDisable()
        {
            _input.MoveInput -= OnMove;
            _input.RotateInput -= OnRotate;
            _input.JumpInput -= OnJump;
        }

        private void OnMove(Vector3 movement)
        {
            _motor.Move(movement * _moveSpeed);
        }

        private void OnRotate(Vector3 rotation)
        {
            _motor.Turn(rotation.y * _turnSpeed);
            _motor.Look(rotation.x * _turnSpeed);
        }

        private void OnJump()
        {
            _motor.Jump(_jumpStrength);
        }
    }
}
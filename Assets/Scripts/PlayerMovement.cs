using UnityEngine;

namespace Assets.Scripts {
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour {
        private CharacterController _characterController;

        [SerializeField] private float _walkingSpeed = 6f;
        [SerializeField] private float _runningSpeed = 10f;
        [SerializeField] private float _gravity = -9.81f;
        [SerializeField] private float _jumpHeight = 3f;

        [SerializeField] private Transform _groundCheck = null;
        [SerializeField] private float _groundDistance = 0.45f;
        [SerializeField] private LayerMask _groundMask;

        private Vector3 _velocity;
        private bool _isGrounded;

        [HideInInspector] public bool _isAlive = true;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
        }

        private void Start()
        {
            if (_groundCheck == null) {
                Debug.Log("Warning: Player Movement Controller is missing a Ground Check Transform");
            }
        }

        private void Update()
        {
            if (!_isAlive) return;

            _isGrounded = _velocity.y < 0 && Physics.CheckSphere(_groundCheck.position, _groundDistance, _groundMask);

            if (_isGrounded && _velocity.y < 0) {
                _velocity.y = -2f;
            }

            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            Vector3 move = transform.right * x + transform.forward * z;
            float speed = Input.GetKey(KeyCode.LeftShift) ? _runningSpeed : _walkingSpeed;
            _characterController.Move(move * speed * Time.deltaTime);

            if (Input.GetButtonDown("Jump") & _isGrounded) {
                _velocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
            }

            _velocity.y += _gravity * Time.deltaTime;

            _characterController.Move(_velocity * Time.deltaTime);
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utility;
using Utility.References;

namespace Player
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float _moveSpeed = 5;
        [SerializeField] private float _runSpeed = 8;
        [SerializeField] private float _jumpSpeed = 1.6f;
        [SerializeField] private float _airAcceleration = 8;
        [SerializeField] private float _airResistance = 8;
        private Rigidbody _rigidbody;
        private float _movementForwards;
        private float _movementRight;
        private float _movementMultiplier = 10f;
        private bool _isRunning;
        private bool _wishJump;

        [Header("Ground Detection")]
        [SerializeField] private Vector3 _groundCheck = Vector3.down;
        [SerializeField] private float _groundCheckRadius = 0.25f;
        [SerializeField] private LayerMask _groundMask = 0;
        [SerializeField] private float _groundMoveDownForce = 80f;
        private CapsuleCollider _collider;
        private Collider[] _groundCheckCollidingWith = { };
        private List<Collider> _colliderCollidingWith = new List<Collider>();
        private bool _isGrounded;

        [Header("Slopes")]
        [SerializeField] private float _slopeRaycastExtension = 0.8f;
        [SerializeField] private float _maxSlopeAngle = 60f;
        [SerializeField] private float _slopeHopCounterForce = 85f;
        [SerializeField] private float _slopeHopHitDistance = .1f;
        private RaycastHit _currentSlopeRaycastHit;
        private Vector3 _groundSlopeDirection;
        private float _groundSlopeAngle;
        private float _minSlopeHopSpeed = .5f;
        private float _maxSlopeHopSpeed = 100f;

        [Header("World")]
        [SerializeField] private Vector3 _gravity = Vector3.down * 9.81f;
        [SerializeField] private float _groundDrag = 5f;
        [SerializeField] private float _airDrag = 2f;

        [Header("References")]
        [SerializeField] private TransformVariable _transform = null;
        [SerializeField] private BoolVariable _playerHasControl = null;

        private bool CanWalkSlope => _groundSlopeAngle <= _maxSlopeAngle;
        private bool DetectGroundCheckMatchesCollision => (from c1 in _groundCheckCollidingWith from c2 in _colliderCollidingWith where c1 == c2 select c1).Any();

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<CapsuleCollider>();
            if (_transform != null) _transform.Position = transform.position;
            if (_transform == null) Debug.Log("[" + GetType().Name + "] Transform Variable missing on " + name);
            if (_playerHasControl == null) Debug.Log("[" + GetType().Name + "] Player Has Control Bool Variable missing on " + name);
        }

        private void OnCollisionEnter(Collision collision)
        {
            _colliderCollidingWith.Add(collision.collider);
        }

        private void OnCollisionExit(Collision collision)
        {
            if (_colliderCollidingWith.Contains(collision.collider)) _colliderCollidingWith.Remove(collision.collider);
        }

        private void Update()
        {
            if (_playerHasControl != null && !_playerHasControl.Value) {
                _rigidbody.drag = 10;
                return;
            }

            _movementForwards = Input.GetAxisRaw("Vertical");
            _movementRight = Input.GetAxisRaw("Horizontal");

            if (Input.GetButtonDown("Run") && !_isRunning) _isRunning = true;
            if (Input.GetButtonUp("Run")) _isRunning = false;

            if (Input.GetButtonDown("Jump") && !_wishJump) _wishJump = true;
            if (Input.GetButtonUp("Jump")) _wishJump = false;

            _groundCheckCollidingWith = Physics.OverlapSphere(transform.position + _groundCheck, _groundCheckRadius, _groundMask);
            _isGrounded = _groundCheckCollidingWith.Length > 0;

            SwitchDrag();

            if (_isGrounded && _wishJump && (!DetectSlope() || CanWalkSlope)) Jump();

            _transform.Position = transform.position;
        }

        private void FixedUpdate()
        {
            if (_playerHasControl != null && !_playerHasControl.Value) return;
            Move();
        }

        private void Move()
        {
            // Calculate how fast we should be moving
            Vector3 moveDirection = (_movementForwards * transform.forward + _movementRight * transform.right).normalized;
            Vector3 slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, _currentSlopeRaycastHit.normal).normalized;
            Vector3 force;

            if (_isGrounded && !DetectSlope()) {
                force = moveDirection * (_isRunning ? _runSpeed : _moveSpeed) * _movementMultiplier;

                if (!DetectGroundCheckMatchesCollision) {
                    force += -transform.up * _groundMoveDownForce;
                }

                if (!Physics.Raycast(transform.position - (transform.up * _collider.height) / 2 + (moveDirection * _slopeHopHitDistance) + (transform.up * _slopeHopHitDistance), -_currentSlopeRaycastHit.normal, _slopeHopHitDistance + .1f, _groundMask)) {
                    if (_rigidbody.velocity.magnitude > _minSlopeHopSpeed && _rigidbody.velocity.magnitude < _maxSlopeHopSpeed) {
                        force += -_currentSlopeRaycastHit.normal * _slopeHopCounterForce;
                    }
                }
                //Debug.Log("Ground walk: " + force);
            } else if (_isGrounded && DetectSlope() && CanWalkSlope) {
                force = slopeMoveDirection * (_isRunning ? _runSpeed : _moveSpeed) * _movementMultiplier;

                if (!DetectGroundCheckMatchesCollision) {
                    force += -transform.up * _groundMoveDownForce;
                }

                if (!Physics.Raycast(transform.position - (transform.up * _collider.height) / 2 + (slopeMoveDirection * _slopeHopHitDistance) + (transform.up * _slopeHopHitDistance), -_currentSlopeRaycastHit.normal, _slopeHopHitDistance + .2f, _groundMask)) {
                    if (_rigidbody.velocity.magnitude > _minSlopeHopSpeed && _rigidbody.velocity.magnitude < _maxSlopeHopSpeed) {
                        force += -_currentSlopeRaycastHit.normal * _slopeHopCounterForce;
                    }
                }
                //Debug.Log("Slope walk: " + force);
            } else {
                force = (moveDirection * _airAcceleration * _airResistance + _gravity);
                //Debug.Log("Air walk: " + force);
            }

            _rigidbody.AddForce(force, ForceMode.Acceleration);
        }

        private void Jump()
        {
            float jumpForce = Mathf.Sqrt(2 * _jumpSpeed * _gravity.magnitude);
            _rigidbody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }

        private bool DetectSlope()
        {
            Physics.Raycast(transform.position, Vector3.down, out _currentSlopeRaycastHit, (_collider.height / 2) + _slopeRaycastExtension, _groundMask);
            if (_currentSlopeRaycastHit.collider == null || _currentSlopeRaycastHit.normal == Vector3.up) return false;

            Vector3 slopeCross = Vector3.Cross(_currentSlopeRaycastHit.normal, Vector3.down);
            _groundSlopeDirection = Vector3.Cross(slopeCross, _currentSlopeRaycastHit.normal);
            _groundSlopeAngle = Vector3.Angle(_currentSlopeRaycastHit.normal, Vector3.up);
            return true;
        }

        private void SwitchDrag()
        {
            _rigidbody.drag = _isGrounded ? _groundDrag : _airDrag;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + _groundCheck, _groundCheckRadius);
        }
    }
}
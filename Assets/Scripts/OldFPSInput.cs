using System;
using UnityEngine;

namespace Assets.Scripts {
    public class OldFPSInput : MonoBehaviour {
        [SerializeField] private bool _invertVertical = false;

        public event Action<Vector3> MoveInput = delegate { };
        public event Action<Vector3> RotateInput = delegate { };
        public event Action JumpInput = delegate { };

        private void Update()
        {
            DetectMoveInput();
            DetectRotateInput();
            DetectJumpInput();
        }

        private void DetectMoveInput()
        {
            float xInput = Input.GetAxisRaw("Horizontal");
            float yInput = Input.GetAxisRaw("Vertical");
            if (xInput == 0 && yInput == 0) return;

            Vector3 horizontalMovement = transform.right * xInput;
            Vector3 forwardMovement = transform.forward * yInput;

            Vector3 movement = (horizontalMovement + forwardMovement).normalized;

            MoveInput?.Invoke(movement);
        }

        private void DetectRotateInput()
        {
            float xInput = Input.GetAxisRaw("Mouse X");
            float yInput = Input.GetAxisRaw("Mouse Y");
            if (xInput == 0 && yInput == 0) return;

            if (_invertVertical) {
                yInput = -yInput;
            }

            Vector3 rotation = new Vector3(yInput, xInput, 0);
            RotateInput?.Invoke(rotation);
        }

        private void DetectJumpInput()
        {
            if (Input.GetKeyDown(KeyCode.Space)) {
                JumpInput?.Invoke();
            }
        }
    }
}
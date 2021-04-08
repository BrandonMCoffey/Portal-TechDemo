using UnityEngine;

namespace Assets.Scripts.Interactables {
    public class InteractableObject : MonoBehaviour {
        private bool _held;
        private Transform _parent;
        private Rigidbody _rigidbody;
        private Transform _cameraTransform;
        private float _cameraOffset;

        private void Awake()
        {
            _parent = transform.parent;
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (!_held || _cameraTransform == null) return;
            transform.position = _cameraTransform.position + _cameraTransform.forward * _cameraOffset;
        }

        public void Interact(Transform cameraT, float dist)
        {
            _held = true;
            _cameraTransform = cameraT;
            _cameraOffset = dist;
            transform.SetParent(cameraT);
        }

        public void Drop()
        {
            _held = false;
            transform.parent = _parent;
        }
    }
}
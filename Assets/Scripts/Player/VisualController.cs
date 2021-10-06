using UnityEngine;

namespace Player
{
    public class VisualController : MonoBehaviour
    {
        [SerializeField] private Transform _weaponTransform = null;
        [SerializeField] private Transform _referenceTransform = null;

        [SerializeField] private float _moveSpeed = 80;
        [SerializeField] private float _rotateSpeed = 16;

        private bool _isClone;
        private VisualController _originalController;

        public Transform WeaponTransform => _weaponTransform;

        public void SetClone(GameObject original)
        {
            _isClone = true;
            _originalController = original.GetComponent<VisualController>();
        }

        private void Update()
        {
            if (_weaponTransform == null || _referenceTransform == null || _isClone && _originalController == null) {
                Debug.Log("Missing references", gameObject);
                return;
            }

            if (_isClone) {
                Transform original = _originalController.WeaponTransform;
                _weaponTransform.localPosition = original.localPosition;
                _weaponTransform.localRotation = original.localRotation;
                return;
            }

            Vector3 smoothedPosition = Vector3.Lerp(_weaponTransform.position, _referenceTransform.position, _moveSpeed * Time.deltaTime);
            _weaponTransform.position = smoothedPosition;

            Vector3 desiredRotation = _referenceTransform.eulerAngles;
            desiredRotation.y = 0;
            Quaternion smoothedRotation = Quaternion.Lerp(_weaponTransform.localRotation, Quaternion.Euler(desiredRotation), _rotateSpeed * Time.deltaTime);
            _weaponTransform.localRotation = smoothedRotation;
        }
    }
}
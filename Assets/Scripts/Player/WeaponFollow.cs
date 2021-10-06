using UnityEngine;
using Utility.References;

namespace Player
{
    public class WeaponFollow : MonoBehaviour
    {
        [SerializeField] private TransformReference _transformToFollow = null;
        [SerializeField] private float _moveSpeed = 5;
        [SerializeField] private float _rotateSpeed = 1;

        Vector3 _offset = Vector3.zero;

        private void Update()
        {
            if (_transformToFollow == null) return;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, _transformToFollow.Position, _moveSpeed * Time.deltaTime);
            transform.position = smoothedPosition;
            Vector3 desiredRotation = _transformToFollow.Rotation.eulerAngles;
            Vector3 smoothedRotation = Vector3.Lerp(transform.eulerAngles, desiredRotation, _rotateSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(smoothedRotation);
        }

        public void Teleporting()
        {
        }
    }
}
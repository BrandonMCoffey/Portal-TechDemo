using Assets.Scripts.Utility;
using UnityEngine;

namespace Assets.Scripts.Player {
    public class WeaponFollow : MonoBehaviour {
        [SerializeField] private TransformReference _transformToFollow = null;
        [SerializeField] private float _followSpeed = 0.5f;

        private void Update()
        {
            if (_transformToFollow == null) return;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, _transformToFollow.Position, _followSpeed);
            transform.position = smoothedPosition;
            Vector3 desiredRotation = _transformToFollow.Rotation.eulerAngles;
            Quaternion smoothedRotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(desiredRotation), _followSpeed);
            transform.rotation = smoothedRotation;
        }
    }
}
using Assets.Scripts.Utility;
using UnityEngine;

namespace Assets.Scripts {
    public class PlayerCamera : MonoBehaviour {
        [Header("Settings")]
        [SerializeField] private Vector3 _positionOffset = Vector3.up;

        [Header("References")]
        [SerializeField] private TransformVariable _transformToFollow = null;

        private void Update()
        {
            transform.position = _transformToFollow.Position + _positionOffset;
            transform.rotation = _transformToFollow.Rotation;
        }
    }
}
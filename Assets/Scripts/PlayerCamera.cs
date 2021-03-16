using Assets.Scripts.Portals;
using Assets.Scripts.Utility;
using UnityEngine;

namespace Assets.Scripts {
    [RequireComponent(typeof(Camera))]
    public class PlayerCamera : MonoBehaviour {
        [Header("Settings")]
        [SerializeField] private Vector3 _positionOffset = Vector3.up;

        [Header("References")]
        [SerializeField] private TransformVariable _transformToFollow = null;
        [SerializeField] private PortalController _portalController = null;

        private Camera _camera;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void Update()
        {
            transform.position = _transformToFollow.Position + _positionOffset;
            transform.rotation = _transformToFollow.Rotation;
        }

        private void OnPreCull()
        {
            if (_portalController != null) _portalController.RenderPortals(_camera);
        }
    }
}
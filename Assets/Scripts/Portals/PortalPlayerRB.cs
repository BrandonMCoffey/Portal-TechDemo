using Assets.Scripts.GameEvents.Logic;
using Assets.Scripts.Player;
using UnityEngine;

namespace Assets.Scripts.Portals {
    [RequireComponent(typeof(Rigidbody), typeof(PlayerLook))]
    public class PortalPlayerRB : PortalTraveler {
        [SerializeField] private GameEvent _onTravel = null;

        private PlayerLook _playerLook;

        private void Start()
        {
            _playerLook = GetComponent<PlayerLook>();
        }

        public override void Teleport(Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot)
        {
            if (_onTravel != null) _onTravel.Raise();
            transform.position = pos;
            Vector3 eulerRot = rot.eulerAngles;
            float delta = Mathf.DeltaAngle(_playerLook.SmoothYaw, eulerRot.y);
            _playerLook.Yaw += delta;
            _playerLook.SmoothYaw += delta;
            transform.eulerAngles = Vector3.up * _playerLook.SmoothYaw;
            Rigidbody.velocity = toPortal.TransformVector(fromPortal.InverseTransformVector(Rigidbody.velocity));
            Rigidbody.angularVelocity = toPortal.TransformVector(fromPortal.InverseTransformVector(Rigidbody.angularVelocity));
            Physics.SyncTransforms();
        }
    }
}
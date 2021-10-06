using System.Collections.Generic;
using Player;
using UnityEngine;
using Utility.GameEvents.Logic;

namespace Portals
{
    [RequireComponent(typeof(PlayerMovementCC))]
    public class PortalPlayerCC : PortalTraveler
    {
        [SerializeField] private GameEvent _onTravel = null;
        [SerializeField] private WeaponFollow _weaponFollow = null;

        private PlayerMovementCC _playerMovement;

        private void Start()
        {
            _playerMovement = GetComponent<PlayerMovementCC>();
        }

        public override void Teleport(Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot)
        {
            if (_onTravel != null) _onTravel.Raise();
            transform.position = pos;
            Vector3 eulerRot = rot.eulerAngles;
            float delta = Mathf.DeltaAngle(_playerMovement.SmoothYaw, eulerRot.y);
            _playerMovement.Yaw += delta;
            _playerMovement.SmoothYaw += delta;
            transform.eulerAngles = Vector3.up * _playerMovement.SmoothYaw;

            if (_weaponFollow != null) {
                _weaponFollow.Teleporting();
            }

            _playerMovement.Velocity = toPortal.TransformVector(fromPortal.InverseTransformVector(_playerMovement.Velocity));
            //_playerMovement.angularVelocity = toPortal.TransformVector(fromPortal.InverseTransformVector(_playerMovement.angularVelocity));
            Physics.SyncTransforms();
        }
    }
}
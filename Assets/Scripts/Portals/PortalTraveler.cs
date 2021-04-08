using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Portals {
    [RequireComponent(typeof(Collider))]
    public class PortalTraveler : MonoBehaviour {
        [SerializeField] private MeshRenderer _travelerArt = null;

        private GameObject _clone;
        private Portal _inPortal;
        private Portal _outPortal;
        private Collider _collider;
        private MeshFilter _meshFilter;

        private static readonly Quaternion HalfTurn = Quaternion.Euler(0.0f, 180.0f, 0.0f);

        private void Awake()
        {
            _collider = GetComponent<Collider>();

            _clone = new GameObject(name + "_Clone");
            _clone.SetActive(false);
            var meshFilter = _clone.AddComponent<MeshFilter>();
            var meshRenderer = _clone.AddComponent<MeshRenderer>();

            if (_travelerArt == null) return;
            _meshFilter = _travelerArt.GetComponent<MeshFilter>();
            meshFilter.mesh = _meshFilter.mesh;
            meshRenderer.materials = _travelerArt.materials;
            _clone.transform.localScale = transform.localScale;
        }

        private void LateUpdate()
        {
            if (_inPortal == null || _outPortal == null) return;

            if (_clone.activeSelf) {
                var inTransform = _inPortal.transform;
                var outTransform = _outPortal.transform;

                // Update clone
                Vector3 relativePos = inTransform.InverseTransformPoint(transform.position);
                relativePos = HalfTurn * relativePos;
                _clone.transform.position = outTransform.TransformPoint(relativePos);
                Quaternion relativeRot = Quaternion.Inverse(inTransform.rotation) * transform.rotation;
                relativeRot = HalfTurn * relativeRot;
                _clone.transform.rotation = outTransform.rotation * relativeRot;
            }
        }

        public void SetInPortal(Portal inPortal, Portal outPortal)
        {
            _inPortal = inPortal;
            _outPortal = outPortal;
            Physics.IgnoreCollision(_collider, inPortal.WallCollider);
            _clone.SetActive(true);
        }

        public void ExitPortal()
        {
            Physics.IgnoreCollision(_collider, _inPortal.WallCollider, false);
            _clone.SetActive(false);
        }

        public void Warp()
        {
            var inTransform = _inPortal.transform;
            var outTransform = _outPortal.transform;

            // Update position of object.
            Vector3 relativePos = inTransform.InverseTransformPoint(transform.position);
            relativePos = HalfTurn * relativePos;
            transform.position = outTransform.TransformPoint(relativePos);

            // Update rotation of object.
            Quaternion relativeRot = Quaternion.Inverse(inTransform.rotation) * transform.rotation;
            relativeRot = HalfTurn * relativeRot;
            transform.rotation = outTransform.rotation * relativeRot;

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null) {
                // Update velocity of rigidbody.
                Vector3 relativeVel = inTransform.InverseTransformDirection(rb.velocity);
                relativeVel = HalfTurn * relativeVel;
                rb.velocity = outTransform.TransformDirection(relativeVel);
            }

            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null) {
                // Update velocity of rigidbody.
                Vector3 relativeVel = inTransform.InverseTransformDirection(cc.velocity);
                relativeVel = HalfTurn * relativeVel;
                Vector3 vel = outTransform.TransformDirection(relativeVel);
                cc.velocity.Set(vel.x, vel.y, vel.z);
            }

            ExitPortal();

            // Swap portal references.
            //var tempPortal = _inPortal;
            //_inPortal = _outPortal;
            //_outPortal = tempPortal;
        }
    }
}
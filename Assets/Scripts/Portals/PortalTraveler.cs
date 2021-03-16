using UnityEngine;

namespace Assets.Scripts.Portals {
    [RequireComponent(typeof(Collider), typeof(Rigidbody))]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class PortalTraveler : MonoBehaviour {
        private GameObject _clone;
        private Portal _inPortal;
        private Portal _outPortal;
        private Collider _collider;
        private Rigidbody _rigidbody;

        private static readonly Quaternion HalfTurn = Quaternion.Euler(0.0f, 180.0f, 0.0f);

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _rigidbody = GetComponent<Rigidbody>();

            _clone = new GameObject(name + "_Clone");
            _clone.SetActive(false);
            var meshFilter = _clone.AddComponent<MeshFilter>();
            var meshRenderer = _clone.AddComponent<MeshRenderer>();

            meshFilter.mesh = GetComponent<MeshFilter>().mesh;
            meshRenderer.materials = GetComponent<MeshRenderer>().materials;
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

            // Update velocity of rigidbody.
            Vector3 relativeVel = inTransform.InverseTransformDirection(_rigidbody.velocity);
            relativeVel = HalfTurn * relativeVel;
            _rigidbody.velocity = outTransform.TransformDirection(relativeVel);

            // Swap portal references.
            var tempPortal = _inPortal;
            _inPortal = _outPortal;
            _outPortal = tempPortal;
        }
    }
}
using System.Linq;
using UnityEngine;

namespace Portals
{
    [RequireComponent(typeof(Collider))]
    public class PortalTraveler : MonoBehaviour
    {
        public GameObject GraphicsObject = null;

        public GameObject GraphicsClone { get; set; }
        public Vector3 PreviousOffsetFromPortal { get; set; }
        public Material[] OriginalMaterials { get; set; }
        public Material[] CloneMaterials { get; set; }

        internal Collider Collider;
        internal Rigidbody Rigidbody;

        internal Portal InPortal;

        private Collider _tempCollider;

        private void Awake()
        {
            Collider = GetComponent<Collider>();
            Rigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (_tempCollider != null && InPortal.LinkedPortal.isActiveAndEnabled) {
                Physics.IgnoreCollision(Collider, _tempCollider, true);
                _tempCollider = null;
            }
        }

        public virtual void Teleport(Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot)
        {
            Debug.Log("Teleport");
            transform.position = pos;
            transform.rotation = rot;
            if (Rigidbody != null) {
                Rigidbody.velocity = toPortal.TransformVector(fromPortal.InverseTransformVector(Rigidbody.velocity));
                Rigidbody.angularVelocity = toPortal.TransformVector(fromPortal.InverseTransformVector(Rigidbody.angularVelocity));
            }
            Physics.SyncTransforms();
        }

        // Called when first touches portal
        public void EnterPortalThreshold(Portal inPortal)
        {
            InPortal = inPortal;
            if (GraphicsClone == null) {
                GraphicsClone = Instantiate(GraphicsObject);
                GraphicsClone.transform.SetParent(GraphicsObject.transform.parent);
                GraphicsClone.transform.localScale = GraphicsObject.transform.localScale;
                OriginalMaterials = GetMaterials(GraphicsObject);
                CloneMaterials = GetMaterials(GraphicsClone);
            } else {
                GraphicsClone.SetActive(true);
            }
            if (inPortal.LinkedPortal != null) {
                if (inPortal.LinkedPortal.isActiveAndEnabled) {
                    Physics.IgnoreCollision(Collider, inPortal.WallCollider, true);
                } else {
                    _tempCollider = inPortal.WallCollider;
                }
            }
        }

        // Called once no longer touching portal (excluding when teleporting)
        public void ExitPortalThreshold(Portal inPortal)
        {
            InPortal = null;
            GraphicsClone.SetActive(false);
            // Disable slicing
            foreach (var mat in OriginalMaterials) {
                mat.SetVector("sliceNormal", Vector3.zero);
            }
            if (inPortal.WallCollider != null) {
                Physics.IgnoreCollision(Collider, inPortal.WallCollider, false);
                _tempCollider = null;
            }
        }

        public void SetSliceOffsetDst(float dst, bool clone)
        {
            for (int i = 0; i < OriginalMaterials.Length; i++) {
                if (clone) {
                    CloneMaterials[i].SetFloat("sliceOffsetDst", dst);
                } else {
                    OriginalMaterials[i].SetFloat("sliceOffsetDst", dst);
                }
            }
        }

        private static Material[] GetMaterials(GameObject obj)
        {
            var renderers = obj.GetComponentsInChildren<MeshRenderer>();
            return renderers.SelectMany(r => r.materials).ToArray();
        }
    }
}
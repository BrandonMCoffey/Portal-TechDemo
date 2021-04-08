using Assets.Scripts.Utility;
using UnityEngine;

namespace Assets.Scripts.Portals {
    [CreateAssetMenu]
    public class PortalVariable : TransformVariable {
        public bool ValidLocation;
        public int PortalID;
        public Collider Collider;
    }
}
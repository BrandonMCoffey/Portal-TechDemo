using UnityEngine;

namespace Assets.Scripts.Utility {
    [CreateAssetMenu]
    public class PortalVariable : TransformVariable {
        public int PortalID;
        public Collider Collider;

        public override void ResetData()
        {
            base.ResetData();
            PortalID = -1;
            Collider = null;
        }
    }
}
using UnityEngine;
using Utility.References;

namespace Portals
{
    [CreateAssetMenu]
    public class PortalVariable : TransformVariable
    {
        public bool ValidLocation;
        public int PortalID;
        public Collider Collider;
    }
}
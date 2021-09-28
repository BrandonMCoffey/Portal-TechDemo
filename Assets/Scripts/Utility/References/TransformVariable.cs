using UnityEngine;

namespace Utility.References
{
    [CreateAssetMenu]
    public class TransformVariable : ScriptableObject
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Forward;
    }
}
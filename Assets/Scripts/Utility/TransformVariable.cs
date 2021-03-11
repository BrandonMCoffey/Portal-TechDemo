using UnityEngine;

namespace Assets.Scripts.Utility {
    [CreateAssetMenu]
    public class TransformVariable : ScriptableObject {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Forward;
    }
}
using UnityEngine;

namespace Assets.Scripts.Utility {
    [CreateAssetMenu]
    public class TransformVariable : ScriptableObject {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Forward;

        public virtual void ResetData()
        {
            Position = Vector3.zero;
            Rotation = Quaternion.identity;
            Forward = Vector3.zero;
        }
    }
}
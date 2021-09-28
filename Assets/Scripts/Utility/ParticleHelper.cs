using UnityEngine;
using Utility.References;

namespace Utility
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleHelper : MonoBehaviour
    {
        [SerializeField] private TransformVariable _transform = null;
        [SerializeField] private Vector3 _transformAdjust = Vector3.zero;
        [SerializeField] private bool _useDirection = false;
        [SerializeField] private Vector3 _rotationAdjust = Vector3.zero;

        private ParticleSystem _particles;

        private void Awake()
        {
            if (_transform == null) Debug.Log("[" + GetType().Name + "] Transform Variable missing on " + name);
            _particles = GetComponent<ParticleSystem>();
        }

        public void EmitAtLocation(int amount)
        {
            if (_transform == null || _transform.Position == Vector3.zero) return;
            transform.position = _transform.Position + _transformAdjust;
            if (_useDirection) {
                transform.rotation = _transform.Rotation * Quaternion.Euler(_rotationAdjust);
            }
            _particles.Emit(amount);
        }
    }
}
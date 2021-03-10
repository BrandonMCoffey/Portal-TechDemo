using Assets.Scripts.Utility;
using UnityEngine;

namespace Assets.Scripts {
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleHelper : MonoBehaviour {
        [SerializeField] private TransformVariable _transform = null;
        [SerializeField] private Vector3 _transformOverride = Vector3.zero;

        private ParticleSystem _particles;

        private void Awake()
        {
            if (_transform == null) Debug.Log("[" + GetType().Name + "] Transform Variable missing on " + name);
            _particles = GetComponent<ParticleSystem>();
        }

        public void EmitAtLocation(int amount)
        {
            if (_transform == null || _transform.Position == Vector3.zero) return;
            transform.position = _transform.Position + _transformOverride;
            _particles.Emit(amount);
        }
    }
}
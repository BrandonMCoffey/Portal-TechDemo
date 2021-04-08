using UnityEngine;

namespace Assets.Scripts.Enemy {
    [RequireComponent(typeof(BoxCollider))]
    public class Enemy : MonoBehaviour {
        internal BoxCollider _collider;

        public virtual bool IsActive => false;

        private void Awake()
        {
            _collider = GetComponent<BoxCollider>();
        }

        public virtual void PlayerInRange(bool active)
        {
        }

        public virtual void OnHit()
        {
        }
    }
}
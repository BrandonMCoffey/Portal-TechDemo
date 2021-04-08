using System.Collections;
using Assets.Scripts.Player;
using Assets.Scripts.Utility;
using UnityEngine;

namespace Assets.Scripts.Enemy {
    [RequireComponent(typeof(Collider))]
    public class EnemyBullet : MonoBehaviour {
        [SerializeField] private float _speed = 5;
        [SerializeField] private float _damage = 1;
        [SerializeField] private float _lifespan = 5;

        private Coroutine _deathCoroutine;

        private void OnEnable()
        {
            _deathCoroutine = StartCoroutine(DeathTimeout());
        }

        private void OnDisable()
        {
            StopCoroutine(_deathCoroutine);
        }

        private void Update()
        {
            transform.position += transform.forward * _speed * Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null) playerHealth.Damage(_damage);
            Kill();
        }

        private void Kill()
        {
            if (_deathCoroutine != null) StopCoroutine(_deathCoroutine);
            Destroy(gameObject);
        }

        private IEnumerator DeathTimeout()
        {
            yield return new WaitForSeconds(_lifespan);
            Kill();
        }
    }
}
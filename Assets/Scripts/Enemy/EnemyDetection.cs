using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Utility.References;

namespace Enemy
{
    public class EnemyDetection : MonoBehaviour
    {
        [SerializeField] private float _searchRadius = 5;
        [SerializeField] private float _enemyDelay = 0.25f;
        [SerializeField] private TransformReference _playerTransform = null;
        [SerializeField] private LayerMask _playerMask = 0;
        [SerializeField] private UnityEvent _onEnter = new UnityEvent();
        [SerializeField] private UnityEvent _onExit = new UnityEvent();

        private bool _playerInRange;
        private bool _canSeePlayer;
        private bool _updateDelay;

        private void Update()
        {
            if (_playerTransform == null || _updateDelay) return;
            _playerInRange = Physics.CheckSphere(transform.position, _searchRadius, _playerMask);
            if (!_playerInRange && _canSeePlayer) {
                _onExit.Invoke();
                _canSeePlayer = false;
            }
            if (!_playerInRange) return;

            StartCoroutine(DelayEnemy());
            Physics.Linecast(transform.position, _playerTransform.Position, out var hit);

            bool lineOfSight = (hit.transform == null || (1 << hit.transform.gameObject.layer & _playerMask) != 0 || hit.transform == transform);
            if (!_canSeePlayer && lineOfSight) {
                _canSeePlayer = true;
                _onEnter.Invoke();
            } else if (_canSeePlayer && !lineOfSight) {
                _canSeePlayer = false;
                _onExit.Invoke();
            }
            StartCoroutine(DelayEnemy());
        }

        private IEnumerator DelayEnemy()
        {
            _updateDelay = true;
            for (float t = 0; t <= _enemyDelay; t += Time.deltaTime) {
                yield return null;
            }
            _updateDelay = false;
        }
    }
}
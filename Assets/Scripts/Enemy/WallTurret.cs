using System.Collections;
using Assets.Scripts.Audio;
using Assets.Scripts.GameEvents.Logic;
using Assets.Scripts.Portals;
using Assets.Scripts.Utility;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Enemy {
    public class WallTurret : Enemy {
        [SerializeField] private float _openTime = 1;
        [SerializeField] private Transform _panelTransform = null;
        [SerializeField] private float _panelOpeningAmount = 100;
        [SerializeField] private Transform _turretGun = null;
        [SerializeField, Range(0, 1)] private float _turretGunSwivelSpeed = 0.5f;
        [SerializeField] private Transform _turretGunFire = null;
        [SerializeField] private GameObject _turretBullet = null;
        [SerializeField] private float _fireDelay = 1;
        [SerializeField] private TransformReference _playerTransform = null;
        [SerializeField] private WallTurretHealth _health = null;
        [SerializeField] private PlayFromSource _turretAudio = null;
        [SerializeField] private GameEvent _refreshPortals = null;
        [SerializeField] private UnityEvent _onDeath = new UnityEvent();

        private Coroutine _currentCoroutine;
        private Coroutine _bulletCoroutine;

        private bool _playerInRange;
        private float _openState;

        private bool _isAlive = true;
        private bool _canFire = true;

        public override bool IsActive => _openState == 0;

        private void Update()
        {
            if (!_isAlive) return;
            if (_panelTransform != null) {
                _panelTransform.localEulerAngles = new Vector3(_openState * _panelOpeningAmount, 0, 0);
            }
            if (_turretGun != null && _openState > 0.9f) {
                LookAtPlayer();
                if (_turretGunFire != null && _turretBullet != null && _canFire) {
                    _bulletCoroutine = StartCoroutine(FireBullet(_openState > 0.95f));
                }
            }
            if (_openState > 0.1f && !_collider.enabled) {
                _collider.enabled = true;
            } else if (_openState < 0.1f && _collider.enabled) {
                _collider.enabled = false;
            }
        }

        public override void PlayerInRange(bool active)
        {
            _playerInRange = active;
            if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);
            _currentCoroutine = StartCoroutine(_playerInRange ? OpenSequence() : CloseSequence());
        }

        private void LookAtPlayer()
        {
            Quaternion currentRotation = _turretGun.localRotation;
            _turretGun.LookAt(_playerTransform.Position + Vector3.up);
            _turretGun.Rotate(new Vector3(0, 90, _openState * _panelOpeningAmount));
            Quaternion desiredRotation = _turretGun.localRotation;
            _turretGun.localRotation = Quaternion.Lerp(currentRotation, desiredRotation, _turretGunSwivelSpeed);
        }

        public override void OnHit()
        {
            if (_turretAudio != null) _turretAudio.PlayOneShot(1);
            if (_health != null) {
                if (_health.Damage()) Shutdown();
            } else {
                Shutdown();
            }
        }

        private void Shutdown()
        {
            _onDeath.Invoke();
            if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);
            _currentCoroutine = StartCoroutine(ShutdownSequence());
        }

        public void EnableBullets(bool active)
        {
            if (_bulletCoroutine != null) StopCoroutine(_bulletCoroutine);
            if (active) {
                _bulletCoroutine = StartCoroutine(FireBullet(false));
            }
            _canFire = false;
        }

        private void KillPortals()
        {
            Collider[] hitColliders = Physics.OverlapBox(transform.position + _collider.center, _collider.size / 2);
            foreach (var col in hitColliders) {
                Portal portal = col.GetComponent<Portal>();
                if (portal != null) {
                    portal.gameObject.SetActive(false);
                    Debug.Log("Boom");
                    if (_refreshPortals != null) _refreshPortals.Raise();
                }
            }
        }

        private IEnumerator OpenSequence()
        {
            if (_openState < 0.05f) KillPortals();
            if (_turretAudio != null) _turretAudio.PlayOneShot(2);
            for (float t = _openState * _openTime; t <= _openTime; t += Time.deltaTime) {
                _openState = t / _openTime;
                yield return null;
            }
            _openState = 1;
        }

        private IEnumerator CloseSequence()
        {
            for (float t = _openTime - _openState * _openTime; t <= _openTime; t += Time.deltaTime) {
                _openState = 1 - t / _openTime;
                yield return null;
            }
            _openState = 0;
        }

        private IEnumerator ShutdownSequence()
        {
            for (float t = _openTime - _openState * _openTime; t <= _openTime; t += Time.deltaTime) {
                _openState = 1 - t / _openTime;
                yield return null;
            }
            _openState = 0;
            _collider.enabled = false;
            _isAlive = false;
        }

        private IEnumerator FireBullet(bool fire)
        {
            _canFire = false;
            if (fire) {
                Instantiate(_turretBullet, _turretGunFire.position, _turretGunFire.rotation);
                if (_turretAudio != null) _turretAudio.PlayOneShot(0);
            }
            yield return new WaitForSeconds(_fireDelay);
            _canFire = true;
        }
    }
}
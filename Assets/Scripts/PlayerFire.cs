using System.Collections;
using System.Security.Cryptography;
using UnityEngine;

namespace Assets.Scripts {
    public class PlayerFire : MonoBehaviour {
        [SerializeField] private ParticleSystem _muzzleFlash = null;
        [SerializeField] private ParticleSystem _hitSparks = null;
        [SerializeField] private AudioClip _fireWeaponSoundEffect = null;

        [HideInInspector] public bool _isAlive = true;

        private void Awake()
        {
            if (_muzzleFlash == null) {
                Debug.Log("Warning: Player is missing the Muzzle Flash Particle System");
            }

            if (_hitSparks == null) {
                Debug.Log("Warning: Player is missing the Hit Sparks Particle System");
            }

            if (_fireWeaponSoundEffect == null) {
                Debug.Log("Warning: Player is missing the Weapon Fire Sound Effect");
            }
        }

        private void Update()
        {
            if (!_isAlive) return;

            if (Input.GetKeyDown(KeyCode.Mouse0)) {
                FireWeapon();
            }
        }

        private void FireWeapon()
        {
            _muzzleFlash.Emit(5);
            StartCoroutine(CreateWeaponFireSound());
        }

        private IEnumerator CreateWeaponFireSound()
        {
            GameObject soundPlayer = new GameObject("WeaponFireSound");
            soundPlayer.transform.parent = _muzzleFlash.transform.parent;
            AudioSource sound = soundPlayer.AddComponent<AudioSource>();
            sound.clip = _fireWeaponSoundEffect;
            sound.Play();
            yield return new WaitForSeconds(1);
            Destroy(soundPlayer);
        }
    }
}
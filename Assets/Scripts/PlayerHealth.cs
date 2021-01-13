using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Assets.Scripts {
    public class PlayerHealth : MonoBehaviour {
        [SerializeField] private Slider _healthSliderView = null;
        [SerializeField] private int _maxHealth = 4;
        [SerializeField] private ParticleSystem _deathParticles = null;

        [SerializeField] private AudioClip _playerHurtSoundEffect = null;
        [SerializeField] private AudioClip _playerDeathSoundEffect = null;

        private int _currentHealth;
        private bool _isAlive = true;

        private void Start()
        {
            _currentHealth = _maxHealth;

            if (_healthSliderView == null) {
                Debug.Log("Warning: Player Health is missing the Health Slider");
            } else {
                _healthSliderView.maxValue = _maxHealth;
                _healthSliderView.value = _maxHealth;
            }

            if (_deathParticles == null) {
                Debug.Log("Warning: Player Health is missing the Death Particles");
            }

            if (_playerHurtSoundEffect == null) {
                Debug.Log("Warning: Player Health is missing the Hurt Sound Effect");
            }

            if (_playerDeathSoundEffect == null) {
                Debug.Log("Warning: Player Health is missing the Death Sound Effect");
            }
        }

        public void Damage(int amount)
        {
            if (!_isAlive) return;

            _currentHealth -= amount;

            if (_healthSliderView != null) {
                _healthSliderView.value = _currentHealth;
            }

            if (_currentHealth <= 0) {
                StartCoroutine(PlaySound(_playerDeathSoundEffect));
                Kill();
            } else {
                StartCoroutine(PlaySound(_playerHurtSoundEffect));
            }
        }

        public void Kill()
        {
            _isAlive = false;

            GameObject art = transform.Find("Art").gameObject;
            if (art != null) {
                art.SetActive(false);
            }

            PlayerMovement playerMovement = GetComponent<PlayerMovement>();
            if (playerMovement != null) {
                playerMovement._isAlive = false;
            }

            PlayerFire playerFire = GetComponent<PlayerFire>();
            if (playerFire != null) {
                playerFire._isAlive = false;
            }

            Transform playerCameraParent = transform.Find("CameraParent");
            if (playerCameraParent != null) {
                PlayerCamera playerCamera = playerCameraParent.GetComponent<PlayerCamera>();
                if (playerCamera != null) {
                    playerCamera._isAlive = false;
                }
            }

            _deathParticles.Emit(25);
        }

        private IEnumerator PlaySound(AudioClip clip)
        {
            GameObject soundPlayer = new GameObject("PlayerSounds");
            soundPlayer.transform.parent = transform;
            AudioSource sound = soundPlayer.AddComponent<AudioSource>();
            sound.clip = clip;
            sound.Play();
            yield return new WaitForSeconds(1);
            Destroy(soundPlayer);
        }
    }
}
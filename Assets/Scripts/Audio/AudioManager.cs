using UnityEngine;

namespace Assets.Scripts.Audio {
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour {
        public static AudioManager Instance = null;

        private AudioSource _audioSource;

        private void Awake()
        {
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                _audioSource = GetComponent<AudioSource>();
            } else {
                Destroy(gameObject);
            }
        }

        public void PlaySong(AudioClip clip)
        {
            _audioSource.clip = clip;
            _audioSource.Play();
        }

        public bool IsPlaying()
        {
            return _audioSource.isPlaying;
        }
    }
}
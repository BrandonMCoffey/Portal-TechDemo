using UnityEngine;

namespace Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioClip _audioClip;

        public static AudioManager Instance;

        private AudioSource _audioSource;

        #region Singleton

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

        #endregion

        private void Update()
        {
            if (!IsPlaying()) PlaySong(_audioClip);
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
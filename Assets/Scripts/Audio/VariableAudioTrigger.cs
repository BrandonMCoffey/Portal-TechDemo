using Assets.Scripts.Utility;
using UnityEngine;

namespace Assets.Scripts.Audio {
    [RequireComponent(typeof(AudioSource))]
    public class VariableAudioTrigger : MonoBehaviour {
        [SerializeField] private FloatVariable _variable = null;
        [SerializeField] private FloatReference _lowThreshold = null;

        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            if (_variable.Value < _lowThreshold) {
                if (!_audioSource.isPlaying)
                    _audioSource.Play();
            } else {
                if (_audioSource.isPlaying)
                    _audioSource.Stop();
            }
        }
    }
}
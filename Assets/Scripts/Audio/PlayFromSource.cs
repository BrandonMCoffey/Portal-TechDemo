using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Audio {
    [RequireComponent(typeof(AudioSource))]
    public class PlayFromSource : MonoBehaviour {
        [SerializeField] private List<AudioClip> _audioClips = new List<AudioClip>();

        [SerializeField] private List<float> _clipVolume = new List<float>();

        private AudioSource _source;
        private float _sourceVolume;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            _sourceVolume = _source.volume;
        }

        public void PlayOneShot(int clipNumber)
        {
            if (clipNumber >= _audioClips.Count) return;
            _source.volume = clipNumber < _clipVolume.Count ? _clipVolume[clipNumber] : _sourceVolume;
            _source.PlayOneShot(_audioClips[clipNumber]);
        }
    }
}
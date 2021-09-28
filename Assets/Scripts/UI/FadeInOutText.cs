using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI
{
    public class FadeInOutText : MonoBehaviour
    {
        [SerializeField] private List<TextMeshProUGUI> _textToFade = new List<TextMeshProUGUI>();

        [SerializeField] private float _fadeInTime = 0.5f;
        [SerializeField] private float _sitTime = 1;
        [SerializeField] private float _fadeOutTime = 0.5f;

        private Coroutine _fadeRoutine;

        private float _fadeStatus;

        private void Awake()
        {
            foreach (var text in _textToFade) {
                text.gameObject.SetActive(false);
            }
        }

        public void FadeInStart()
        {
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(FadeRoutine());
        }

        public void ForceFadeOut()
        {
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(FadeOutRoutine());
        }

        private IEnumerator FadeRoutine()
        {
            foreach (var text in _textToFade) {
                text.gameObject.SetActive(true);
            }
            for (float t = _fadeStatus * _fadeInTime; t <= _fadeInTime; t += Time.deltaTime) {
                _fadeStatus = t / _fadeInTime;
                foreach (var text in _textToFade) {
                    Color color = text.color;
                    color.a = _fadeStatus;
                    text.color = color;
                }
                yield return null;
            }
            _fadeStatus = 1;

            for (float t = 0; t <= _sitTime; t += Time.deltaTime) {
                yield return null;
            }

            for (float t = 0; t <= _fadeOutTime; t += Time.deltaTime) {
                _fadeStatus = 1 - t / _fadeOutTime;
                foreach (var text in _textToFade) {
                    Color color = text.color;
                    color.a = _fadeStatus;
                    text.color = color;
                }
                yield return null;
            }
            _fadeStatus = 0;

            foreach (var text in _textToFade) {
                text.gameObject.SetActive(false);
            }
        }

        private IEnumerator FadeOutRoutine()
        {
            for (float t = _fadeOutTime - _fadeStatus * _fadeOutTime; t <= _fadeOutTime; t += Time.deltaTime) {
                _fadeStatus = 1 - t / _fadeOutTime;
                foreach (var text in _textToFade) {
                    Color color = text.color;
                    color.a = _fadeStatus;
                    text.color = color;
                }
                yield return null;
            }
            _fadeStatus = 0;
            foreach (var text in _textToFade) {
                text.gameObject.SetActive(false);
            }
        }
    }
}
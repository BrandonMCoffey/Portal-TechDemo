using System.Collections;
using Assets.Scripts.Utility;
using UnityEngine;

namespace Assets.Scripts.Portals {
    [RequireComponent(typeof(LineRenderer))]
    public class PortalLaser : MonoBehaviour {
        [SerializeField] private Material _portal0 = null;
        [SerializeField] private Material _portal1 = null;
        [SerializeField] private AnimationCurve _durationCurve = AnimationCurve.Constant(0, 1, 1);
        [SerializeField] private TransformReference _fromLocation = null;

        private Coroutine _coroutine;

        private LineRenderer _lineRenderer;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            if (_fromLocation == null) Debug.Log("[" + GetType().Name + "] From Location Variable missing on " + name);
        }

        private void OnEnable()
        {
            _lineRenderer.SetPosition(0, Vector3.zero);
            _lineRenderer.SetPosition(1, Vector3.zero);
        }

        public void DrawLine(PortalVariable portalLocation)
        {
            _lineRenderer.material = portalLocation.PortalID == 0 ? _portal0 : _portal1;
            _lineRenderer.SetPosition(0, _fromLocation.Position);
            _lineRenderer.SetPosition(1, portalLocation.Position);
            if (_coroutine != null) {
                StopCoroutine(_coroutine);
            }
            _coroutine = StartCoroutine(LineTimer());
        }

        public IEnumerator LineTimer()
        {
            _lineRenderer.enabled = true;
            float timeStamp = Time.time;
            float duration = _durationCurve[_durationCurve.length - 1].time;
            while (Time.time < timeStamp + duration) {
                float t = (Time.time - timeStamp) / duration;
                _lineRenderer.widthMultiplier = _durationCurve.Evaluate(t);
                yield return null;
            }
            _lineRenderer.enabled = false;
        }
    }
}
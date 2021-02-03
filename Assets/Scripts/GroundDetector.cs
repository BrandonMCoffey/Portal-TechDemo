using System;
using UnityEngine;

namespace Assets.Scripts {
    public class GroundDetector : MonoBehaviour {
        public event Action GroundDetected = delegate { };
        public event Action GroundVanished = delegate { };

        private void OnTriggerEnter(Collider other)
        {
            GroundDetected?.Invoke();
        }

        private void OnTriggerExit(Collider other)
        {
            GroundVanished?.Invoke();
        }
    }
}
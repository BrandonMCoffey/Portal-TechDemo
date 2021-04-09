using Assets.Scripts.GameEvents.Logic;
using Assets.Scripts.Player;
using UnityEngine;

namespace Assets.Scripts.Interactables {
    public class WinVolume : MonoBehaviour {
        [SerializeField] private GameEvent _onWin = null;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.GetComponent<PlayerHealth>()) return;
            if (_onWin != null) _onWin.Raise();
        }
    }
}
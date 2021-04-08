using Assets.Scripts.GameEvents.Logic;
using Assets.Scripts.Player;
using UnityEngine;

namespace Assets.Scripts.Interactables {
    [RequireComponent(typeof(Collider))]
    public class Collectable : MonoBehaviour {
        [SerializeField] private GameEvent _onCollected = null;

        private void OnTriggerEnter(Collider other)
        {
            PlayerHealth playerHealth = other.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null) {
                if (_onCollected != null) _onCollected.Raise();
                Destroy(gameObject);
            }
        }
    }
}
using System.Collections;
using UnityEngine;

namespace Assets.Scripts {
    public class DamageVolume : MonoBehaviour {
        [SerializeField] private int _damage = 1;
        [SerializeField] private int _delay = 1;

        private bool _canDamage = true;

        private void OnTriggerStay(Collider other)
        {
            if (!_canDamage) return;
            if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

            PlayerHealth playerHealth = other.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth == null) {
                Debug.Log("Warning: Invalid Object on Player layer detected in Damage Volume: " + other.gameObject);
                return;
            }

            playerHealth.Damage(_damage);
            StartCoroutine(DelayNextDamage());
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

            PlayerHealth playerHealth = other.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth == null) return;

            StopCoroutine(DelayNextDamage());
            _canDamage = true;
        }

        private IEnumerator DelayNextDamage()
        {
            _canDamage = false;
            yield return new WaitForSeconds(_delay);
            _canDamage = true;
        }
    }
}
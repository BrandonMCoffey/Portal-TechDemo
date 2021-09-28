using UnityEngine;
using Utility;
using Utility.GameEvents.Logic;
using Utility.References;

namespace Player
{
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health Values")]
        [SerializeField] private FloatVariable _currentHealth = null;
        [SerializeField] private FloatReference _startingHealth = new FloatReference(4);
        [SerializeField] private FloatReference _maxHealth = new FloatReference(4);

        [Header("Triggered Events")]
        [SerializeField] private GameEvent _damageEvent = null;
        [SerializeField] private GameEvent _deathEvent = null;

        [Header("References")]
        [SerializeField] private BoolVariable _playerHasControl = null;

        private void Awake()
        {
            if (_currentHealth == null) Debug.Log("[" + GetType().Name + "] Current Health Float Variable missing on " + name);
            if (_damageEvent == null) Debug.Log("[" + GetType().Name + "] Damage Event missing on " + name);
            if (_deathEvent == null) Debug.Log("[" + GetType().Name + "] Death Event missing on " + name);
            if (_playerHasControl == null) Debug.Log("[" + GetType().Name + "] Player Has Control Bool Variable missing on " + name);
        }

        private void Start()
        {
            _currentHealth.SetValue(_startingHealth);
        }

        public void Damage(float damage)
        {
            if (!_playerHasControl.Value) return;
            _currentHealth.ApplyChange(-damage);
            if (_currentHealth.Value > 0) {
                _damageEvent.Raise();
            } else {
                Kill();
            }
        }

        public void Kill()
        {
            _deathEvent.Raise();
            _playerHasControl.SetValue(false);

            GameObject art = transform.Find("Art").gameObject;
            if (art != null) {
                art.SetActive(false);
            }
        }
    }
}
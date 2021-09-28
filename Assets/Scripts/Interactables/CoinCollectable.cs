using Player;
using UnityEngine;
using Utility.GameEvents.Logic;
using Utility.References;

namespace Interactables
{
    public class CoinCollectable : MonoBehaviour
    {
        [SerializeField] private FloatVariable _collection = null;
        [SerializeField] private GameEvent _onCollect = null;
        [SerializeField] private float _bobAmplitude = 1;
        [SerializeField] private float _bobTime = 1;
        [SerializeField] private float _rotateSpeed = 1;

        private Vector3 _initialPos;

        private void Awake()
        {
            _initialPos = transform.position;
            Vector3 rot = transform.eulerAngles;
            rot.y = Random.Range(0, 360);
            transform.eulerAngles = rot;
        }

        private void FixedUpdate()
        {
            Vector3 rot = transform.eulerAngles;
            rot.y += _rotateSpeed;
            transform.eulerAngles = rot;

            transform.position = _initialPos + transform.up * Mathf.Sin(Time.time * _bobTime) * _bobAmplitude;
        }

        private void OnTriggerEnter(Collider other)
        {
            PlayerHealth playerHealth = other.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth == null) return;
            if (_collection != null) _collection.ApplyChange(1);
            if (_onCollect != null) _onCollect.Raise();
            Destroy(gameObject);
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    public class WallTurretHealth : MonoBehaviour
    {
        [SerializeField] private int _health = 3;
        [SerializeField] private List<MeshRenderer> _healthVisuals = new List<MeshRenderer>();
        [SerializeField] private Material _fullHealthMaterial = null;
        [SerializeField] private Material _noHealthMaterial = null;

        private void Awake()
        {
            if (_health != _healthVisuals.Count) Debug.Log("Invalid amount of health visuals on " + gameObject.name);
            foreach (MeshRenderer visual in _healthVisuals) {
                visual.material = _fullHealthMaterial;
            }
        }

        public bool Damage()
        {
            _health--;
            int num = 0;
            foreach (MeshRenderer visual in _healthVisuals) {
                num++;
                visual.material = num <= _health ? _fullHealthMaterial : _noHealthMaterial;
            }
            return _health <= 0;
        }
    }
}
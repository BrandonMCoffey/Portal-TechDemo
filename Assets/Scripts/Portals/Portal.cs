using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Portals {
    [RequireComponent(typeof(BoxCollider))]
    public class Portal : MonoBehaviour {
        [HideInInspector] public Portal OtherPortal = null;
        [HideInInspector] public Collider WallCollider = null;
        [HideInInspector] public Collider Collider;

        private List<PortalTraveler> _portalTravelers = new List<PortalTraveler>();

        private void Awake()
        {
            Collider = GetComponent<Collider>();
        }

        private void Update()
        {
            foreach (var traveler in _portalTravelers) {
                Vector3 pos = transform.InverseTransformPoint(traveler.transform.position);
                if (pos.z > 0) traveler.Warp();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var traveler = other.GetComponent<PortalTraveler>();
            if (traveler != null) {
                _portalTravelers.Add(traveler);
                traveler.SetInPortal(this, OtherPortal);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var traveler = other.GetComponent<PortalTraveler>();
            if (_portalTravelers.Contains(traveler)) {
                _portalTravelers.Remove(traveler);
                traveler.ExitPortal();
            }
        }
    }
}
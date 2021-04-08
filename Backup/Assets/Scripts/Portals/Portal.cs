using System.Collections.Generic;
using Assets.Scripts.Utility;
using UnityEngine;

namespace Assets.Scripts.Portals {
    [RequireComponent(typeof(BoxCollider), typeof(PortalRenderer))]
    public class Portal : MonoBehaviour {
        public Portal OtherPortal = null;

        [HideInInspector] public PortalRenderer PortalRenderer = null;
        [HideInInspector] public Collider WallCollider = null;
        [HideInInspector] public Collider Collider;

        private List<PortalTraveler> _portalTravelers = new List<PortalTraveler>();

        private void Awake()
        {
            PortalRenderer = GetComponent<PortalRenderer>();
            Collider = GetComponent<Collider>();
        }

        private void OnEnable()
        {
            if (OtherPortal != null && OtherPortal.isActiveAndEnabled) {
                PortalRenderer.OtherRenderer = OtherPortal.PortalRenderer;
                OtherPortal.PortalRenderer.OtherRenderer = PortalRenderer;
            }
        }

        private void OnDisable()
        {
            foreach (var traveler in _portalTravelers) {
                traveler.ExitPortal();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var traveler = other.GetComponent<PortalTraveler>();
            if (traveler != null) {
                Debug.Log("Enter");
                _portalTravelers.Add(traveler);
                traveler.SetInPortal(this, OtherPortal);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            Debug.Log("Exit");
            var traveler = other.GetComponent<PortalTraveler>();
            if (_portalTravelers.Contains(traveler)) {
                _portalTravelers.Remove(traveler);
                traveler.ExitPortal();
            }
        }

        private void Update()
        {
            foreach (var traveler in _portalTravelers) {
                Debug.Log("Travel");
                Vector3 pos = transform.InverseTransformPoint(traveler.transform.position);
                if (pos.z > 0) traveler.Warp();
            }
        }

        // Called before any portal cameras are rendered for the current frame
        public void PreRenderPortal()
        {
            foreach (var traveler in _portalTravelers) {
                // Update Slice Parameters
            }
        }

        // Manually render the camera attached to this portal
        // Called after PrePortalRender, and before PostPortalRender
        public void RenderPortal(Camera playerCamera)
        {
            if (!isActiveAndEnabled || !OtherPortal.isActiveAndEnabled) return;

            // Skip rendering the view from this portal if player is not looking at the linked portal
            if (CameraUtility.VisibleFromCamera(OtherPortal.PortalRenderer.MeshRenderer, playerCamera)) {
                PortalRenderer.Render(playerCamera);
            }
        }

        // Called once all portals have been rendered, but before the player camera renders
        public void PostRenderPortal()
        {
            foreach (var traveler in _portalTravelers) {
                // Update Slice Parameters
            }
        }
    }
}
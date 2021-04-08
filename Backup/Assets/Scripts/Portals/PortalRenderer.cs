using System.Collections.Generic;
using Assets.Scripts.Utility;
using UnityEngine;

namespace Assets.Scripts.Portals {
    public class PortalRenderer : MonoBehaviour {
        public MeshRenderer MeshRenderer;

        [SerializeField] private float _nearClipOffset = 0.05f;
        [SerializeField] private float _nearClipLimit = 0.2f;

        [HideInInspector] public PortalRenderer OtherRenderer = null;
        [HideInInspector] public MeshFilter MeshFilter;

        private Camera _portalCamera;
        private RenderTexture _texture;

        private Vector3 PortalCamPos => _portalCamera.transform.position;

        private Quaternion _flipRotation = Quaternion.Euler(0f, 180f, 0f);

        private const int RecursionLimit = 7;

        private void Awake()
        {
            if (MeshRenderer != null) MeshFilter = MeshRenderer.GetComponent<MeshFilter>();
            _portalCamera = new GameObject(gameObject.name + " Camera").AddComponent<Camera>();
            _portalCamera.transform.SetParent(transform.parent);

            _portalCamera.targetDisplay = 1; // FIXME Temp fix for display issue
        }

        private void OnEnable()
        {
            bool active = OtherRenderer != null && OtherRenderer.isActiveAndEnabled;
            _portalCamera.gameObject.SetActive(active);
            if (OtherRenderer != null) OtherRenderer._portalCamera.gameObject.SetActive(active);
        }

        public void Render(Camera playerCamera)
        {
            CreateViewTexture();

            var localToWorldMatrix = playerCamera.transform.localToWorldMatrix;
            var renderPositions = new Vector3[RecursionLimit];
            var renderRotations = new Quaternion[RecursionLimit];

            int startIndex = 0;
            _portalCamera.projectionMatrix = playerCamera.projectionMatrix;
            for (int i = 0; i < RecursionLimit; i++) {
                // No need for recursive rendering if linked portal is not visible through this portal
                if (i > 0 && !CameraUtility.BoundsOverlap(MeshFilter, OtherRenderer.MeshFilter, _portalCamera)) break;

                localToWorldMatrix = transform.localToWorldMatrix * OtherRenderer.transform.worldToLocalMatrix * localToWorldMatrix;
                int renderOrderIndex = RecursionLimit - i - 1;
                renderPositions[renderOrderIndex] = localToWorldMatrix.GetColumn(3);
                renderRotations[renderOrderIndex] = localToWorldMatrix.rotation;

                _portalCamera.transform.SetPositionAndRotation(renderPositions[renderOrderIndex], renderRotations[renderOrderIndex]);
                startIndex = renderOrderIndex;
            }

            // Hide MeshRenderer so that camera can see through portal
            MeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            OtherRenderer.MeshRenderer.material.SetInt("displayMask", 0);

            for (int i = startIndex; i < RecursionLimit; i++) {
                _portalCamera.transform.SetPositionAndRotation(renderPositions[i], renderRotations[i]);
                SetNearClipPlane();
                HandleClipping();
                _portalCamera.Render();

                if (i == startIndex) {
                    OtherRenderer.MeshRenderer.material.SetInt("displayMask", 1);
                }
            }

            // Unhide objects hidden at start of render
            MeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }

        private void CreateViewTexture()
        {
            if (_texture != null && _texture.width == Screen.width && _texture.height == Screen.height) return;

            if (_texture != null) {
                _texture.Release();
            }
            _texture = new RenderTexture(Screen.width, Screen.height, 0);
            // Render the view from the portal camera to the view texture
            _portalCamera.targetTexture = _texture;
            // Display the view texture on the MeshRenderer of the linked portal
            OtherRenderer.MeshRenderer.material.SetTexture("_MainTex", _texture);
        }


        // Use custom projection matrix to align portal camera's near clip plane with the surface of the portal
        // Note that this affects precision of the depth buffer, which can cause issues with effects like screen-space AO
        private void SetNearClipPlane()
        {
            // Learning resource:
            // http://www.terathon.com/lengyel/Lengyel-Oblique.pdf
            Transform clipPlane = transform;
            int dot = System.Math.Sign(Vector3.Dot(clipPlane.forward, transform.position - _portalCamera.transform.position));

            Vector3 camSpacePos = _portalCamera.worldToCameraMatrix.MultiplyPoint(clipPlane.position);
            Vector3 camSpaceNormal = _portalCamera.worldToCameraMatrix.MultiplyVector(clipPlane.forward) * dot;
            float camSpaceDst = -Vector3.Dot(camSpacePos, camSpaceNormal) + _nearClipOffset;

            // Don't use oblique clip plane if very close to portal as it seems this can cause some visual artifacts
            if (Mathf.Abs(camSpaceDst) > _nearClipLimit) {
                Vector4 clipPlaneCameraSpace = new Vector4(camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDst);

                // Update projection based on new clip plane
                // Calculate matrix with player cam so that player camera settings (fov, etc) are used
                _portalCamera.projectionMatrix = _portalCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
            } else {
                _portalCamera.projectionMatrix = _portalCamera.projectionMatrix;
            }
        }

        private void HandleClipping()
        {
            /*
            // There are two main graphical issues when slicing travelers
            // 1. Tiny sliver of mesh drawn on backside of portal
            //    Ideally the oblique clip plane would sort this out, but even with 0 offset, tiny sliver still visible
            // 2. Tiny seam between the sliced mesh, and the rest of the model drawn onto the portal MeshRenderer
            // This function tries to address these issues by modifying the slice parameters when rendering the view from the portal
            // Would be great if this could be fixed more elegantly, but this is the best I can figure out for now
            const float hideDst = -1000;
            const float showDst = 1000;
            float screenThickness = OtherRenderer.ProtectScreenFromClipping(_portalCamera.transform.position);

            foreach (var traveler in _trackedTravelers) {
                traveler.SetSliceOffsetDst(SameSideOfPortal(traveler.transform.position, PortalCamPos) ? hideDst : showDst, false);

                // Ensure clone is properly sliced, in case it's visible through this portal:
                int cloneSideOfLinkedPortal = -SideOfPortal(traveler.transform.position);
                bool camSameSideAsClone = linkedPortal.SideOfPortal(PortalCamPos) == cloneSideOfLinkedPortal;
                if (camSameSideAsClone) {
                    traveler.SetSliceOffsetDst(screenThickness, true);
                } else {
                    traveler.SetSliceOffsetDst(-screenThickness, true);
                }
            }

            var offsetFromPortalToCam = PortalCamPos - transform.position;
            foreach (var linkedTraveler in linkedPortal._trackedTravelers) {
                var travelerPos = linkedTraveler.graphicsObject.transform.position;
                var clonePos = linkedTraveler.GraphicsClone.transform.position;
                // Handle clone of linked portal coming through this portal:
                bool cloneOnSameSideAsCam = linkedPortal.SideOfPortal(travelerPos) != SideOfPortal(PortalCamPos);
                linkedTraveler.SetSliceOffsetDst(cloneOnSameSideAsCam ? hideDst : showDst, true);

                // Ensure traveler of linked portal is properly sliced, in case it's visible through this portal:
                bool camSameSideAsTraveler = linkedPortal.SameSideOfPortal(linkedTraveler.transform.position, PortalCamPos);
                if (camSameSideAsTraveler) {
                    linkedTraveler.SetSliceOffsetDst(screenThickness, false);
                } else {
                    linkedTraveler.SetSliceOffsetDst(-screenThickness, false);
                }
            }
            */
        }
    }
}
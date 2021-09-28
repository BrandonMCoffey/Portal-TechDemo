using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace Portals
{
    [RequireComponent(typeof(Collider))]
    public class Portal : MonoBehaviour
    {
        [Header("Main Settings")]
        public Portal LinkedPortal;
        public MeshRenderer Screen;
        public int RecursionLimit = 10;
        public Collider WallCollider = null;

        [Header("Advanced Settings")]
        public float NearClipOffset = 0.05f;
        public float NearClipLimit = 0.2f;

        [HideInInspector] public Collider Collider;
        [HideInInspector] public Camera PlayerCamera;

        private RenderTexture _viewTexture;
        private Camera _portalCam;
        private List<PortalTraveler> _trackedTravelers;
        private MeshFilter _screenMeshFilter;

        private bool _bufferTravelers;

        private Vector3 PortalCamPos => _portalCam.transform.position;

        private void Awake()
        {
            Collider = GetComponent<Collider>();
            PlayerCamera = Camera.main;
            _portalCam = GetComponentInChildren<Camera>();
            _portalCam.enabled = false;
            _trackedTravelers = new List<PortalTraveler>();
            _screenMeshFilter = Screen.GetComponent<MeshFilter>();
            Screen.material.SetInt("displayMask", 1);
        }

        private void OnValidate()
        {
            if (LinkedPortal != null) {
                LinkedPortal.LinkedPortal = this;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var traveler = other.GetComponent<PortalTraveler>();
            if (traveler) {
                OnTravelerEnterPortal(traveler);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var traveler = other.GetComponent<PortalTraveler>();
            if (traveler && _trackedTravelers.Contains(traveler)) {
                traveler.ExitPortalThreshold(this);
                _trackedTravelers.Remove(traveler);
            }
        }

        private void LateUpdate()
        {
            HandleTravelers(_bufferTravelers);
            _bufferTravelers = false;
        }

        private void HandleTravelers(bool reset = false)
        {
            for (int i = 0; i < _trackedTravelers.Count; i++) {
                PortalTraveler traveler = _trackedTravelers[i];
                Transform travelerT = traveler.transform;
                var m = LinkedPortal.transform.localToWorldMatrix * transform.worldToLocalMatrix * travelerT.localToWorldMatrix;

                Vector3 offsetFromPortal = travelerT.position - transform.position;
                int portalSide = System.Math.Sign(Vector3.Dot(offsetFromPortal, transform.forward));
                int portalSideOld = System.Math.Sign(Vector3.Dot(traveler.PreviousOffsetFromPortal, transform.forward));
                // Teleport the traveler if it has crossed from one side of the portal to the other
                if (reset) break;
                if (portalSide != portalSideOld) {
                    var positionOld = travelerT.position;
                    var rotOld = travelerT.rotation;
                    traveler.Teleport(transform, LinkedPortal.transform, m.GetColumn(3), m.rotation);
                    traveler.GraphicsClone.transform.SetPositionAndRotation(positionOld, rotOld);
                    // Can't rely on OnTriggerEnter/Exit to be called next frame since it depends on when FixedUpdate runs
                    traveler.ExitPortalThreshold(this);
                    LinkedPortal.OnTravelerEnterPortal(traveler);
                    _trackedTravelers.RemoveAt(i);
                    i--;
                } else {
                    traveler.GraphicsClone.transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);
                    //UpdateSliceParams (traveler);
                    traveler.PreviousOffsetFromPortal = offsetFromPortal;
                }
            }
        }

        public void SetInactiveColor()
        {
            Screen.material.SetInt("displayMask", 0);
        }

        // Called before any portal cameras are rendered for the current frame
        public void PreRenderPortal()
        {
            foreach (var traveler in _trackedTravelers) {
                UpdateSliceParams(traveler);
            }
        }

        // Manually render the camera attached to this portal
        // Called after PrePortalRender, and before PostPortalRender
        public void RenderPortal()
        {
            // Skip rendering the view from this portal if player is not looking at the linked portal
            if (!CameraUtility.VisibleFromCamera(LinkedPortal.Screen, PlayerCamera)) {
                LinkedPortal.Screen.material.SetInt("displayMask", 0);
                return;
            }

            CreateViewTexture();

            var localToWorldMatrix = PlayerCamera.transform.localToWorldMatrix;
            var renderPositions = new Vector3[RecursionLimit];
            var renderRotations = new Quaternion[RecursionLimit];

            int startIndex = 0;
            _portalCam.projectionMatrix = PlayerCamera.projectionMatrix;
            for (int i = 0; i < RecursionLimit; i++) {
                if (i > 1) {
                    // No need for recursive rendering if linked portal is not visible through this portal
                    if (!CameraUtility.BoundsOverlap(_screenMeshFilter, LinkedPortal._screenMeshFilter, _portalCam)) {
                        LinkedPortal.Screen.material.SetInt("displayMask", 0);
                        break;
                    }
                }
                localToWorldMatrix = transform.localToWorldMatrix * LinkedPortal.transform.worldToLocalMatrix * localToWorldMatrix;
                int renderOrderIndex = RecursionLimit - i - 1;
                renderPositions[renderOrderIndex] = localToWorldMatrix.GetColumn(3);
                renderRotations[renderOrderIndex] = localToWorldMatrix.rotation;

                _portalCam.transform.SetPositionAndRotation(renderPositions[renderOrderIndex], renderRotations[renderOrderIndex]);
                startIndex = renderOrderIndex;
            }

            // Hide screen so that camera can see through portal
            Screen.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            LinkedPortal.Screen.material.SetInt("displayMask", 0);

            for (int i = startIndex; i < RecursionLimit; i++) {
                _portalCam.transform.SetPositionAndRotation(renderPositions[i], renderRotations[i]);
                SetNearClipPlane();
                HandleClipping();
                _portalCam.Render();

                if (i == startIndex) {
                    LinkedPortal.Screen.material.SetInt("displayMask", 1);
                }
            }

            // Unhide objects hidden at start of render
            Screen.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }

        private void HandleClipping()
        {
            // There are two main graphical issues when slicing travellers
            // 1. Tiny sliver of mesh drawn on backside of portal
            //    Ideally the oblique clip plane would sort this out, but even with 0 offset, tiny sliver still visible
            // 2. Tiny seam between the sliced mesh, and the rest of the model drawn onto the portal screen
            // This function tries to address these issues by modifying the slice parameters when rendering the view from the portal
            // Would be great if this could be fixed more elegantly, but this is the best I can figure out for now
            const float hideDst = -1000;
            const float showDst = 1000;
            float screenThickness = LinkedPortal.ProtectScreenFromClipping(_portalCam.transform.position);

            foreach (var traveler in _trackedTravelers) {
                traveler.SetSliceOffsetDst(SameSideOfPortal(traveler.transform.position, PortalCamPos) ? hideDst : showDst, false);

                // Ensure clone is properly sliced, in case it's visible through this portal:
                int cloneSideOfLinkedPortal = -SideOfPortal(traveler.transform.position);
                bool camSameSideAsClone = LinkedPortal.SideOfPortal(PortalCamPos) == cloneSideOfLinkedPortal;
                if (camSameSideAsClone) {
                    traveler.SetSliceOffsetDst(screenThickness, true);
                } else {
                    traveler.SetSliceOffsetDst(-screenThickness, true);
                }
            }

            foreach (var linkedTraveler in LinkedPortal._trackedTravelers) {
                var travelerPos = linkedTraveler.GraphicsObject.transform.position;
                var clonePos = linkedTraveler.GraphicsClone.transform.position;
                // Handle clone of linked portal coming through this portal:
                bool cloneOnSameSideAsCam = LinkedPortal.SideOfPortal(travelerPos) != SideOfPortal(PortalCamPos);
                linkedTraveler.SetSliceOffsetDst(cloneOnSameSideAsCam ? hideDst : showDst, true);

                // Ensure traveler of linked portal is properly sliced, in case it's visible through this portal:
                bool camSameSideAsTraveler = LinkedPortal.SameSideOfPortal(linkedTraveler.transform.position, PortalCamPos);
                if (camSameSideAsTraveler) {
                    linkedTraveler.SetSliceOffsetDst(screenThickness, false);
                } else {
                    linkedTraveler.SetSliceOffsetDst(-screenThickness, false);
                }
            }
        }

        // Called once all portals have been rendered, but before the player camera renders
        public void PostRenderPortal()
        {
            foreach (var traveler in _trackedTravelers) {
                UpdateSliceParams(traveler);
            }
            ProtectScreenFromClipping(PlayerCamera.transform.position);
        }

        private void CreateViewTexture()
        {
            if (_viewTexture != null && _viewTexture.width == UnityEngine.Screen.width && _viewTexture.height == UnityEngine.Screen.height) return;
            if (_viewTexture != null) {
                _viewTexture.Release();
            }
            _viewTexture = new RenderTexture(UnityEngine.Screen.width, UnityEngine.Screen.height, 0);
            // Render the view from the portal camera to the view texture
            _portalCam.targetTexture = _viewTexture;
            // Display the view texture on the screen of the linked portal
            LinkedPortal.Screen.material.SetTexture("_MainTex", _viewTexture);
        }

        // Sets the thickness of the portal screen so as not to clip with camera near plane when player goes through
        private float ProtectScreenFromClipping(Vector3 viewPoint)
        {
            float halfHeight = PlayerCamera.nearClipPlane * Mathf.Tan(PlayerCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float halfWidth = halfHeight * PlayerCamera.aspect;
            float dstToNearClipPlaneCorner = new Vector3(halfWidth, halfHeight, PlayerCamera.nearClipPlane).magnitude;
            float screenThickness = dstToNearClipPlaneCorner;

            Transform screenT = Screen.transform;
            bool camFacingSameDirAsPortal = Vector3.Dot(transform.forward, transform.position - viewPoint) > 0;
            screenT.localScale = new Vector3(screenT.localScale.x, screenT.localScale.y, screenThickness);
            screenT.localPosition = Vector3.forward * screenThickness * ((camFacingSameDirAsPortal) ? 0.5f : -0.5f);
            return screenThickness;
        }

        private void UpdateSliceParams(PortalTraveler traveler)
        {
            // Calculate slice normal
            int side = SideOfPortal(traveler.transform.position);
            Vector3 sliceNormal = transform.forward * -side;
            Vector3 cloneSliceNormal = LinkedPortal.transform.forward * side;

            // Calculate slice centre
            Vector3 slicePos = transform.position;
            Vector3 cloneSlicePos = LinkedPortal.transform.position;

            // Adjust slice offset so that when player standing on other side of portal to the object, the slice doesn't clip through
            float sliceOffsetDst = 0;
            float cloneSliceOffsetDst = 0;
            float screenThickness = Screen.transform.localScale.z;

            bool playerSameSideAsTraveler = SameSideOfPortal(PlayerCamera.transform.position, traveler.transform.position);
            if (!playerSameSideAsTraveler) {
                sliceOffsetDst = -screenThickness;
            }
            bool playerSameSideAsCloneAppearing = side != LinkedPortal.SideOfPortal(PlayerCamera.transform.position);
            if (!playerSameSideAsCloneAppearing) {
                cloneSliceOffsetDst = -screenThickness;
            }

            // Apply parameters
            for (int i = 0; i < traveler.OriginalMaterials.Length; i++) {
                traveler.OriginalMaterials[i].SetVector("sliceCentre", slicePos);
                traveler.OriginalMaterials[i].SetVector("sliceNormal", sliceNormal);
                traveler.OriginalMaterials[i].SetFloat("sliceOffsetDst", sliceOffsetDst);

                traveler.CloneMaterials[i].SetVector("sliceCentre", cloneSlicePos);
                traveler.CloneMaterials[i].SetVector("sliceNormal", cloneSliceNormal);
                traveler.CloneMaterials[i].SetFloat("sliceOffsetDst", cloneSliceOffsetDst);
            }
        }

        // Use custom projection matrix to align portal camera's near clip plane with the surface of the portal
        // Note that this affects precision of the depth buffer, which can cause issues with effects like screenspace AO
        private void SetNearClipPlane()
        {
            // Learning resource:
            // http://www.terathon.com/lengyel/Lengyel-Oblique.pdf
            Transform clipPlane = transform;
            int dot = System.Math.Sign(Vector3.Dot(clipPlane.forward, transform.position - _portalCam.transform.position));

            Vector3 camSpacePos = _portalCam.worldToCameraMatrix.MultiplyPoint(clipPlane.position);
            Vector3 camSpaceNormal = _portalCam.worldToCameraMatrix.MultiplyVector(clipPlane.forward) * dot;
            float camSpaceDst = -Vector3.Dot(camSpacePos, camSpaceNormal) + NearClipOffset;

            // Don't use oblique clip plane if very close to portal as it seems this can cause some visual artifacts
            if (Mathf.Abs(camSpaceDst) > NearClipLimit) {
                Vector4 clipPlaneCameraSpace = new Vector4(camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDst + 0.2f);

                // Update projection based on new clip plane
                // Calculate matrix with player cam so that player camera settings (fov, etc) are used
                _portalCam.projectionMatrix = PlayerCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
            } else {
                _portalCam.projectionMatrix = PlayerCamera.projectionMatrix;
            }
        }

        private void OnTravelerEnterPortal(PortalTraveler traveler)
        {
            if (_trackedTravelers.Contains(traveler)) return;
            traveler.EnterPortalThreshold(this);
            traveler.PreviousOffsetFromPortal = traveler.transform.position - transform.position;
            _trackedTravelers.Add(traveler);
        }

        public void RemoveTravelers()
        {
            for (int i = _trackedTravelers.Count - 1; i > 0; i++) {
                _trackedTravelers[i].ExitPortalThreshold(this);
                _trackedTravelers.Remove(_trackedTravelers[i]);
            }
        }

        public void BufferTravelers()
        {
            _bufferTravelers = true;
        }

        private int SideOfPortal(Vector3 pos)
        {
            return System.Math.Sign(Vector3.Dot(pos - transform.position, transform.forward));
        }

        private bool SameSideOfPortal(Vector3 posA, Vector3 posB)
        {
            return SideOfPortal(posA) == SideOfPortal(posB);
        }
    }
}
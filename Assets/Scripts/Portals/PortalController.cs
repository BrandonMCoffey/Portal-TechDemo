using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Portals {
    public class PortalController : MonoBehaviour {
        [SerializeField] private LayerMask _wallMask = 0;
        [SerializeField] private List<Portal> _portals = new List<Portal>(2);

        private int _portalToPlace;

        private const float PortalHeight = 1f;
        private const float PortalWidth = 0.5f;
        private const string PortalMask = "Portal";

        private Transform _portalPlacementTest;

        private void Awake()
        {
            _portalPlacementTest = new GameObject("Portal Placement Tester").transform;
            _portalPlacementTest.SetParent(transform);
        }

        private void OnEnable()
        {
            if (_portals.Count == 0) {
                foreach (Transform obj in transform) {
                    Portal portal = obj.GetComponent<Portal>();
                    if (portal != null) {
                        _portals.Add(portal);
                    }
                }
            }

            foreach (var portal in _portals) {
                portal.gameObject.SetActive(false);
            }
        }

        public void RenderPortals(Camera playerCamera)
        {
            foreach (var portal in _portals) {
                portal.PreRenderPortal();
            }
            foreach (var portal in _portals) {
                portal.RenderPortal(playerCamera);
            }
            foreach (var portal in _portals) {
                portal.PostRenderPortal();
                // if (playerCamera != null) ProtectScreenFromClipping(playerCamera.transform.position, portal.PortalRenderer.MeshRenderer.transform);
            }
        }

        /*private void ProtectScreenFromClipping(Vector3 viewPoint, Transform screen)
        {
            float halfHeight = _playerCamera.nearClipPlane * Mathf.Tan(_playerCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float halfWidth = halfHeight * _playerCamera.aspect;
            float dstToNearClipPlaneCorner = new Vector3(halfWidth, halfHeight, _playerCamera.nearClipPlane).magnitude;
            float screenThickness = dstToNearClipPlaneCorner;

            bool camFacingSameDirAsPortal = Vector3.Dot(transform.forward, transform.position - viewPoint) > 0;
            screen.localScale = new Vector3(screen.localScale.x, screen.localScale.y, screenThickness);
            screen.localPosition = Vector3.forward * screenThickness * ((camFacingSameDirAsPortal) ? 0.5f : -0.5f);
            // return screenThickness;
        }*/

        public void PlacePortal(PortalVariable portal)
        {
            if (!portal.ValidLocation) return;
            _portalToPlace = portal.PortalID;

            _portalPlacementTest.position = portal.Position;
            _portalPlacementTest.rotation = portal.Rotation;
            _portalPlacementTest.position -= portal.Forward * 0.001f;

            // Debug.Log("Start:            " + _portalPlacementTest.position);
            FixOverhangs();
            FixIntersects();

            if (!CheckOverlap()) return;

            _portals[_portalToPlace].gameObject.SetActive(true);
            _portals[_portalToPlace].WallCollider = portal.Collider;
            _portals[_portalToPlace].transform.position = _portalPlacementTest.position;
            _portals[_portalToPlace].transform.rotation = _portalPlacementTest.rotation;
        }

        // Ensure the portal cannot extend past the edge of a surface.
        private void FixOverhangs()
        {
            var testPoints = new List<Vector3>
            {
                new Vector3(-PortalWidth - 0.1f, 0, 0.1f),
                new Vector3(PortalWidth + 0.1f, 0, 0.1f),
                new Vector3(0, -PortalHeight - 0.1f, 0.1f),
                new Vector3(0, PortalHeight + 0.1f, 0.1f)
            };

            var testDirs = new List<Vector3>
            {
                Vector3.right,
                -Vector3.right,
                Vector3.up,
                -Vector3.up
            };

            var testDist = new List<float>
            {
                PortalWidth,
                PortalWidth,
                PortalHeight,
                PortalHeight
            };

            for (int i = 0; i < 4; ++i) {
                Vector3 raycastPos = _portalPlacementTest.TransformPoint(testPoints[i]);
                Vector3 raycastDir = _portalPlacementTest.TransformDirection(testDirs[i]);

                if (Physics.CheckSphere(raycastPos, 0.05f, _wallMask)) break;

                if (Physics.Raycast(raycastPos, raycastDir, out var hit, testDist[i], _wallMask)) {
                    var offset = hit.point - raycastPos;
                    _portalPlacementTest.Translate(offset, Space.World);
                }
            }
            // Debug.Log("Overhang:   " + _portalPlacementTest.position);
        }

        // Ensure the portal cannot intersect a section of wall.
        private void FixIntersects()
        {
            var testDirs = new List<Vector3>
            {
                Vector3.right,
                -Vector3.right,
                Vector3.up,
                -Vector3.up
            };

            var testDistances = new List<float> {PortalWidth + 0.1f, PortalWidth + 0.1f, PortalHeight + 0.1f, PortalHeight + 0.1f};

            for (int i = 0; i < 4; ++i) {
                Vector3 raycastPos = _portalPlacementTest.TransformPoint(0.0f, 0.0f, -0.1f);
                Vector3 raycastDir = _portalPlacementTest.TransformDirection(testDirs[i]);

                if (Physics.Raycast(raycastPos, raycastDir, out var hit, testDistances[i], _wallMask)) {
                    var offset = (hit.point - raycastPos);
                    var newOffset = -raycastDir * (testDistances[i] - offset.magnitude);
                    _portalPlacementTest.Translate(newOffset, Space.World);
                }
            }
            // Debug.Log("Intersect:     " + _portalPlacementTest.position);
        }

        // Once positioning has taken place, ensure the portal isn't intersecting anything.
        private bool CheckOverlap(bool second = false)
        {
            var checkExtents = new Vector3(PortalWidth - 0.1f, PortalHeight - 0.1f, 0.05f);

            var checkPositions = new Vector3[]
            {
                _portalPlacementTest.position + _portalPlacementTest.TransformVector(new Vector3(0.0f, 0.0f, -0.1f)),

                _portalPlacementTest.position + _portalPlacementTest.TransformVector(new Vector3(-PortalWidth, -PortalHeight, -0.1f)),
                _portalPlacementTest.position + _portalPlacementTest.TransformVector(new Vector3(-PortalWidth, PortalHeight, -0.1f)),
                _portalPlacementTest.position + _portalPlacementTest.TransformVector(new Vector3(PortalWidth, -PortalHeight, -0.1f)),
                _portalPlacementTest.position + _portalPlacementTest.TransformVector(new Vector3(PortalWidth, PortalHeight, -0.1f)),

                _portalPlacementTest.TransformVector(new Vector3(0.0f, 0.0f, 0.2f))
            };

            // Ensure the portal does not intersect walls.
            var intersections = Physics.OverlapBox(checkPositions[0], checkExtents, _portalPlacementTest.rotation, _wallMask);

            if (intersections.Length > 0) return false;

            // Ensure the portal corners overlap a surface.
            bool isOverlapping = true;

            for (int i = 1; i < checkPositions.Length - 1; ++i) {
                isOverlapping &= Physics.Linecast(checkPositions[i], checkPositions[i] + checkPositions[checkPositions.Length - 1], _wallMask);
            }

            if (!isOverlapping) return false;

            checkExtents = new Vector3(PortalWidth + 0.05f, PortalHeight + 0.05f, 0.05f);

            var portalIntersections = Physics.OverlapBox(checkPositions[0], checkExtents, _portalPlacementTest.rotation, LayerMask.GetMask(PortalMask));
            // We are allowed to intersect the old portal position.
            if (portalIntersections.Length > 0) {
                foreach (var portalCollider in portalIntersections) {
                    if (_portals.Count > _portalToPlace && portalCollider == _portals[_portalToPlace].Collider) continue;

                    if (second) return false;
                    portalCollider.gameObject.layer = 10; // FIXME invalid solution
                    FixOverhangs();
                    FixIntersects();
                    portalCollider.gameObject.layer = LayerMask.NameToLayer(PortalMask);
                    return CheckOverlap(true);
                }
            }

            return true;
        }

        public void RemovePortals()
        {
            foreach (var portal in _portals) {
                portal.gameObject.SetActive(false);
            }
        }
    }
}
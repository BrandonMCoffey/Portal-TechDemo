using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Portals
{
    public class PortalController : MonoBehaviour
    {
        [SerializeField] private bool _disableAtStart = true;
        [SerializeField] private LayerMask _wallMask = 0;
        [SerializeField] private List<Portal> _portals = new List<Portal>(2);

        private List<GameObject> _tempObjects = new List<GameObject>();

        private int _portalToPlace;

        private bool _bothPortalsPlaced = true;

        private const float PortalHeight = 1.15f;
        private const float PortalWidth = 0.575f;
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

            if (!_disableAtStart) return;
            foreach (var portal in _portals) {
                portal.gameObject.SetActive(false);
            }
            _bothPortalsPlaced = false;
        }

        public void ResetPortals(bool reset)
        {
            if (reset) {
                _bothPortalsPlaced = false;
                foreach (var p in _portals) {
                    p.gameObject.SetActive(false);
                }
            } else {
                _bothPortalsPlaced = true;
                foreach (var p in _portals) {
                    _bothPortalsPlaced &= p.isActiveAndEnabled;
                    if (p.isActiveAndEnabled) p.BufferTravelers();
                }
            }
        }

        public void SetMainCamera(Camera playerCamera)
        {
            foreach (var portal in _portals) {
                portal.PlayerCamera = playerCamera;
            }
        }

        public void RenderPortals()
        {
            if (!_bothPortalsPlaced) {
                foreach (var portal in _portals.Where(portal => portal.isActiveAndEnabled)) {
                    portal.SetInactiveColor();
                }
                return;
            }
            foreach (var portal in _portals) {
                portal.PreRenderPortal();
            }
            foreach (var portal in _portals) {
                portal.RenderPortal();
            }
            foreach (var portal in _portals) {
                portal.PostRenderPortal();
            }
        }

        public void PlacePortal(PortalVariable portal)
        {
            for (int i = _tempObjects.Count - 1; i > 0; i--) {
                Destroy(_tempObjects[i]);
            }

            if (!portal.ValidLocation) return;
            _portalToPlace = portal.PortalID;

            _portalPlacementTest.position = portal.Position;
            _portalPlacementTest.rotation = portal.Rotation;
            _portalPlacementTest.position -= portal.Forward * 0.001f;

            FixUpsideDown();

            _portalPlacementTest.Translate(-_portalPlacementTest.up * 0.5f);

            //Debug.Log("Start:            " + _portalPlacementTest.position);
            FixOverhangs();
            //Debug.Log("Overhang:   " + _portalPlacementTest.position);
            FixIntersects();
            //Debug.Log("Intersect:     " + _portalPlacementTest.position);

            if (!CheckOverlap()) return;
            //Debug.Log("Overlap:           " + _portalPlacementTest.position);

            _portals[_portalToPlace].gameObject.SetActive(true);
            _portals[_portalToPlace].RemoveTravelers();
            _portals[_portalToPlace].WallCollider = portal.Collider;
            _portals[_portalToPlace].transform.position = _portalPlacementTest.position - _portalPlacementTest.forward.normalized * 0.05f;
            _portals[_portalToPlace].transform.rotation = _portalPlacementTest.rotation;
            if (_portalToPlace == 0) _portals[_portalToPlace].transform.rotation *= Quaternion.Euler(0, 180, 0);

            _bothPortalsPlaced = true;
            foreach (var p in _portals) {
                _bothPortalsPlaced &= p.isActiveAndEnabled;
                if (p.isActiveAndEnabled) p.BufferTravelers();
            }
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

            var testDistances = new List<float> { PortalWidth + 0.1f, PortalWidth + 0.1f, PortalHeight + 0.1f, PortalHeight + 0.1f };

            for (int i = 0; i < 4; ++i) {
                Vector3 raycastPos = _portalPlacementTest.TransformPoint(0.0f, 0.0f, -0.1f);
                Vector3 raycastDir = _portalPlacementTest.TransformDirection(testDirs[i]);

                if (Physics.Raycast(raycastPos, raycastDir, out var hit, testDistances[i], _wallMask)) {
                    var offset = hit.point - raycastPos;
                    var newOffset = -raycastDir * (testDistances[i] - offset.magnitude);
                    _portalPlacementTest.Translate(newOffset, Space.World);
                }
            }
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
                    if (_portals[_portalToPlace].isActiveAndEnabled) {
                        if (portalCollider == _portals[_portalToPlace].Collider) continue;
                        Transform parent = portalCollider.transform.parent;
                        Collider collider = null;
                        if (parent != null) collider = parent.GetComponent<Collider>();
                        if (collider != null && collider == _portals[_portalToPlace].Collider) continue;
                    }

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

        private void FixUpsideDown()
        {
            if (_portalPlacementTest.up == Vector3.down) {
                _portalPlacementTest.Rotate(new Vector3(0, 0, 180));
            }
        }

        public void RemovePortals()
        {
            foreach (var portal in _portals) {
                portal.gameObject.SetActive(false);
            }
        }
    }
}
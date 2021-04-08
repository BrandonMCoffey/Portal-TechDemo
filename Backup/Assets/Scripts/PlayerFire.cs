using System.Collections;
using System.Security.Cryptography;
using Assets.Scripts.Portals;
using Assets.Scripts.Utility;
using UnityEngine;

namespace Assets.Scripts {
    public class PlayerFire : MonoBehaviour {
        [Header("Settings")]
        [SerializeField] private float _fireMaxDistance = 500;
        [SerializeField] private LayerMask _surfaceMask = 0;
        [SerializeField] private LayerMask _enemyMask = 0;
        [SerializeField] private LayerMask _portalMask = 0;
        [SerializeField] private LayerMask _ignoreMask = 0;
        [SerializeField] private bool _shootThroughPortals = false;

        [Header("References")]
        [SerializeField] private PortalVariable _impactTransform = null;
        [SerializeField] private GameEvent _onPlayerFired = null;
        [SerializeField] private BoolVariable _playerHasControl = null;

        private void Awake()
        {
            if (_impactTransform == null) Debug.Log("[" + GetType().Name + "] Impact Transform Variable missing on " + name);
            if (_onPlayerFired == null) Debug.Log("[" + GetType().Name + "] On Player Fired Event missing on " + name);
            if (_playerHasControl == null) Debug.Log("[" + GetType().Name + "] Player Has Control Bool Variable missing on " + name);
        }

        private void Update()
        {
            if (_playerHasControl != null) {
                if (!_playerHasControl.Value) return;
            }

            if (Input.GetButtonDown("Fire1")) {
                FireWeapon(0);
            } else if (Input.GetButtonDown("Fire2")) {
                FireWeapon(1);
            }
        }

        private void FireWeapon(int num)
        {
            if (_impactTransform != null) {
                _impactTransform.ValidLocation = false;
                FirePortal(num, transform.parent.position, transform.parent.forward, _fireMaxDistance);
            }
            if (_onPlayerFired != null) _onPlayerFired.Raise();
        }

        private void FirePortal(int portalId, Vector3 pos, Vector3 dir, float distance)
        {
            Physics.Raycast(pos, dir, out var hit, distance, ~_ignoreMask);

            if (hit.collider == null) {
                // Debug.Log("Sorry, you cant portal to space... Yet!");
                _impactTransform.Position = pos + dir * distance;
                return;
            }
            if (((1 << hit.collider.gameObject.layer) & _surfaceMask) != 0) {
                // Debug.Log("Portalable Surface!");
                _impactTransform.ValidLocation = true;
                SetPortalVariable(hit, portalId);
            } else if (((1 << hit.collider.gameObject.layer) & _portalMask) != 0) {
                // Debug.Log("Can you shoot portals through portals?");
                var inPortal = hit.collider.GetComponent<Portal>();
                if (_shootThroughPortals && inPortal != null && inPortal.OtherPortal != null) {
                    // If we shoot a portal, recursively fire through the portal.
                    var outPortal = inPortal.OtherPortal;

                    // Update position of raycast origin with small offset.
                    Vector3 relativePos = inPortal.transform.InverseTransformPoint(hit.point + dir);
                    relativePos = Quaternion.Euler(0.0f, 180.0f, 0.0f) * relativePos;
                    pos = outPortal.transform.TransformPoint(relativePos);

                    // Update direction of raycast.
                    Vector3 relativeDir = inPortal.transform.InverseTransformDirection(dir);
                    relativeDir = Quaternion.Euler(0.0f, 180.0f, 0.0f) * relativeDir;
                    dir = outPortal.transform.TransformDirection(relativeDir);

                    distance -= Vector3.Distance(pos, hit.point);

                    FirePortal(portalId, pos, dir, distance - hit.distance);
                } else {
                    FirePortal(portalId, hit.point + dir / 100, dir, distance - hit.distance);
                }
            } else if (((1 << hit.collider.gameObject.layer) & _enemyMask) != 0) {
                // Debug.Log("You hit an enemy. Uh, it died?");
                SetPortalVariable(hit, portalId);
            } else {
                // Debug.Log("You fool. You can portal here.");
                SetPortalVariable(hit, portalId);
            }
        }

        private void SetPortalVariable(RaycastHit hit, int id)
        {
            // Orient the portal according to camera look direction and surface direction.
            var cameraRotation = transform.parent.rotation;
            var portalRight = cameraRotation * Vector3.right;

            if (Mathf.Abs(portalRight.x) >= Mathf.Abs(portalRight.z)) {
                portalRight = (portalRight.x >= 0) ? Vector3.right : -Vector3.right;
            } else {
                portalRight = (portalRight.z >= 0) ? Vector3.forward : -Vector3.forward;
            }

            var portalForward = -hit.normal;
            var portalUp = -Vector3.Cross(portalRight, portalForward);

            var portalRotation = Quaternion.LookRotation(portalForward, portalUp);

            // Attempt to place the portal.
            _impactTransform.PortalID = id;
            _impactTransform.Collider = hit.collider.GetComponent<Collider>();
            _impactTransform.Position = hit.point;
            _impactTransform.Rotation = portalRotation;

            // bool wasPlaced = portals.Portals[portalId].PlacePortal(hit.collider, hit.point, portalRotation);
        }
    }
}
using Portals;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(Camera))]
    public class PlayerCamera : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PortalController _portalController = null;

        private void Start()
        {
            if (_portalController != null) _portalController.SetMainCamera(GetComponent<Camera>());
        }

        private void OnPreCull()
        {
            if (_portalController != null) _portalController.RenderPortals();
        }
    }
}
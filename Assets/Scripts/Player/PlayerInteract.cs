using Interactables;
using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    [RequireComponent(typeof(Camera))]
    public class PlayerInteract : MonoBehaviour
    {
        [SerializeField] private Image _cursorImage = null;

        private bool _canInteract;
        private RaycastHit _raycastFocus;

        private Camera _camera;

        private InteractableObject _interactedObject;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void Update()
        {
            if (Input.GetButtonDown("Interact") && _canInteract) {
                Interaction();
            }
        }


        private void FixedUpdate()
        {
            Ray ray = new Ray(_camera.transform.position, _camera.transform.forward);

            if (Physics.Raycast(ray, out _raycastFocus, 3) && _raycastFocus.collider.gameObject.layer == LayerMask.NameToLayer("Interactables")) {
                if (_cursorImage != null) _cursorImage.color = Color.green;
                _canInteract = true;
            } else {
                if (_cursorImage != null) _cursorImage.color = Color.white;
                _canInteract = false;
            }
        }

        private void Interaction()
        {
            Debug.Log(_interactedObject);
            if (_interactedObject == null) {
                InteractableObject interactComponent = _raycastFocus.collider.transform.GetComponent<InteractableObject>();
                if (interactComponent != null) {
                    _interactedObject = interactComponent;
                    interactComponent.Interact(_camera.transform, 3);
                    _canInteract = false;
                }
            } else {
                _interactedObject.Drop();
                _interactedObject = null;
                _canInteract = true;
            }
        }
    }
}
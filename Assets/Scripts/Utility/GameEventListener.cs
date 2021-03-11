using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Utility {
    public class GameEventListener : MonoBehaviour {
        [Tooltip("Event to register with.")]
        public GameEvent Event;

        [Tooltip("Response to invoke when Event is raised.")]
        public UnityEvent Response;

        private void OnEnable()
        {
            if (Event != null) Event.RegisterListener(this);
        }

        private void OnDisable()
        {
            if (Event != null) Event.UnRegisterListener(this);
        }

        public void OnEventRaised()
        {
            Response.Invoke();
        }
    }
}
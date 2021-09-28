using UnityEngine;
using UnityEngine.Events;
using Utility.GameEvents.Logic;

namespace Utility.GameEvents
{
    public class ActionOnEvent : GameEventListener
    {
        [Tooltip("Response to invoke when Event is raised.")]
        public UnityEvent Response;

        public override void OnEventRaised()
        {
            Response.Invoke();
        }
    }
}
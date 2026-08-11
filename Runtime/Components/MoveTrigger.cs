using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Jeomseon.UI.Components
{
    public class MoveTrigger : MonoBehaviour, IMoveHandler
    {
        public event UnityAction<AxisEventData> OnMoveEvent
        {
            add => onMoveEvent.AddListener(value);
            remove => onMoveEvent.RemoveListener(value);
        }
        
        [SerializeField, FormerlySerializedAs("_onMoveEvent")] private UnityEvent<AxisEventData> onMoveEvent = new();

        public void OnMove(AxisEventData eventData)
        {
            onMoveEvent.Invoke(eventData);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Jeomseon.Unity.UI.Components
{
    public class PointerDownTrigger : MonoBehaviour, IPointerDownHandler
    {
        public event UnityAction<PointerEventData> OnPointerDownEvent
        {
            add => onPointerDownEvent.AddListener(value);
            remove => onPointerDownEvent.RemoveListener(value);
        }
        
        [SerializeField, FormerlySerializedAs("_onPointerDownEvent")] private UnityEvent<PointerEventData> onPointerDownEvent = new();

        public void OnPointerDown(PointerEventData eventData)
        {
            onPointerDownEvent.Invoke(eventData);
        }
    }
}

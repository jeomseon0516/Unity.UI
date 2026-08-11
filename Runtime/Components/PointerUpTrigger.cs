using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Jeomseon.UI.Components
{
    public class PointerUpTrigger : MonoBehaviour, IPointerUpHandler
    {
        public event UnityAction<PointerEventData> OnPointerUpEvent
        {
            add => onPointerUpEvent.AddListener(value);
            remove => onPointerUpEvent.RemoveListener(value);
        }
        
        [SerializeField, FormerlySerializedAs("_onPointerUpEvent")] private UnityEvent<PointerEventData> onPointerUpEvent = new();
        
        public void OnPointerUp(PointerEventData eventData)
        {
            onPointerUpEvent.Invoke(eventData);
        }
    }
}
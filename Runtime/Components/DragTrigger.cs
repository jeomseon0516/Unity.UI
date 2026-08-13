using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Jeomseon.Unity.UI.Components
{
    public class DragTrigger : MonoBehaviour, IDragHandler
    {
        public event UnityAction<PointerEventData> OnDragEvent
        {
            add => onDragEvent.AddListener(value);
            remove => onDragEvent.RemoveListener(value);
        }
        
        [SerializeField, FormerlySerializedAs("_onDragEvent")] private UnityEvent<PointerEventData> onDragEvent = new();
        
        public void OnDrag(PointerEventData eventData)
        {
            onDragEvent.Invoke(eventData);
        }
    }
}

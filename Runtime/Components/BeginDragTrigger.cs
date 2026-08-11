using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Jeomseon.UI.Components
{
    public class BeginDragTrigger : MonoBehaviour, IBeginDragHandler
    {
        public event UnityAction<PointerEventData> OnBeginDragEvent
        {
            add => onBeginDragEvent.AddListener(value);
            remove => onBeginDragEvent.RemoveListener(value);
        }
        
        [SerializeField, FormerlySerializedAs("_onBeginDragEvent")] private UnityEvent<PointerEventData> onBeginDragEvent = new();        
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            onBeginDragEvent.Invoke(eventData);
        }
    }
}
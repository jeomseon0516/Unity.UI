using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Jeomseon.Unity.UI.Components
{
    public class EndDragTrigger : MonoBehaviour, IEndDragHandler
    {
        public event UnityAction<PointerEventData> OnEndDragEvent
        {
            add => onEndDragEvent.AddListener(value);
            remove => onEndDragEvent.RemoveListener(value);
        }
        
        [SerializeField, FormerlySerializedAs("_onEndDragEvent")] private UnityEvent<PointerEventData> onEndDragEvent = new();
        
        public void OnEndDrag(PointerEventData eventData)
        {
            onEndDragEvent.Invoke(eventData);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Jeomseon.Unity.UI.Components
{
    public class DropTrigger : MonoBehaviour, IDropHandler
    {
        public event UnityAction<PointerEventData> OnDropEvent
        {
            add => onDropEvent.AddListener(value);
            remove => onDropEvent.RemoveListener(value);
        }
        
        [SerializeField, FormerlySerializedAs("_onDropEvent")] private UnityEvent<PointerEventData> onDropEvent = new();
        
        public void OnDrop(PointerEventData eventData)
        {
            onDropEvent.Invoke(eventData);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Jeomseon.UI.Components
{
    public class PointerClickTrigger : MonoBehaviour, IPointerClickHandler
    {
        public event UnityAction<PointerEventData> OnPointerClickEvent
        {
            add => onPointerClickEvent.AddListener(value);
            remove => onPointerClickEvent.RemoveListener(value);
        }
        
        [SerializeField, FormerlySerializedAs("_onPointerClickEvent")] private UnityEvent<PointerEventData> onPointerClickEvent = new();
        
        public void OnPointerClick(PointerEventData eventData)
        {
            onPointerClickEvent.Invoke(eventData);
        }
    }
}

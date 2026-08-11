using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Jeomseon.UI.Components
{
    public class InitializePotentialDragTrigger : MonoBehaviour, IInitializePotentialDragHandler
    {
        public event UnityAction<PointerEventData> OnInitializePotentialDragEvent
        {
            add => onInitializePotentialDragEvent.AddListener(value);
            remove => onInitializePotentialDragEvent.RemoveListener(value);
        }
        
        [SerializeField, FormerlySerializedAs("_onInitializePotentialDragEvent")] private UnityEvent<PointerEventData> onInitializePotentialDragEvent = new();
        
        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            onInitializePotentialDragEvent.Invoke(eventData);
        }
    }
}

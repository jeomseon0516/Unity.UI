using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Jeomseon.Unity.UI.Components
{
    public class CancelTrigger : MonoBehaviour, ICancelHandler
    {
        public event UnityAction<BaseEventData> OnCancelEvent
        {
            add => onCancelEvent.AddListener(value);
            remove => onCancelEvent.RemoveListener(value);
        }
        
        [SerializeField, FormerlySerializedAs("_onCancelEvent")] private UnityEvent<BaseEventData> onCancelEvent = new();
        
        public void OnCancel(BaseEventData eventData)
        {
            onCancelEvent.Invoke(eventData);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Jeomseon.Unity.UI.Components
{
    public class DeselectTrigger : MonoBehaviour, IDeselectHandler
    {
        public event UnityAction<BaseEventData> OnDeselectEvent
        {
            add => onDeselectEvent.AddListener(value);
            remove => onDeselectEvent.RemoveListener(value);
        }
        
        [SerializeField, FormerlySerializedAs("_onDeselectEvent")] private UnityEvent<BaseEventData> onDeselectEvent = new();
        
        public void OnDeselect(BaseEventData eventData)
        {
            onDeselectEvent.Invoke(eventData);
        }
    }
}

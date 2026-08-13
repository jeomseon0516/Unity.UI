using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Jeomseon.Unity.UI.Components
{
    public class SelectTrigger : MonoBehaviour, ISelectHandler
    {
        public event UnityAction<BaseEventData> OnSelectEvent
        {
            add => onSelectEvent.AddListener(value);
            remove => onSelectEvent.RemoveListener(value);
        }

        [SerializeField, FormerlySerializedAs("_onSelectEvent")] private UnityEvent<BaseEventData> onSelectEvent = new();

        public void OnSelect(BaseEventData eventData)
        {
            onSelectEvent.Invoke(eventData);
        }
    }
}

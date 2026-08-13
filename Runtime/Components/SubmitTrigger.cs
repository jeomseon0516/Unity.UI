using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Jeomseon.Unity.UI.Components
{
    public class SubmitTrigger : MonoBehaviour, ISubmitHandler
    {
        public event UnityAction<BaseEventData> OnSubmitEvent
        {
            add => onSubmitEvent.AddListener(value);
            remove => onSubmitEvent.RemoveListener(value);
        }
        
        [SerializeField, FormerlySerializedAs("_onSubmitEvent")] private UnityEvent<BaseEventData> onSubmitEvent = new();
        
        public void OnSubmit(BaseEventData eventData)
        {
            onSubmitEvent.Invoke(eventData);   
        }
    }
}

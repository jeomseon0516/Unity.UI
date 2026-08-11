using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Jeomseon.UI.Components
{
public class UpdateSelectedTrigger : MonoBehaviour, IUpdateSelectedHandler
{
    public event UnityAction<BaseEventData> OnUpdateSelectedEvent
    {
        add => onUpdateSelectedEvent.AddListener(value);
        remove => onUpdateSelectedEvent.RemoveListener(value);
    }
    
    [SerializeField, FormerlySerializedAs("_onUpdateSelectedEvent")] private UnityEvent<BaseEventData> onUpdateSelectedEvent = new();
    
    public void OnUpdateSelected(BaseEventData eventData)
    {
        onUpdateSelectedEvent.Invoke(eventData);
    }
}
}

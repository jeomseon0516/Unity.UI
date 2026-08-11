using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Jeomseon.UI.Components
{
public class ScrollTrigger : MonoBehaviour, IScrollHandler
{
    public event UnityAction<PointerEventData> OnScrollEvent
    {
        add => onScrollEvent.AddListener(value);
        remove => onScrollEvent.RemoveListener(value);
    }
    
    [SerializeField, FormerlySerializedAs("_onScrollEvent")] private UnityEvent<PointerEventData> onScrollEvent = new();
    
    public void OnScroll(PointerEventData eventData)
    {
        onScrollEvent.Invoke(eventData);
    }
}
}

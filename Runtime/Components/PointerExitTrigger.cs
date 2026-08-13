using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Jeomseon.Unity.UI.Components
{
    public class PointerExitTrigger : MonoBehaviour, IPointerExitHandler
    {
        public event UnityAction<PointerEventData> OnPointerExitEvent
        {
            add => onPointerExitEvent.AddListener(value);
            remove => onPointerExitEvent.RemoveListener(value);
        }

        [SerializeField, FormerlySerializedAs("_onPointerExitEvent")] private UnityEvent<PointerEventData> onPointerExitEvent = new();

        public void OnPointerExit(PointerEventData eventData)
        {
            onPointerExitEvent.Invoke(eventData);
        }
    }
}

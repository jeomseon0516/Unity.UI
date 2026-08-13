using Jeomseon.Unity.UI.Components;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Jeomseon.Samples.UI
{
    [RequireComponent(typeof(PointerClickTrigger))]
    public sealed class PointerClickSample : MonoBehaviour
    {
        private void OnEnable()
        {
            GetComponent<PointerClickTrigger>().OnPointerClickEvent += OnClick;
        }

        private static void OnClick(PointerEventData eventData)
        {
            Debug.Log($"UI 클릭 위치: {eventData.position}");
        }
    }
}

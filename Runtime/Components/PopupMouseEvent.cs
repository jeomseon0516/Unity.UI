using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Jeomseon.Unity.UI.Components
{
    using static UIHelper;
    /// <summary>
    /// .. UI 팝업창에 쓸수있는 컴포넌트 입니다.
    /// 클릭했을때 RectTransform에 있는 요소들과 마우스 포인터가 충돌이 일어나지 않았을 경우 등록된 콜백 함수를 호출합니다.
    /// 마우스 포인터가 팝업 창 외부를 클릭했을때 팝업 창이 꺼지는 효과를 만들때 사용할 수 있습니다.
    /// </summary>
    public sealed class PopupMouseEvent : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        [field: Header("Functions")]
        [field: SerializeField] public UnityEvent OtherFunctions { get; private set; }

        [Header("Targets")]
        [SerializeField, FormerlySerializedAs("_rectTransforms")]
        private List<RectTransform> rectTransforms;

        private bool _isClick = false;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (rectTransforms.Any(rectTransform =>
                    RectTransformUtility.RectangleContainsScreenPoint(
                        rectTransform,
                        eventData.position,
                        eventData.pressEventCamera))) return;

            _isClick = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_isClick && !rectTransforms.Any(rectTransform =>
                    RectTransformUtility.RectangleContainsScreenPoint(
                        rectTransform,
                        eventData.position,
                        eventData.pressEventCamera)))
                OtherFunctions.Invoke();

            _isClick = false;
        }

        private void OnDisable()
        {
            _isClick = false;
        }


        public void AddUI(params RectTransform[] rectTransforms) => this.rectTransforms.AddRange(rectTransforms);
        public void DeleteUI(params RectTransform[] rectTransforms) => this.rectTransforms.RemoveAll(rectTransforms.Contains);
    }
}

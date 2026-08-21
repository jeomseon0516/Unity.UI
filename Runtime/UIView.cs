using Jeomseon.Unity.UI.Channels;
using UnityEngine.UIElements;

namespace Jeomseon.Unity.UI
{
    public abstract class UIView : VisualElement
    {
        public bool IsVisible { get; private set; }

        /// <summary>
        /// UI 열기·닫기를 요청하는 통로입니다. Notification 이벤트는 노출되지 않으므로 View가
        /// 다른 View의 상태 변화를 감지할 수 없습니다.
        /// </summary>
        protected IUIRequester Channel { get; private set; }

        protected UIView()
        {
            style.display = DisplayStyle.None;
            pickingMode = PickingMode.Position;
        }

        internal void Initialize(IUIRequester channel)
        {
            Channel = channel;
            OnScreenCreated();
        }

        /// <summary>
        /// 생성 훅을 다시 실행하지 않고 채널만 교체합니다. <see cref="Initialize"/>는 View를 만들 때
        /// 한 번만 호출하며, 이후 채널이 바뀌는 경우에는 이 메서드로 재구축 없이 연결만 옮깁니다.
        /// </summary>
        internal void SetChannel(IUIRequester channel) => Channel = channel;

        internal void SetVisible(bool visible)
        {
            if (IsVisible == visible) return;

            IsVisible = visible;
            style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;

            if (visible) OnShown();
            else OnHidden();
        }

        protected void RequestClose() => Channel.RequestClose(this);

        protected virtual void OnScreenCreated() { }

        protected virtual void OnShown() { }

        protected virtual void OnHidden() { }
    }
}

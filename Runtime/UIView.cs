using Jeomseon.Unity.UI.Channels;
using UnityEngine.UIElements;

namespace Jeomseon.Unity.UI
{
    public abstract class UIView : VisualElement
    {
        public bool IsVisible { get; private set; }

        protected UIChannel Channel { get; private set; }

        protected UIView()
        {
            style.display = DisplayStyle.None;
            pickingMode = PickingMode.Position;
        }

        internal void Initialize(UIChannel channel)
        {
            Channel = channel;
            OnScreenCreated();
        }

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

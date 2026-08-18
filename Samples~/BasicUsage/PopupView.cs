using Jeomseon.Unity.UI;
using UnityEngine.UIElements;

namespace Jeomseon.Samples.UI
{
    [UxmlElement]
    public sealed partial class PopupView : UIView
    {
        protected override void OnScreenCreated()
        {
            VisualElement backdrop = this.Q("backdrop");
            VisualElement popup = this.Q("popup");

            backdrop.RegisterCallback<PointerDownEvent>(_ => RequestClose());
            popup.RegisterCallback<PointerDownEvent>(evt => evt.StopPropagation());
            this.Q<Button>("close-button").clicked += RequestClose;
        }
    }
}

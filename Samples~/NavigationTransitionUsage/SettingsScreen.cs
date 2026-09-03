using Jeomseon.Unity.UI;
using UnityEngine.UIElements;

namespace Jeomseon.Samples.UI
{
    [UxmlElement]
    public sealed partial class SettingsScreen : UIView
    {
        protected override void OnScreenCreated()
        {
            this.Q<Button>("to-menu").clicked += () => NavigationTransitionSample.Instance.Nav.PopTo<MenuScreen>();
            this.Q<Button>("back").clicked += () => NavigationTransitionSample.Instance.Nav.Back();
        }
    }
}

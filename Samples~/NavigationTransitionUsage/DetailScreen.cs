using Jeomseon.Unity.UI;
using UnityEngine.UIElements;

namespace Jeomseon.Samples.UI
{
    [UxmlElement]
    public sealed partial class DetailScreen : UIView
    {
        protected override void OnScreenCreated()
        {
            this.Q<Button>("to-settings").clicked += () => NavigationTransitionSample.Instance.Nav.Push<SettingsScreen>();
            this.Q<Button>("back").clicked += () => NavigationTransitionSample.Instance.Nav.Back();
        }
    }
}

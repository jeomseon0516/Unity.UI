using Jeomseon.Unity.UI;
using UnityEngine.UIElements;

namespace Jeomseon.Samples.UI
{
    [UxmlElement]
    public sealed partial class MenuScreen : UIView
    {
        protected override void OnScreenCreated()
        {
            this.Q<Button>("to-detail").clicked += () => NavigationTransitionSample.Instance.Nav.Push<DetailScreen>();
            this.Q<Button>("to-settings").clicked += () => NavigationTransitionSample.Instance.Nav.Push<SettingsScreen>();
        }
    }
}

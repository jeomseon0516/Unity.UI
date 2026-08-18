using Jeomseon.Unity.UI;
using UnityEngine.UIElements;

namespace Jeomseon.Samples.UI
{
    // .. System 레이어 예시입니다. backdrop 없이 화면 일부만 덮는 비모달 토스트라 Popup이 열려
    // .. 있어도 Popup 위에서 계속 조작할 수 있습니다(ADR-0008 3절).
    [UxmlElement]
    public sealed partial class LoadingView : UIView
    {
        protected override void OnScreenCreated()
        {
            this.Q<Button>("dismiss-button").clicked += RequestClose;
        }
    }
}

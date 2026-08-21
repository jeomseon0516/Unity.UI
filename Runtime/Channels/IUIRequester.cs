namespace Jeomseon.Unity.UI.Channels
{
    /// <summary>
    /// UI 열기·닫기를 요청하는 최소 계약입니다. <see cref="UIView"/>는 이 계약만 보므로 다른 View의
    /// 열림·닫힘 Notification을 구독할 수 없습니다. View끼리 서로의 상태를 감지하는 암묵적 결합을
    /// 막기 위한 접근 범위 제한이며(ADR-0008), 구현을 교체하기 위한 추상화가 아닙니다.
    /// </summary>
    public interface IUIRequester
    {
        /// <summary>지정한 View 타입을 열도록 요청합니다.</summary>
        void RequestOpen<T>() where T : UIView;
        /// <summary>지정한 View를 닫도록 요청합니다.</summary>
        void RequestClose(UIView screen);
        /// <summary>모든 Layer의 열린 View를 닫도록 요청합니다.</summary>
        void RequestCloseAll();
    }
}

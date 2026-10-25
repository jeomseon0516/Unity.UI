namespace Jeomseon.Unity.UI
{
    // .. 선언 순서가 렌더링 순서입니다. 나중에 오는 레이어가 항상 앞 레이어 위에 그려집니다.
    // .. System은 로딩 화면·전역 알림/토스트처럼 Popup 위에 항상 떠 있어야 하는 화면 전용
    // .. 레이어입니다. Manager는 UILayer 전체를 열거해 각 레이어를 독립적으로 처리합니다.
    public enum UILayer
    {
        Screen,
        Popup,
        System
    }
}

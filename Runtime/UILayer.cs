namespace Jeomseon.Unity.UI
{
    // .. 선언 순서가 렌더링 순서입니다. 나중에 오는 레이어가 항상 앞 레이어 위에 그려집니다.
    // .. System은 로딩 화면·전역 알림/토스트처럼 Popup 위에 항상 떠 있어야 하는 화면 전용
    // .. 레이어입니다(ADR-0008 3절, 고정 3단계 모델). 아직 이 레이어를 쓰는 Sample/Test 화면은
    // .. 없지만(커버리지 공백), Manager가 UILayer 전체를 열거해 처리하므로 삭제 대상이 아닙니다.
    public enum UILayer
    {
        Screen,
        Popup,
        System
    }
}

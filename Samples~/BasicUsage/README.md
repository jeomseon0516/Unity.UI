# UI Toolkit Basic Usage

`UIBasicUsageSample.unity`를 열고 Play Mode를 시작합니다.

1. `HomeView`가 자동으로 열리는지 확인합니다.
2. Previous/Next 버튼과 드래그로 `UICarousel`의 선택 항목을 바꿉니다.
3. Game View 너비를 바꾸며 `UIGrid`의 셀 크기와 간격이 함께 변하는지 확인합니다.
4. Open Popup을 누른 뒤 팝업 내부 클릭은 유지되고, Close 또는 어두운 backdrop 클릭은 팝업만 닫는지 확인합니다.

Scene의 `UIStackManager`와 화면은 서로 직접 참조하지 않습니다. 같은 `UIChannel` 자산이 화면 카탈로그와 열기·닫기 요청 채널을 겸합니다.

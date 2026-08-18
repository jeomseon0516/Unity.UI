# UI Toolkit Basic Usage

`UIBasicUsageSample.unity`를 열고 Play Mode를 시작합니다.

1. `HomeView`가 자동으로 열리는지 확인합니다.
2. Previous/Next 버튼과 드래그로 `UICarousel`의 선택 항목을 바꿉니다.
3. Game View 너비를 바꾸며 `UIGrid`의 셀 크기와 간격이 함께 변하는지 확인합니다. 셀이 화면
   높이를 넘으면 스크롤되는지도 확인합니다. Grid 영역은 `UIScrollView`라 **마우스로 셀을 직접
   잡아끌어도 스크롤됩니다.** 위아래 끝을 넘겨 당기면 고무줄처럼 저항하다 놓으면 되돌아오고,
   빠르게 튕기면 관성으로 미끄러집니다. 셀을 짧게 클릭하는 것과 끌어서 스크롤하는 것이 서로
   방해하지 않는지도 함께 확인합니다.
   (기본 UI Toolkit `ScrollView`는 이 세 동작을 터치 입력에서만 지원합니다 — Unity의 의도된
   설계이며, `UIScrollView`가 마우스에서도 되도록 채웁니다.)
4. Open Popup을 누른 뒤 팝업 내부 클릭은 유지되고, Close 또는 어두운 backdrop 클릭은 팝업만 닫는지 확인합니다.
5. Open System Toast를 눌러 `System` 레이어 화면(`LoadingView`)이 우측 상단에 뜨는지 확인합니다.
   토스트를 띄운 채로 Open Popup을 눌러 팝업을 열면, 팝업의 어두운 backdrop 위로 토스트가 계속
   보이는지 확인합니다(System은 Popup보다 항상 위 레이어). 토스트는 backdrop이 없어 팝업이 열려
   있어도 계속 화면에 남아 있습니다. Dismiss로 닫습니다.

Scene의 `UIStackManager`와 화면은 서로 직접 참조하지 않습니다. 같은 `UIChannel` 자산이 화면 카탈로그와 열기·닫기 요청 채널을 겸합니다.

Scene에는 UI 외에 두 오브젝트가 더 있으며, 둘 다 UI Toolkit 동작에 필요합니다.

- `Main Camera` — UI Toolkit 전용 Scene이라도 Camera가 하나도 없으면 프레임버퍼가 클리어되지 않아
  이전 프레임이 잔상으로 남습니다.
- `EventSystem`(+ `InputSystemUIInputModule`) — 이게 없으면 버튼 클릭과 드래그 입력이 패널로
  전달되지 않습니다.

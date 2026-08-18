# UI 로드맵

우선순위: `P0` 결함·안전성 → `P1` 핵심 구조 → `P2` API·성능 → `P3` 장기 확장

## 작업 순서

1. **P0-01/P0-02 — UIManager 수명·스택 안정화** (uGUI 구현 폐기, `ADR-0008`로 대체됨, 2026-08-18)
   - 원래 계획은 uGUI `UIManager`/`BaseUI`의 결함(파괴된 UI가 활성 스택에 남는 경우, 등록되지 않은
     UI를 닫을 때 전체 스택이 닫히는 경우, `baseUIList` 중복 키 등록 시 예외)을 직접 수정하는
     것이었습니다. 실제로 이 세 결함을 발견·수정까지 했으나(커밋 전), 사용자가 uGUI 자체를
     UI Toolkit으로 전면 재설계하기로 결정해 uGUI `UIManager`/`BaseUI`를 통째로 삭제했습니다.
   - 아래 P2-01(현재 완료)의 새 `UIStackManager`/`UIView`는 이 세 결함에 해당하는 문제를 구조적으로
     갖지 않거나(파쇄된 참조 — `UIView`는 `VisualElement`라 Unity native 파괴/fake-null 개념이 없어
     `MissingReferenceException` 자체가 불가능) 새 구현에서 동일하게 방어합니다(등록되지 않은 화면을
     닫아도 무관한 화면에 영향 없음, 중복 타입 등록은 경고만 남기고 무시). 상세는 P2-01 참고.
2. **P1-01 — UI 등록 설정 에셋** (완료, `ADR-0008`, 2026-08-18)
   - `UIChannel`(ScriptableObject)에 `{Layer, VisualTreeAsset}` 카탈로그 엔트리를 Inspector에서
     구성합니다. UXML 루트 자체를 `[UxmlElement] UIView` 파생 타입으로 선언하고 Manager가
     `VisualTreeAsset.Instantiate()`로 생성된 실제 View를 등록합니다. 타입 문자열·전용 Drawer·
     `Type.GetType`·`TypeCache`·`Activator.CreateInstance`는 사용하지 않습니다.
3. **P1-02 — 비동기 UI 로딩**
   - Addressables를 강제하지 않는 loader 계약과 로딩 취소·실패 정책을 설계합니다. 카탈로그 밖에서
     코드로 화면을 등록하는 경로(예: 런타임에 Addressables로 받아온 화면)는 아직 없습니다 — 이
     항목에서 함께 설계합니다.
4. **P2-01 — UI Toolkit 전면 전환** (Core 구현 완료, Unity 검증 대기, 2026-08-18, `ADR-0008` 참고)
   - `UIStackManager`/`UIView`(옛 `UIManager`/`BaseUI`)를 `UIDocument`/`VisualElement` 기반으로
     재설계했습니다. 책임을 "타입 기반 화면 조회·등록, 레이어별 스택 북키핑, 정렬"로 좁히고,
     레이어 간·레이어 내 입력 차단은 UI Toolkit의 피킹(트리 순서)에 맡기고 코드로 구현하지
     않습니다(backdrop 컨벤션으로 해결, `ADR-0008` 3절).
   - `UILayer` 3단계(`Screen`/`Popup`/`System`) 고정, 레이어별 독립 스택.
   - `UIStackManager`와 `UIView`는 서로를 직접 참조하지 않고 `UIChannel`(ScriptableObject)을 통해서만
     상호작용합니다. `UIStackManager`는 일반 `MonoBehaviour`이며 정적 `Instance` 접근을 제공하지
     않습니다. Core는 VContainer 등 특정 DI 컨테이너에 의존하지 않으며, 나중에 DI가 필요해지면 같은
     `UIChannel` 자산을 주입하는 것으로 해결됩니다(`ADR-0008` 5절).
   - 채널 요청 처리 결과는 `UIChannel.ScreenOpened`/`ScreenClosed`/`AllScreensClosed` public event로
     발행합니다. Inspector 영구 리스너는 선택적 `UIChannelListener`가 채널을 구독해 private
     UnityEvent로 중계합니다. 프로젝트 자산인 `UIChannel`이 Scene 객체를 직접 참조하지 않습니다.
   - Manager의 직접 `GetUI`/`OpenUI`/`CloseUI`/`CloseAllUI` public API는 제거했습니다. 외부 요청과
     완료 알림은 모두 `UIChannel`을 통과하며, 테스트용 화면 조회·등록 경계만 internal입니다.
   - `Tests/Runtime/UIStackManagerPlayModeTests`(4개: 열기/닫기 가시성, 중복 등록 무시, 스택에 없는
     화면 닫기 무동작, 채널 Request 이벤트 도달)로 검증합니다. `dotnet build`로 Runtime/Editor/Tests
     전부 컴파일 오류 0개 확인. **Unity Test Runner 실행 확인 대기**(TestProject가 사용자 Editor에서
     열려 있어 이번 세션엔 배치모드 실행 불가).
   - **나머지 uGUI `Components/*`는 UI Toolkit이 이미 동등 기능을 기본 제공하는지 개별 확인 후
     삭제했습니다**(재구현이 아니라 제거, 전부 실행 완료):
     - Trigger 20종(`PointerClickTrigger` 등 `I*Handler` 래퍼 전부) — `VisualElement.
       RegisterCallback<TEvent>()`로 완전히 대체되어 래퍼 자체가 불필요.
     - `RangeAdjustmentSlider`(+`RangeAdjustmentSliderEditor`) — `UnityEngine.UIElements.
       MinMaxSlider`가 동일 기능 기본 제공.
     - `ToggleSelector` — `UnityEngine.UIElements.RadioButtonGroup`이 배타적 선택 그룹 +
       `RegisterValueChangedCallback`을 기본 제공.
     - `PopupMouseEvent`("팝업 밖 클릭 감지") — 팝업 자신의 backdrop 엘리먼트에
       `RegisterCallback<PointerDownEvent>` 하나면 충분해 별도 컴포넌트가 불필요.
     - `TmpAutoEditorRefresh` — uGUI+TMP 조합의 Editor 강제 리프레시 워크어라운드이며 UI Toolkit
       텍스트 렌더링에는 이 문제 자체가 없음.
     - `DragAndDropEvent` — `UIHelper`(uGUI `RectTransform` 전용) 삭제로 컴파일이 깨져 있었고,
       네이티브 대응도 없이 앱 특화적이라 **일단 삭제**. UI Toolkit 버전이 실제로 필요해지면
       `PointerCaptureEvent`/`Manipulator` 기반으로 재설계(백로그, 아래 참고).
     - `MessagePopup`/`WaitPopup` — 옛 `BaseUI`를 직접 상속해 삭제. 새 `UIView` 기반으로 Sample에서
       다시 작성할지는 별도 검토.
   - **네이티브 대응이 없어 실제 전환이 필요했던 것 — 재설계 완료, Unity 검증 대기(2026-08-18)**:
     기존 `HorizontalSelector`(버튼 전용 페이지 전환)와 `HorizontalEnumeratedItem`(드래그 스냅
     캐러셀)을 직접 읽어보니 "가로로 나열된 아이템 중 하나를 선택"이라는 같은 문제를 두 번 따로
     구현한 것이었고(`HorizontalEnumeratedItem`이 상위 호환), `EnumeratedElements`는 전혀 다른
     문제(콘텐츠 너비 비율 기반 `GridLayoutGroup` 반응형 자동 크기 조정, uGUI 스크롤뷰 인벤토리
     격자에 쓰였을 것으로 추정)였습니다. 사용자가 기존 사용성이 나빴다고 확인해 단순 포팅 대신
     재설계했습니다.
     - `Components/UICarousel.cs` — `HorizontalSelector`+`HorizontalEnumeratedItem` 통합.
       `Draggable` 플래그로 드래그 지원 여부만 선택(꺼면 옛 `HorizontalSelector`처럼 버튼 전용
       사용도 가능). `SelectedIndex`/`SelectedIndexChanged`, 드래그 시 탄성(`Elasticity`) 및
       스냅(`SnapSpeed`), `AddItem(s)`/`ClearItems`/`SelectPrevious`/`SelectNext` 제공.
       `IVisualElementScheduler`(`schedule.Execute().Every(16)`)로 애니메이션을 구동합니다
       (uGUI `FixedUpdate` 대응).
     - `Components/UIGrid.cs` — `EnumeratedElements` 대체. `ColumnCount`/
       `ItemWidthToHeightRatio`/`ItemSpacingRatio`/`PaddingRatio`를 콘텐츠 너비 비율로 셀
       크기·간격·패딩에 반영합니다. `flexDirection: Row`+`flexWrap: Wrap` 기반.
     - 둘 다 `[UxmlElement]`로 UXML에 직접 배치 가능합니다.
     - `com.unity.ugui`/`Unity.TextMeshPro` 의존성이 이제 전혀 필요 없어져 `package.json`
       dependencies와 Runtime/Editor asmdef 참조에서 제거했습니다. 사용처가 없고 UI Toolkit의
       `GeometryChangedEvent`로 대체되는 `ResolutionObserver`도 제거해 `Jeomseon.Unity.Singleton`
       의존성까지 제거했습니다. `validate-package.sh`/`git diff --check` 통과.
     - **드래그 물리(탄성 계수, 스냅 속도) 수치는 실제 조작감을 보지 못한 상태로 정한 초기값이라,
       Unity에서 만져보면 조정이 필요할 가능성이 높습니다.**
   - `Samples~/BasicUsage`를 새 `UIStackManager`/`UIView` 기준으로 다시 작성했습니다(2026-08-18).
     `UIDocument`/`PanelSettings`/`UIChannel`, `HomeView(Screen)`/`PopupView(Popup)`를 실제 Scene에
     직렬화했으며 backdrop 입력 차단, `UICarousel`, `UIGrid`를 한 Scene에서 확인할 수 있습니다.
     **Unity Editor 시각·조작 검증은 대기 중입니다.**
5. **P2-02 — Trigger 중복 축소** (위 P2-01 삭제 결정으로 해소, 별도 통합 구현 불필요)
6. **P3-01 — Navigation·Transition 확장**
   - 화면 전환, history, modal, animation은 Core 관리자와 분리된 선택 계층으로 설계합니다.

## 백로그

- `DragAndDropEvent`의 UI Toolkit 재설계 여부(길게 눌러 복제 이미지를 드래그하는 특수 동작). 필요성
  자체가 불확실해 재요청이 있을 때 착수합니다.

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
   - `UILayer` 3단계(`Screen`/`Popup`/`System`) 고정, 레이어별 독립 스택. `System`은 로딩 화면·전역
     알림/토스트처럼 Popup보다 항상 위에 떠야 하는 화면 전용 레이어로 `ADR-0008` 3절에 이미 확정된
     설계입니다. `UIStackManager.BuildCatalog()`가 `UILayer` 전체를 열거해 제네릭하게 지원하므로
     구현 누락이 아니며, 삭제 대상도 아닙니다. **다만 이 레이어를 실제로 쓰는 Sample 화면이나
     PlayMode 테스트가 아직 없어(커버리지 공백, 2026-08-18 확인) `Screen`/`Popup`과 달리 동작이
     검증된 적이 없습니다** — 다음 작업에서 System 레이어 화면(예: 로딩 오버레이) 하나를 Sample에
     추가해 실제로 확인해야 합니다.
   - `UIStackManager`와 `UIView`는 서로를 직접 참조하지 않고 `UIChannel`(ScriptableObject)을 통해서만
     상호작용합니다. `UIStackManager`는 일반 `MonoBehaviour`이며 정적 `Instance` 접근을 제공하지
     않습니다. Core는 VContainer 등 특정 DI 컨테이너에 의존하지 않으며, 나중에 DI가 필요해지면
     프로젝트 조합 루트가 DI로 확보한 `UIChannel`을 `UIStackManager.Initialize(UIChannel)`에 직접
     넘기는 것으로 해결됩니다(`ADR-0008` 5절). 패키지는 특정 DI 라이브러리를 참조하지 않습니다.
     한 줄만 위임하는 별도 `UIStackManagerInitializer` 래퍼 타입은 의미 없는 간접 계층이라
     추가하지 않기로 확정했습니다(2026-08-18, `AGENTS.md` 공통 판단 순서 3번).
   - 채널 요청 처리 결과는 `UIChannel.ScreenOpened(UIView)`/`ScreenClosed(UIView)`/
     `AllScreensClosed` public event로 발행합니다. `Type` 대신 실제 `UIView` 인스턴스를 전달해
     코드 구독자가 별도 변환 없이 타입 안전하게 접근할 수 있습니다. Inspector 영구 리스너는
     선택적 `UIChannelListener`가 `UnityEvent<UIView>`(Dynamic 바인딩)로 직접 중계합니다 —
     Dynamic 모드는 인자를 직렬화하지 않고 런타임 값을 그대로 넘기므로 `UnityEngine.Object`가
     아닌 `UIView`도 문제없이 Inspector에 노출됩니다(2026-08-18, `Type`→문자열 변환 방식에서
     전환). 프로젝트 자산인 `UIChannel`이 Scene 객체를 직접 참조하지 않습니다. 2026-08-19
     `Samples~/BasicUsage` Scene에 `screenOpened`/`screenClosed` Dynamic listener를 실제 직렬화하고
     Development Player에서 `HomeView` 런타임 인스턴스가 전달되는 것을 로그로 확인했습니다.
   - `OpenRequested`는 `Type`, `CloseRequested`는 `UIView` 인스턴스로 인자 타입이 다른데, 이는
     의도된 비대칭입니다(2026-08-18 확정). Open 요청 시점엔 아직 인스턴스가 없어 Type으로만 표현
     가능하고, Close 요청은 호출부가 이미 인스턴스를 쥐고 있는 경우(화면 자신의
     `RequestClose()`)가 대부분이라 `Instantiate<T>()`/`Destroy(instance)`와 같은 결입니다. 반면
     결과 알림(`ScreenOpened`/`ScreenClosed`)은 이 시점엔 인스턴스가 확정돼 있으므로 둘 다
     `UIView`로 통일합니다. 완전한 형태 통일보다 "그 시점에 아는 정보 그대로"가 우선이라는 원칙을
     따릅니다.
   - Manager의 직접 `GetUI`/`OpenUI`/`CloseUI`/`CloseAllUI` public API는 제거했습니다. 외부 요청과
     완료 알림은 모두 `UIChannel`을 통과하며, 테스트용 화면 조회·등록 경계만 internal입니다.
     - `Tests/Runtime/UIStackManagerPlayModeTests`(6개: 열기/닫기 가시성, System 레이어 열기/닫기,
       중복 등록 무시, 스택에 없는 화면 닫기 무동작, 채널 Request 이벤트 도달, 전체 레이어 닫기와
       완료 알림)로 검증합니다. `dotnet build`로 Runtime/Editor/Tests 전부 컴파일 오류 0개 확인.
       Unity Test Runner PlayMode 6개도 2026-08-19 실행해 전부 통과했습니다(0.185초, 실패 0).
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
     - `UICarousel`은 포인터 캡처 손실·취소 시에도 드래그 상태가 남지 않도록
       `PointerCancelEvent`/`PointerCaptureOutEvent`에서 스냅을 마무리합니다.
     - 둘 다 `[UxmlElement]`로 UXML에 직접 배치 가능합니다.
     - `com.unity.ugui`/`Unity.TextMeshPro` 의존성이 이제 전혀 필요 없어져 `package.json`
       dependencies와 Runtime/Editor asmdef 참조에서 제거했습니다. 사용처가 없고 UI Toolkit의
       `GeometryChangedEvent`로 대체되는 `ResolutionObserver`도 제거해 `Jeomseon.Unity.Singleton`
       의존성까지 제거했습니다. `validate-package.sh`/`git diff --check` 통과.
     - 드래그 물리(탄성 계수, 스냅 속도) 수치는 2026-08-18 시점엔 실측 전 초기값이라 재조정이
       필요할 것으로 봤으나, 2026-09-01 사용자 실측으로 조작감이 확인돼 안정화된 값으로
       유지합니다. (탄성 비활성화/하드 클램프 같은 추가 옵션은 P3-03에서 별도 검토.)
   - `Samples~/BasicUsage`를 새 `UIStackManager`/`UIView` 기준으로 다시 작성했습니다(2026-08-18).
     `UIDocument`/`PanelSettings`/`UIChannel`, `HomeView(Screen)`/`PopupView(Popup)`를 실제 Scene에
     직렬화했으며 backdrop 입력 차단, `UICarousel`, `UIGrid`를 한 Scene에서 확인할 수 있습니다.
     Development Player에서 Camera의 Solid Color 클리어와 초기 UI 렌더링을 확인했습니다
     (2026-08-19). 나머지 전체 Editor 조작 검증은 아래 미검증 항목만 남습니다.
     - `LoadingView(System)`를 추가해 System 레이어 실사용 예시를 보강했습니다(2026-08-18, 이전엔
       Runtime/Sample/Tests 어디에도 System 레이어 사용처가 없었던 커버리지 공백을 해소).
       `HomeView`의 "Open System Toast" 버튼으로 열며, backdrop 없이 화면 일부만 덮는 비모달
       토스트라(ADR-0008 3절) Popup이 열려 있어도 그 위에 계속 표시됩니다. `UIChannel.asset`에
       `layer: 2`(System) 엔트리로 등록했습니다.
     - **TestProject의 `Assets/Samples/Jeomseon Unity UI/0.5.0/Basic Usage`는 Unity Package
       Manager로 한 번 Import된 별도 복사본이라, 패키지 `Samples~`를 고쳐도 자동으로 갱신되지
       않습니다.** `LoadingView`를 포함한 이번 Sample 변경사항은 Unity에서 Sample을 다시
       Import해야 반영되고 검증 가능합니다 — 지금까지의 `dotnet build`
       현재는 0.5.0 Sample을 다시 Import했고, Dynamic listener 예시까지 패키지 원본과 동기화해
       Development Player 검증에 사용했습니다.
   - **`Editor/` 재도입 — `UIStackManager` Edit Mode 미리보기**(2026-08-18). uGUI Canvas와 달리
     `UIStackManager`의 화면 조립은 `Awake()`/`Initialize()` 등 일반 `MonoBehaviour` 생명주기에
     묶여 있어 Play Mode에서만 보였습니다. `UIStackManager` 런타임 코드 자체는 건드리지 않고,
     `[CustomEditor(typeof(UIStackManager))] UIStackManagerEditor`(신규 `Editor/`
     asmdef)가 selection 시점의 `Editor.OnEnable`/`OnDisable`(Play Mode와 무관, Inspector
     선택·해제에 연동)에 맞춰 기존 `Initialize(UIChannel)`/`ClearCatalog()`를 호출해 Edit Mode
     미리보기를 제공합니다. `UIStackManager`를 선택하면 카탈로그가 즉시 뜨고, 선택 해제하면
     정리됩니다. 이 기능을 위해 `ClearCatalog()`를 `private` → `internal`로 열었습니다
     (`AssemblyInfo.cs`의 기존 `InternalsVisibleTo("Jeomseon.Unity.UI.Editor")` 재사용).
     이번 세션 초반 "Editor 코드가 전혀 남지 않아 Editor asmdef를 삭제했다"고 기록했던 것과
     달리 다시 생겼습니다 — 이유는 이 Edit Mode 미리보기 기능 하나뿐입니다.
     - **등록된 화면은 `RegisterScreen`의 마지막에서 `SetVisible(false)`(`display: None`)로
       시작하고, 실제로 화면을 여는 것은 Play Mode에서만 도는 채널 요청입니다.** 그래서 카탈로그만
       구성하면 투명한 레이어 컨테이너 3개만 생겨 화면이 비어 보입니다. 미리보기는 Screen 레이어의
       첫 화면을 직접 `SetVisible(true)`로 표시합니다. 채널 요청(`RequestOpen`) 대신 직접 표시하는
       이유는 Edit Mode에서 소비자 콜백(`ScreenOpened` 등)을 실행시키지 않기 위해서입니다.
     - 가드는 `Application.isPlaying`만으로 부족합니다. **Play Mode 진입 전환 중에는
       `Application.isPlaying`이 아직 `false`인데 `UIDocument`는 이미 정리돼
       `rootVisualElement`가 `null`**이라 `BuildCatalog`에서 `NullReferenceException`이 납니다.
       `EditorApplication.isPlayingOrWillChangePlaymode`까지 함께 확인하고
       `rootVisualElement == null`도 방어합니다. `OnDisable`의 `ClearCatalog()`에도 같은 가드가
       필요합니다 — 없으면 Play Mode 진입 직후 `Awake`가 구성한 카탈로그를 Editor가 지웁니다.
     - **조사 과정에서 확인된 오해**: "`UIDocument`는 Edit Mode Game 뷰에 렌더링하지 않는다",
       "`sourceAsset`이 비어 있으면 절차적으로 추가한 `rootVisualElement` 자식이 리페인트에
       지워진다"는 두 가설은 **모두 사실이 아니었습니다.** 진단 로그로 Edit Mode에서도
       `panel=Player`, `rootWidth=885`, `childCount=3`이 유지되는 것을 확인했습니다. 빈 UXML을
       `sourceAsset`에 연결하는 우회는 불필요해 되돌렸습니다(`UIDocument.Source Asset`은 이
       Sample에서 계속 비어 있습니다 — 카탈로그로 여러 UXML을 조립하는 구조상 고정 UXML을 쓰지
       않습니다).
5. **P2-02 — Trigger 중복 축소** (위 P2-01 삭제 결정으로 해소, 별도 통합 구현 불필요)
6. **P2-03 — `ScrollView` 확장: uGUI `ScrollRect`와 기능적 동등성** (안정화·사용자 실측 완료,
   2026-09-01 확인)

   Sample 검증 중 UI Toolkit `ScrollView`가 마우스 환경에서 uGUI `ScrollRect` 대비 두 기능이 빠져
   있음이 확인됐습니다. 둘은 별개 결함이 아니라 **같은 뿌리(터치 전용 게이트)** 였습니다.

   | 기능 | uGUI `ScrollRect` | UI Toolkit 기본(마우스) | 본 패키지 확장 |
   | --- | --- | --- | --- |
   | 콘텐츠 드래그 스크롤 | O | **X** | O |
   | 고무줄 오버스크롤 | O (`Movement Type: Elastic`) | **X** | O |
   | 관성 스크롤 | O (`Inertia`) | **X** | O |
   | 휠 / 스크롤바 | O | O | O |

   - **Unity의 공식 입장**: 마우스 드래그 스크롤은 **의도적 미지원**입니다. Unity 담당자가 포럼에서
     "drag scroll은 터치 입력에서만 동작하며 일반적인 마우스 플랫폼 동작을 따른 것"이라고
     답변했고, 제시된 해결책은 `ScrollView.cs` 소스 수정이나 커스텀 컨트롤 작성뿐이었습니다.
     공식 문서도 `touchScrollBehavior`/`elasticity`/`scrollDecelerationRate`가 전부 **터치 인터랙션
     한정**임을 명시합니다.
   - **구현**: `Components/ScrollDragManipulator`(로직)와 `Components/UIScrollView`(`[UxmlElement]`
     ScrollView 서브클래스, Manipulator를 기본 부착)로 나눴습니다. 코드에서는 기존 `ScrollView`나
     내부에 ScrollView를 갖는 `ListView` 등에 `AddManipulator`로 재사용할 수 있고, UXML/UI
     Builder에서는 `UIScrollView`를 바로 배치합니다.
   - **포인터 타입을 구분하지 않습니다.** 마우스·터치가 같은 코드 경로로 동작하므로 아래 미검증
     항목(터치 경로)에 의존하지 않습니다.
   - **경계 초과 표현 방식**: `ScrollView.scrollOffset`은 범위로 clamp되므로 고무줄을 표현할 수
     없습니다. 범위 안은 `scrollOffset`으로, 초과분은 `contentContainer`의 `translate`로 나눠
     적용합니다.
   - **자식 클릭 취소**: 포럼에서 반복 지적된 난점입니다. `DragThreshold`를 넘는 순간
     `CapturePointer`로 포인터를 가로채면 자식 `Button`이 `PointerCaptureOutEvent`를 받아 클릭이
     취소됩니다. 임계값 이하의 움직임은 그대로 클릭으로 처리됩니다.
   - **속성 이름에 `Drag` 접두사**를 둬 `ScrollView`가 이미 가진 `elasticity`·
     `scrollDecelerationRate`와 UXML 속성 이름이 충돌하지 않게 했습니다.
   - **축 선택**: 축 허용의 1차 기준은 **`ScrollView.mode`**이고, `DragHorizontal`/`DragVertical`
     (uGUI `ScrollRect`의 Horizontal/Vertical 체크박스에 대응)이 거기서 더 좁힙니다. 끌 수 없는
     축은 이동량에서 제외되고 **드래그 인정 임계값 계산에서도 빠집니다** — 그래야 세로 전용
     스크롤뷰에서 가로로만 흔들었을 때 드래그가 시작되지 않습니다.
     - **초기 구현 결함**(2026-08-18, 사용자 제보 "Vertical인데 가로로도 드래그된다"):
       `ScrollView.mode`를 전혀 보지 않았습니다. `MaxOffset()`이 `horizontalScroller.highValue`를
       쓰므로 mode가 `Vertical`이면 가로 범위는 0이라 **스크롤은 안 되지만**, 고무줄 로직은 그
       범위 밖에서 따로 동작하므로 초과분이 translate에 반영되어 가로로 끌려다녔습니다. 스크롤
       범위가 0인 것과 "그 축을 아예 건드리지 않는 것"은 다르다는 점이 핵심입니다.
   - **가로 드래그 경로는 미검증입니다**(2026-08-18). Sample의 `UIGrid`는 셀 폭을 퍼센트로 잡아
     콘텐츠 박스를 정확히 채우므로 **구조상 가로로 넘치지 않고**, 따라서
     `horizontalScroller.highValue`가 0이라 가로 스크롤 자체가 발생하지 않습니다. 검증하려면
     뷰포트보다 넓은 콘텐츠가 필요하지만, 이번에는 관성 확인이 목적이라 별도 테스트 콘텐츠를
     만들지 않기로 했습니다. 세로 경로만 실제로 확인된 상태입니다.
   - **초기 구현에서 잡은 결함 2건**(2026-08-18, 사용자 제보 "드래그하면 계속 위로 튄다"):
     1. 스크롤 범위를 `contentContainer`와 뷰포트의 `resolvedStyle` 차이로 직접 계산했는데,
        `contentContainer`가 뷰포트 크기에 맞춰 늘어난 경우 범위가 0에 가깝게 나옵니다. 그러면
        오프셋이 항상 0 근처로 clamp되어 드래그할 때마다 콘텐츠가 최상단으로 끌려갑니다.
        **`horizontalScroller.highValue`/`verticalScroller.highValue`(ScrollView 자신이 쓰는
        범위)를 그대로 사용**하도록 고쳤습니다.
     2. 경계 초과분만 따로 `translate`에 썼는데, ScrollView도 같은 요소의 translate를
        `-scrollOffset`으로 갱신하므로 서로 덮어씁니다. **스크롤분과 초과분을 합친 절대값을 한 번에
        쓰도록** 고쳤습니다(상대값을 더하는 방식은 `scrollOffset`이 그대로일 때 매 프레임
        누적되므로 사용하지 않습니다).
        - 이 과정에서 잠시 `contentContainer.transform.position`으로 바꿨다가 되돌렸습니다.
          `VisualElement.transform`과 `ITransform.position`은 **deprecated**이며 Unity가
          `style.translate`를 쓰라고 안내합니다(즉 둘은 별개 시스템이 아니라 같은 값입니다).
          "서로 다른 API가 경합한다"는 초기 진단은 틀렸고, 실제 원인은 위처럼 **부분값만 쓴 것**과
          1번의 스크롤 범위 오산입니다.
   - **미검증**: "Device Simulator에서는 마우스가 터치로 변환되니 기본 ScrollView의 드래그·고무줄이
     동작할 것"이라는 예상은 사용자 확인 결과 **사실이 아니었습니다**. Input System의 Touch
     Simulation을 별도로 켜야 하는지, 실제 단말에서는 어떤지는 확인하지 않았습니다. 본 확장은 이
     경로에 의존하지 않지만, 실제 단말에서 기본 ScrollView 동작과 본 확장이 **중복 적용되지
     않는지**는 확인이 필요합니다.
   - Sample `HomeView.uxml`의 Grid 영역을 `UIScrollView`로 교체해 검증할 수 있습니다.
   - **드래그 물리 기본값**(2026-08-18 시점 우려 → 2026-09-01 해소, 아래 P2-03 완료 노트 참고).
     당시에는 아래 값들이 실제 조작감을 보지 못한 채 정한 초기값이라 재조정이 필요할 것으로 봤으나,
     2026-09-01 사용자 실측으로 현재 기본값과 조작감이 확인됐습니다. `UICarousel`의
     `Elasticity`/`SnapSpeed` 재검토도 같은 확인에 포함됩니다.

     | 속성 | 현재 기본값 | 참고(uGUI `ScrollRect`) |
     | --- | --- | --- |
     | `DragElasticity` | `0.3` | Elasticity `0.1` |
     | `DragSpringSpeed` | `0.2` | (uGUI는 계수 방식이 달라 직접 대응 없음) |
     | `DragDecelerationRate` | `0.135` | Deceleration Rate `0.135` (동일) |
     | `DragThreshold` | `10` | EventSystem Drag Threshold `10` (동일) |

     `DragDecelerationRate`와 `DragThreshold`는 uGUI 기본값에 맞춰 두었고, `DragElasticity`와
     `DragSpringSpeed`는 2026-08-18 시점엔 재조정이 필요할 것으로 봤으나 2026-09-01 사용자
     실측으로 현재 기본값의 조작감이 확인돼 안정화된 값으로 유지합니다.
   - **물리는 축별로 독립 계산해야 합니다**(2026-08-18, 사용자 제보 "Vertical과 Horizontal이 함께
     적용되면 관성이 안 먹는다"). 초기 구현은 `overshoot.sqrMagnitude`/`velocity.sqrMagnitude`로
     **두 축을 합쳐** 분기해서, 한 축만 경계를 벗어나도 스프링백 분기를 타며
     `_velocity = Vector2.zero`로 **두 축의 관성을 모두 없앴습니다.** 특히
     `VerticalAndHorizontal`이면서 콘텐츠가 한 축으로는 넘치지 않으면(그 축의 스크롤 범위가 0이라
     조금만 끌려도 항상 경계 밖) 반대 축 관성이 아예 동작하지 않습니다. `StepAxis(ref offset,
     ref velocity, clamped, dt)`로 분리해 축마다 경계 복귀·관성 감속을 따로 돌리고, 두 축 모두
     멈췄을 때만 스케줄러를 정지합니다.
   - **애니메이션 중 외부 조작이 먹지 않거나 튀던 문제**(2026-08-18, 사용자 제보 2건). 탄성 복귀·
     관성이 도는 동안에는 `Tick()`이 매 프레임 `scrollOffset`을 `_virtualOffset` 기준으로
     되돌립니다. 그래서 그 사이에 **휠**을 굴리거나 **스크롤바**를 끌면 ScrollView가 옮긴 위치가
     곧바로 덮어써져, 휠은 무시된 것처럼 보이고 스크롤바는 튑니다. **두 경우 모두 같은 원인**이며
     `CancelAnimation()`으로 공통 처리합니다 — 애니메이션을 멈추고 `_virtualOffset`을 실제
     `scrollOffset`으로 **재동기화**한 뒤 남은 초과분 translate를 걷어냅니다. 재동기화를 빼면 이전
     드래그의 낡은 값으로 되돌아갑니다.
     - 휠: `WheelEvent`를 `TrickleDown`으로 받아 ScrollView가 처리하기 전에 취소합니다.
     - 스크롤바: 스크롤바도 ScrollView의 자식이므로 그 위에서 시작한 포인터가 임계값을 넘으면
       우리 매니퓰레이터가 포인터를 가로채 조작을 끊습니다. `PointerDown`의 `evt.target`이 두
       `Scroller` 안에 있으면 **드래그를 시작하지 않되 `CancelAnimation()`은 호출**합니다.
       처음에는 취소 없이 `return`만 해서 위 "튀는" 증상이 남아 있었습니다.
     - **남은 구멍**: 키보드 내비게이션이나 코드에서 `ScrollTo()`로 위치를 바꾸는 경우는 아직
       취소되지 않습니다. 필요해지면 `Tick()`에서 `scrollOffset`이 마지막으로 우리가 쓴 값과
       다른지 비교해 일괄 처리할 수 있지만, ScrollView의 픽셀 그리드 반올림 때문에 허용 오차를
       둬야 하며 잘못 잡으면 탄성이 매번 취소되므로 실측이 필요합니다.
   - **해결 — 스크롤바 트랙 클릭 위치가 부정확함**(2026-08-19).
     스크롤바의 위치표시기(dragger) **바깥 트랙**을 클릭하면 이동하는 위치가 정확하지 않습니다.
     - **우리 버그가 아니라 UI Toolkit 기본 동작으로 확정했습니다.** 코드상 트랙 클릭 경로에서 우리가 하는 일은
       `IsWithinScrollers` → `CancelAnimation()` → `return`뿐이고, 애니메이션이 돌지 않는 상태의
       `CancelAnimation()`은 현재 위치를 그대로 다시 쓰는 no-op이라 위치에 영향이 없어야 합니다.
     - Unity 6000.0 공식 Slider 문서상 `page-size > 0`이면 트랙 클릭 시 포인터 방향으로 정해진
       페이지 양만 이동하고, `page-size = 0`일 때 포인터 위치로 바로 이동합니다. Unity 6000.5
       UXML 스키마에서 `ScrollView.vertical-page-size` 기본값은 `-1`(자동 페이지 크기)입니다.
     - Sample의 `UIScrollView`에 `vertical-page-size="0"`을 지정해 사용자 기대대로 클릭 위치로
       바로 이동하게 했습니다. `UIScrollView` 자체의 기본값은 Unity `ScrollView` 계약을 보존합니다.
   - **드래그 중 프레임 드랍 — 에디터 한정으로 확정**(2026-08-18). 사용자가 Statistics로 프레임
     드랍을 확인했으나, **Development Build에서 측정한 결과 실제 프레임 드랍이 없었습니다.**
     Profiler에서도 UI Toolkit `UpdatePanels`/`RenderPanels`가 각각 0.14ms/0.08ms로 예산(16.6ms)
     대비 무시할 수준이었고, 지배적인 것은 `EditorLoop`(에디터 자체 오버헤드)였습니다. **런타임
     문제가 아니므로 추가 최적화는 하지 않습니다.** 아래 세 가지는 그 과정에서 발견해 적용한
     낭비 제거이며, 그 자체로는 유효합니다:
     1. `OnPointerMove`마다 `Apply()`를 호출하던 것을 값만 갱신하고 **Tick에서 프레임당 한 번만**
        반영하도록 바꿨습니다. 포인터 이동 이벤트는 한 프레임에 여러 번 올 수 있어 그만큼
        ScrollView 갱신이 중복됐습니다.
     2. 탄성 구간에서는 `clamped`가 경계에 고정이라 같은 값인데도 매번 `scrollOffset`에
        재기록해 ScrollView가 스크롤러까지 갱신했습니다. **값이 바뀔 때만 쓰도록** 했습니다.
     3. `UIGrid.Reflow()`가 `GeometryChangedEvent`마다 폭이 그대로여도 모든 자식의 style을 다시
        썼습니다. style 쓰기는 레이아웃을 dirty로 만들어 다시 `GeometryChangedEvent`를 부를 수
        있어, 스크롤처럼 잦은 레이아웃 갱신에서 매번 전체 재작성이 일어납니다. **폭과 아이템 수가
        같으면 건너뛰는 캐시**를 넣었습니다. 대신 `ColumnCount` 등 `[UxmlAttribute]` 속성 setter는
        캐시를 무효화하고 즉시 다시 배치해, UI Builder에서 값을 바꿨을 때 바로 반영됩니다.
     - **Development Player 정량 A/B 추가 확인**(2026-08-19, macOS, 1280x720, 동일 Sample):
       무조작 481프레임과 ScrollView 상하 드래그 465프레임(시작·종료 각 10% 제외)을 raw Profiler로
       비교했습니다. 평균 프레임은 33.418ms → 33.332ms, P95는 33.558ms → 33.741ms,
       P99는 53.635ms → 34.206ms로 프레임 저하는 재현되지 않았습니다. 조작 비용 증가는
       `UIElementsUpdatePanels` +0.051ms/frame, `UIElements.UpdateLayout` +0.0019ms/frame,
       `UIElementsRepaintPanels` +0.0061ms/frame, GC Alloc 약 +63B/frame이었습니다. 입력 처리 비용은
       측정 가능하지만 현재 Sample에서는 프레임 예산을 흔드는 병목이 아닙니다. 이 결과는 macOS
       데스크톱 1회 측정이며, 아래 저사양 실제 기기·다수 항목·장시간 GC 검증을 대체하지 않습니다.
   - **관련 Sample 레이아웃 결함**(2026-08-18, 사용자 제보 "Device Simulator는 정상인데 Game
     View에서 UICarousel이 깨진다"): `.carousel-host`가 `height: 150px`만 있고 `flex-shrink`가
     기본값 1이라, 화면이 짧으면 부모 flex가 이 영역을 눌러 높이가 0에 가까워집니다. `UICarousel`은
     아이템 크기를 **자기 높이 기준**으로 계산하므로 아이템이 전부 한 점에 겹쳤습니다. 세로가 긴
     Device Simulator에서는 눌릴 일이 없어 정상으로 보였습니다. `.carousel-host`/`.button-row`에
     `flex-shrink: 0`을 주고, `UICarousel.Reflow()`에도 높이가 0 이하면 기존 크기를 유지하는
     방어를 추가했습니다.
   - **2026-09-01 사용자 확인**: 성능, 조작감, 탄성 회복과 관성 감속 파라미터를 포함한 현재
     `UIScrollView` 동작 검증은 완료됐습니다. 이전의 "다음 안정화 세션" 및 미검증 표기는 현재
     상태를 대체하지 않는 조사 이력입니다.
   - 탄성 비활성화/하드 클램프처럼 현재 공개 API에 없는 기능은 안정화 누락이 아니라 추후 선택적
     확장으로 분류합니다.
7. **P3-01 — Navigation·Transition 확장 (후순위)**
   - 화면 전환, history, modal, animation은 다음 확장 세션에서 Core 관리자와 분리된 선택 계층으로
     검토합니다. 현재 안정화 완료 조건에는 포함하지 않습니다.
8. **P3-03 — `UIScrollView` 추가 옵션 (후순위)**
   - 탄성 비활성화(하드 클램프, uGUI `ScrollRect.movementType`의 `Clamped` 대응) 등 현재 공개
     API에 없는 스크롤 드래그 옵션. 안정화 누락이 아니라 선택적 확장이며, Navigation·Transition과는
     별개의 ScrollView 전용 항목입니다. 현재 완료 조건에는 포함하지 않습니다.

## 완료 — UIStackManager/UIChannel 구조 재설계 (2026-08-21)

작업 순서 지시대로 진행해 다음을 완료했습니다.

| 스펙 항목 | 결과 |
| --- | --- |
| `UIChannel`에서 `UICatalog` 분리 | 완료 |
| Lifecycle 정리 (`Awake`=구성 / `OnEnable`=구독 / `OnDisable`=해제) | 완료 |
| `Initialize()` 대신 `SetChannel()` + `RebuildCatalog()` 분리 | 완료 |
| `_uiPool` → `_screenRegistry` 개명 | 완료 |
| `UIViewFactory` 분리 | 완료 |
| `UIStackController` 분리 (순수 C#) | 완료 |
| `IUIRequester` 도입 | 완료 |
| `IUIEventSource` 도입 | **하지 않음** (아래 판단 참고) |
| 테스트 보강 | `UIStackControllerTests` 9개 추가, PlayMode 15/15 |

**`IUIRequester`를 도입한 근거**: `UIView.Channel`이 `UIChannel` 전체였을 때 파생 View가
`ScreenOpened` 같은 Notification까지 구독할 수 있었고, 이는 View가 다른 View의 상태를 감지하는
암묵적 결합 경로였습니다. 인터페이스 비용은 메서드 3개뿐이고 기존 Sample 코드
(`Channel.RequestOpen<PopupView>`)는 그대로 동작합니다. 구현 교체가 목적이 아니라 **접근 범위
제한**이 목적이므로 "구현체가 하나뿐이면 인터페이스를 만들지 않는다"는 원칙과 충돌하지 않습니다.

**`IUIEventSource`를 도입하지 않은 근거**: Notification 구독자는 게임 코드이고 이들은 `UIChannel`
에셋을 Inspector에서 직접 참조합니다. 인터페이스로 감싸도 참조 타입이 구체 에셋이라 얻는 것이
없어 클래스 수만 늘어납니다.

Sample도 복구했습니다. `UICatalog.asset`을 추가하고 Scene의 `UIStackManager.catalog`에 연결했으며,
`UIChannel.asset`에 남아 있던 화면 목록은 제거했습니다. Catalog 항목 3개(Screen=HomeView,
Popup=PopupView, System=LoadingView)와 Scene 참조가 Unity에서 실제로 해석되는 것을 확인했습니다.

아키텍처 문서화까지 마쳐 이 재설계 작업은 완료입니다. 클래스별 책임, 의존 규칙, 수명, 개선점,
확장 지점과 도입하지 않은 항목의 근거는 하네스의 `architecture/ui-stack-architecture.md`에
기록했습니다.

### 원본 지시 스펙 (2026-08-20)

사용자가 2026-08-20에 상세 스펙을 전달했습니다. **GridTileSystem 작업을 완료한 뒤** 이 패키지로
돌아와 검토·착수합니다. ADR-0008의 핵심 원칙(`UIView`와 `UIStackManager`가 서로 직접 참조하지
않고 `UIChannel`을 경유)은 그대로 유지하고, 전체 구조를 갈아엎지 않으며 기존 Public API와 동작을
보존한 채 책임만 더 명확히 나눕니다. 요지:

- **`UIChannel`에서 `UICatalog` 분리.** 지금 `UIChannel`이 "정적 UI Configuration(`List<UIScreenEntry>`)"과
  "Runtime Message Channel(Open/Close Request·Notification)" 두 책임을 같이 갖고 있습니다.
  `UICatalog : ScriptableObject`(`IReadOnlyList<UIScreenEntry> Entries`)를 새로 만들어 전자를
  넘기고, `UIChannel`은 Request/Notification 채널 역할만 남깁니다.
- **`UIChannel` 역할 인터페이스 분리 검토.** `UIView`가 `UIChannel` 구현체 전체가 아니라 필요한
  최소 인터페이스만 보도록 `IUIRequester`(`RequestOpen<T>`/`RequestClose`/`RequestCloseAll`)와,
  필요하면 `IUIEventSource`(Notification 쪽)로 나누는 것을 검토합니다. 인터페이스 분리가 복잡도만
  늘리면 무리하게 세분화하지 않습니다.
- **`UIStackManager` 책임 축소 후보**(전부 만들 필요는 없음, SRP상 실제 분리 가치가 있을 때만):
  - `UIViewFactory` — UXML Instantiate → `UIView` 검증·초기화 → 생성 결과(예:
    `UIViewInstance` record struct)를 반환. Stack 관리나 Layer 활성 상태는 여기 넣지 않습니다.
  - `UIStackController` — Layer별 Stack 상태(Open/Close/CloseAll/ordering)를 가능하면
    `MonoBehaviour`가 아닌 순수 C# 객체로 분리. `BringToFront()`/`SetVisible()` 같은 실제 UI
    Toolkit 조작과 순수 Stack 상태 변경의 경계를 분석해서 나눕니다.
  - `UIRegistry` — 규모가 작으면 Manager 내부에 남겨도 됩니다.
  - `UIRoot`/`UIStackManager` 자신은 Unity Lifecycle + `UIDocument` + `UICatalog` + `UIChannel`을
    연결하는 조합 루트로 남습니다.
- **네이밍 정정.** `Dictionary<Type, UIView> _uiPool`은 실제 Object Pool(acquire/release)이
  아니므로 `_screens`/`_registry`/`_screenRegistry` 등 의미에 맞는 이름으로 바꿉니다(추후 진짜
  Pooling을 추가할 때 용어 충돌 방지).
- **Lifecycle 정리.** 현재 `Awake→BuildCatalog`, `OnEnable→Subscribe`,
  `Initialize→Unsubscribe+ClearCatalog+BuildCatalog+Subscribe`로 초기화 경로가 섞여 있습니다.
  `Awake`=런타임 구성, `OnEnable`=구독, `OnDisable`=해제 원칙으로 정리하고, 런타임에 `UIChannel`을
  바꿔야 하면 `Initialize()`가 전부 다시 하는 대신 `SetChannel(UIChannel newChannel)`처럼 명시적
  의존성 교체 API를 씁니다. Catalog 재구축과 Channel 교체는 별개 책임으로 다룹니다.
- **`ScriptableObject` 역할 경계.** `UICatalog.asset`=정적 설정, `UIChannel.asset`=런타임
  통신/Scope 핸들로 명확히 나눕니다. `UIChannel`이 Audio/Save/Network/Player 같은 UI 외부
  시스템까지 노출하는 싱글톤 대체 Service Locator가 되면 안 됩니다 — UI Scope 내부 통신만
  담당합니다. "UI Scope"(`UIRoot`+`UICatalog`+`UIChannel`+`UIViewFactory`+`UIRegistry`+
  `UIStackController`) 개념은 있지만, Screen/Popup/Overlay Layer용 하위 Scope 시스템처럼 지금
  당장 필요하지 않은 확장은 미리 만들지 않습니다.
- **금지 사항(ADR-0008 유지).** `UIView → UIStackManager` 또는 `UIStackManager → 특정 UIView
  구현체` 직접 참조 금지. `UIView → IUIRequester → UIChannel → UI Runtime` 간접 통신만 허용.
- **테스트 가능성.** UI 등록, 중복 타입 등록, Layer Stack ordering, Open/Close/CloseAll, 이미
  닫힌/다른 Layer의 UIView 처리, Channel 요청 전달은 가능한 한 Scene 없이 테스트합니다. UXML
  Instantiate·실제 `VisualElement` 동작처럼 Unity Runtime이 꼭 필요한 부분까지 억지로 순수 C#화
  하지 않습니다.
- **과도한 추상화 금지.** 클래스/인터페이스 수를 늘리는 게 목표가 아닙니다 — 명확한 책임이 있고
  독립적으로 테스트·교체할 실질적 가치가 있을 때만 분리합니다. 구현체가 하나뿐이라는 이유만으로
  인터페이스를 만들지 않습니다.
- **기존 동작 보존 목록**(리팩터 중 변경 금지): `RequestOpen<T>()`, `UIView` Close, `CloseAll`,
  Layer별 Stack, 동일 Screen 재오픈 시 Stack 최상단 이동, `BringToFront()`, `UIView` Visibility
  관리, `ScreenOpened`/`ScreenClosed`/`AllScreensClosed`, 중복 `UIView` 타입 등록 방지, 잘못된
  UXML Layout 검증, "하나의 Layout에 정확히 하나의 UIView" 정책.
- **작업 순서 지시**: 현재 코드·테스트·Public API 전체 분석 → `UICatalog` 분리 → Lifecycle 정리 →
  `_uiPool` 개명 → `UIViewFactory` 분리 여부 분석 후 구현 → `UIStackController` 분리 여부 분석 후
  구현 → `IUIRequester` 등 인터페이스 도입 필요성 검토 → 기존 테스트 수정 → 테스트 보강 → 전체
  테스트 실행 → Public API·기존 동작 회귀 확인 → 변경된 아키텍처(클래스별 책임, 의존 관계,
  개선점, 확장 가능성) 문서화. 한 번에 전체 구조를 바꾸지 않고 이 순서를 따르며, 각 단계에서
  기존 설계보다 실제로 책임·의존성이 명확해질 때만 변경합니다.

## 백로그

- `DragAndDropEvent`의 UI Toolkit 재설계 여부(길게 눌러 복제 이미지를 드래그하는 특수 동작). 필요성
  자체가 불확실해 재요청이 있을 때 착수합니다.

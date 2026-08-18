# 변경 기록

## [Unreleased]

- **(Breaking, `ADR-0008`)** `UIManager`/`BaseUI`(uGUI `Canvas`/`GraphicRaycaster` 기반)를 제거하고
  `UIStackManager`/`UIView`(`UIDocument`/`VisualElement` 기반)로 전면 재설계했습니다.
  - `UIStackManager`는 타입 기반 화면 조회·등록, 레이어(`UILayer`: `Screen`/`Popup`/`System`)별 열림
    스택 북키핑, 정렬(`BringToFront`)만 관리합니다. 레이어 간·레이어 내 입력 차단은 UI Toolkit의
    피킹에 맡기고 코드로 재구현하지 않습니다.
  - `UIStackManager`와 `UIView`는 서로를 직접 참조하지 않고 `UIChannel`(ScriptableObject)을 통해서만
    상호작용합니다(`channel.RequestOpen<T>()`/`RequestClose(view)`). `UIStackManager`는 일반
    `MonoBehaviour`이며 정적 `Instance` 접근을 제공하지 않습니다.
  - 화면 등록은 `UIChannel`의 카탈로그(`{Layer, VisualTreeAsset}` 엔트리)로 이루어집니다. UXML
    루트가 `[UxmlElement] UIView` 파생 타입이므로 Layout과 화면 타입이 구조적으로 일치합니다.
    기존 타입 문자열·전용 Drawer·`Type.GetType`·`TypeCache`·`Activator.CreateInstance`는
    제거했습니다(`ROADMAP.md` P1-01).
  - uGUI 시절의 결함(파괴된 UI가 활성 스택에 남아 `MissingReferenceException`, 등록되지 않은 UI를
    닫으면 무관한 UI까지 전부 닫힘, `baseUIList` 중복 타입 등록 시 예외)은 새 구현에서 구조적으로
    발생하지 않거나(`UIView`는 `VisualElement`라 Unity native 파괴 개념이 없음) 동일하게
    방어합니다.
  - `UIStackManager`는 처리 완료를 `UIChannel.ScreenOpened(Type)`/`ScreenClosed(Type)`/
    `AllScreensClosed()`로 발행합니다. 런타임 소비자는 Manager가 아니라 채널을 구독합니다.
    Inspector 영구 리스너는 선택적 `UIChannelListener`가 Type을 문자열로 변환해 private
    UnityEvent로 중계합니다.
  - 외부 코드가 채널 알림을 우회하지 않도록 `UIStackManager.GetUI`/`OpenUI`/`CloseUI`/
    `CloseAllUI` public API를 제거했습니다. 열기·닫기 요청은 `UIChannel`만 사용합니다.
- **(Breaking)** UI Toolkit이 이미 동등 기능을 기본 제공하는 uGUI 컴포넌트를 삭제했습니다(재구현
  아님): Trigger 20종(`RegisterCallback<TEvent>()`로 대체), `RangeAdjustmentSlider`(→
  `MinMaxSlider`), `ToggleSelector`(→ `RadioButtonGroup`), `PopupMouseEvent`(→ backdrop
  `RegisterCallback<PointerDownEvent>`), `TmpAutoEditorRefresh`(UI Toolkit 텍스트에는 해당 문제
  없음), `DragAndDropEvent`(네이티브 대응 없이 삭제, 필요해지면 재설계), `MessagePopup`/
  `WaitPopup`(옛 `BaseUI` 상속이라 삭제). 대응 Custom Editor(`RangeAdjustmentSliderEditor` 등)도
  함께 삭제했습니다.
- **(Breaking)** `HorizontalSelector`+`HorizontalEnumeratedItem`(같은 문제를 두 번 구현한 중복
  구현이었음)을 `Components/UICarousel`로 통합했습니다(`Draggable` 플래그로 드래그 지원 여부
  선택). `EnumeratedElements`(콘텐츠 너비 비율 기반 반응형 그리드)는 `Components/UIGrid`로
  대체했습니다. 둘 다 `[UxmlElement]` UI Toolkit 커스텀 컨트롤입니다. 드래그 물리(탄성/스냅 속도)
  수치는 초기값이라 Unity에서 조정이 필요할 수 있습니다.
- `com.unity.ugui`/`Unity.TextMeshPro` 의존성을 완전히 제거했습니다(`package.json`, Runtime/Editor
  asmdef).
- **(Breaking)** 매 프레임 `Screen.width`/`Screen.height`를 폴링하던 전역 Singleton
  `ResolutionObserver`를 제거했습니다. UI Toolkit에서는 대상 `VisualElement`의
  `GeometryChangedEvent`를 구독합니다. 이 제거로 `Jeomseon.Unity.Singleton` 의존성도
  완전히 제거했습니다.
- `Tests/Runtime/UIStackManagerPlayModeTests`를 추가했습니다(열기/닫기 가시성, 중복 등록 무시, 스택에
  없는 화면 닫기 무동작, `UIChannel` Request 이벤트 도달). `dotnet build` 기준 Runtime/Editor/Tests
  컴파일 오류 0개. Unity Test Runner 실행은 확인 대기 중입니다.
- `Samples~/BasicUsage`를 UI Toolkit 기반으로 다시 작성했습니다. 즉시 실행 가능한 Scene에서
  `UIChannel` 카탈로그, `Screen`/`Popup` 레이어, backdrop 입력 차단, `UICarousel` 버튼·드래그 선택,
  반응형 `UIGrid`를 함께 확인할 수 있습니다. Unity Editor에서의 시각·조작 검증은 대기 중입니다.

## [0.4.0] - 2026-08-13

- **(Breaking)** Runtime/Editor 네임스페이스를 패키지 규칙에 맞춰 `Jeomseon.Unity.UI[.Components]`와
  `Jeomseon.Unity.UI.Editor`로 변경했습니다. 이전 `Jeomseon.UI` 호환 별칭은 제공하지 않습니다.

## [0.3.3] - 2026-08-11

- 워크스페이스 명명 규칙에 맞춰 워크스페이스 전역 `[SerializeField] private` 필드를
  `_camelCase`에서 `camelCase`로 정리하고 기존 이름을 `[FormerlySerializedAs]`로 보존했습니다.
  관련 `HorizontalEnumeratedItemEditor`/`RangeAdjustmentSliderEditor`/`HorizontalSelectorEditor`의
  `FindProperty` 문자열도 함께 갱신했습니다. 리네이밍 과정에서 실제 결함 2건을 발견해 함께
  수정했습니다: `HorizontalEnumeratedItem.SetSelectedIndexWithOutNotify`가 매개변수와 필드 이름이
  같아지며 자기 대입(no-op)이 되던 문제, `PopupMouseEvent.AddUI`/`DeleteUI`가 매개변수 배열과
  필드를 혼동해 컴파일이 깨지던 문제. 공개 API 변경은 없으며 기존 Scene·Prefab의 직렬화된 값은
  그대로 유지됩니다.

## [0.3.2] - 2026-08-10

- `EditorToolkit`의 `IMGUIHelper`가 `EditorGUILayoutActions`로 이전되면서 4개 Editor 파일이
  참조하던 구 `Jeomseon.Editor.EditorGUIHelper`(존재하지 않는 타입)를 갱신했습니다. 이 개명
  시점부터 Editor 어셈블리가 컴파일되지 않던 상태였던 것을 복구했습니다. 공개 API 변경은 없습니다.
- `com.jeomseon.unity.editor-toolkit` 의존성을 0.4.0으로 올렸습니다(위 수정이 실제로 존재하는
  버전을 최소 요구사항으로 명시하기 위함).

## [0.3.0] - 2026-07-29

- 패키지 폴더 구조와 namespace를 일치시키고 Runtime·Editor·Samples 어셈블리의 `rootNamespace`를 정리했습니다.
- 전역 namespace에 있던 `ScrollTrigger`와 `UpdateSelectedTrigger`를 `Jeomseon.UI.Components`로 이동했습니다.

## [0.2.3] - 2026-07-29

- PointerClickTrigger 사용법을 확인하는 `Basic Usage` 샘플을 추가했습니다.

## [0.2.2] - 2026-07-29

- EditorToolkit 0.3.0을 사용하도록 의존성을 갱신했습니다.

## [0.2.1] - 2026-07-29

- Jeomseon Unity Attributes 0.2.0과 EditorToolkit 0.2.1을 사용하도록 의존성을 갱신했습니다.

## [0.2.0] - 2026-07-29

- 런타임 어셈블리의 EditorToolkit 참조를 제거하고 경량 Attributes 패키지를 참조하도록 변경했습니다.
- UI CustomEditor의 공통 Editor Helper 사용을 위해 Editor 어셈블리 참조는 유지했습니다.

## [Unreleased]

- TODO(api): 범용 컨트롤은 UI Toolkit의 Slider·ListView·Pointer 이벤트로 대체 가능한지 확인하고 런타임 uGUI가 필요한 부분만 유지합니다.
- 정적 이벤트와 전역 인스턴스의 Domain Reload 비활성화 호환성을 검토합니다.

## [0.1.0] - 2026-07-29

- JeomseonScriptPack의 관련 모듈을 독립 UPM 패키지로 분리했습니다.


## [0.3.1] - 2026-08-05

- Unity 6000.5.7f1을 최소 지원 버전으로 상향했습니다.

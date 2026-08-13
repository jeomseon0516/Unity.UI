# 변경 기록

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

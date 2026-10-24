# Jeomseon Unity UI

한국어 | [English](README.en.md)

UI Toolkit 화면 생성·등록·레이어별 스택과 선택적 Navigation/Transition을 제공합니다.

## 빠르게 시작하기

UICatalog에 UIView UXML과 layer 구성 → UIStackManager에 Catalog/Channel 연결 → Channel로 열기·닫기 요청 → 알림을 Navigation/Transition이 소비. 구독과 transition 수명은 소비 코드와 샘플을 따릅니다.

## 계약과 주의사항

Core는 특정 View 구현을 직접 참조하지 않습니다. Screen/Popup/System은 독립 레이어이며 입력 차단은 UI Toolkit picking/backdrop 구성으로 처리합니다. Transition·Navigation은 namespace로 분리된 같은 어셈블리입니다.

## 요구 사항

- Unity `6000.6.0f1`
- 패키지: `com.jeomseon.unity.ui` `0.7.0`

## 설치

프로젝트 `Packages/manifest.json`의 기존 dependencies와 registry를 유지하면서 다음 항목을 병합합니다.

```json
{
  "scopedRegistries": [
    {
      "name": "OpenUPM",
      "url": "https://package.openupm.com",
      "scopes": [
        "com.jeomseon"
      ]
    }
  ],
  "dependencies": {
    "com.jeomseon.unity.ui": "0.7.0"
  }
}
```

또는 Package Manager의 **Install package from git URL**을 사용합니다:

```text
https://github.com/jeomseon0516/Unity.UI.git#v0.7.0
```

## API 사용과 상세 설명

### 구성 (네임스페이스)

한 패키지·한 asmdef이며 하위 계층은 네임스페이스로만 나뉩니다. 서로 독립 동작하고, 의존 방향은
`Jeomseon.Unity.UI`(Core) ← `.Transition` / `.Navigation` 한 방향만 허용합니다.

| 네임스페이스 | 역할 |
| --- | --- |
| `Jeomseon.Unity.UI` | 화면 스택 Core — `UIStackManager` / `UIStackController` / `UIView` / `UILayer` |
| `Jeomseon.Unity.UI.Channels` | `UIChannel`(요청·알림) / `UICatalog`(화면 목록) / `IUIRequester` |
| `Jeomseon.Unity.UI.Components` | 재사용 `VisualElement` 컨트롤 — `UIScrollView` / `UICarousel` / `UIGrid` |
| `Jeomseon.Unity.UI.Transition` | 화면 enter/exit 연출 — `ITransition`, `Fade`/`Slide`/`Scale`, `ScreenTransitions` |
| `Jeomseon.Unity.UI.Navigation` | 뒤로가기 history — `NavigationStack`, `BackNavigationBinder` |

`.Transition`/`.Navigation`은 `using` 하지 않으면 코드에 들어오지 않습니다. 둘을 함께 쓸 때
exit 연출 연결은 소비자 코드 한 줄입니다:
`new NavigationStack(channel, v => screenTransitions.Default.PlayExit(v, TransitionContext.Exit()))`.


런타임 어셈블리는 범용 Attribute 선언만 참조합니다. UI 전용 CustomEditor는 공통 Editor Helper를 사용하므로 EditorToolkit의 Editor 어셈블리를 참조합니다.

## 샘플 실행

**Package Manager > Samples**에서 예제를 Import합니다. 포함된 Scene 또는 Editor 도구를 열고 필요한 Setup 메뉴를 먼저 실행합니다.

- [Basic Usage](Samples~/BasicUsage/README.md)
  - [UIBasicUsageSample](Samples~/BasicUsage/UIBasicUsageSample.unity)
- [Navigation Transition Usage](Samples~/NavigationTransitionUsage/README.md)
  - [NavigationTransitionSample](Samples~/NavigationTransitionUsage/NavigationTransitionSample.unity)

## 테스트

[테스트 코드](Tests)

manifest의 `testables`에 패키지 ID를 추가하고 Unity Test Runner의 관련 EditMode/PlayMode 어셈블리를 실행합니다.

## 추가 문서

[전체 코드 지도와 패키지 구성](Documentation~/Overview.md)
- [Changelog](CHANGELOG.md)

## 라이선스

[License](LICENSE.md)

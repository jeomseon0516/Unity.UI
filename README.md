# Jeomseon Unity UI

UI Toolkit(`UIDocument`/`VisualElement`) 기반 화면 스택 매니저와 재사용 컨트롤.

## OpenUPM으로 설치

프로젝트의 `Packages/manifest.json`에 OpenUPM scoped registry를 한 번 등록합니다.

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

## Git URL로 설치

Unity Package Manager의 `Install package from git URL`에 다음 주소를 사용합니다.

```text
https://github.com/jeomseon0516/Unity.UI.git#v0.7.0
```

## 구성 (네임스페이스)

한 패키지·한 asmdef이며 하위 계층은 네임스페이스로만 나뉩니다. 서로 독립 동작하고, 의존 방향은
`Jeomseon.Unity.UI`(Core) ← `.Transition` / `.Navigation` 한 방향만 허용합니다(하네스 `ADR-0009`).

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

## 리팩토링 방침

Unity가 제공하는 동등 기능과 비교해 대체 가능한 코드는 소스의 한글 TODO 주석과 CHANGELOG의 Unreleased 항목에서 추적합니다.

런타임 어셈블리는 범용 Attribute 선언만 참조합니다. UI 전용 CustomEditor는 공통 Editor Helper를 사용하므로 EditorToolkit의 Editor 어셈블리를 참조합니다.

# Navigation + Transition Usage

`NavigationTransitionSample.unity`를 열고 Play Mode를 시작합니다.

한 패키지·한 asmdef이며 `Jeomseon.Unity.UI.Navigation`과 `Jeomseon.Unity.UI.Transition`은
네임스페이스로만 나뉩니다. 이 Sample은 둘을 함께 씁니다.

## 확인 절차

1. `Menu` 화면이 오른쪽에서 슬라이드-인되며 자동으로 열립니다(`NavigationStack.Push<MenuScreen>`).
2. **Open Detail** → `Detail`이 오른쪽에서 슬라이드-인, 스택 깊이 2. Console에 `[Nav] Push: MenuScreen -> DetailScreen (depth 2)`.
3. **Open Settings** → `Settings`가 슬라이드-인, 깊이 3.
4. **Back** 버튼 또는 **ESC / 게임패드 B**(`BackNavigationBinder` → `NavigationCancelEvent`) → 최상단 화면이 왼쪽으로 슬라이드-아웃되고 그 아래 화면이 드러납니다. 깊이가 1이면 `Back()`은 아무 것도 하지 않습니다.
5. `Settings`에서 **Pop to Menu**(`NavigationStack.PopTo<MenuScreen>`) → `Detail`과 `Settings`가 닫히고 `Menu`로 돌아갑니다.
6. Console의 `[Nav] ...` 로그로 `From -> To`와 스택 깊이를 확인합니다.

## 구성

- enter 연출: `ScreenTransitions(channel, new SlideTransition(SlideEdge.Right))` — `ScreenOpened`를
  구독해 표시 즉시 자동 재생.
- exit 연출: `new NavigationStack(channel, view => backOut.PlayExit(view, TransitionContext.Exit()))`
  — 닫히는 화면을 왼쪽으로 밀어낸 뒤 `RequestClose`. Transition 타입을 직접 참조하지 않고
  델리게이트 한 줄로 엮습니다.
- 화면 이동은 항상 `Nav`를 경유합니다. `channel.RequestOpen`을 직접 부르면 history에 잡히지 않습니다.

Scene에는 UI 외에 `Main Camera`(Solid Color, UI Toolkit 전용 Scene도 프레임버퍼 클리어에 필요)와
`EventSystem`(+ 입력 모듈, 포인터·Navigation 입력 전달에 필요)이 있습니다.

## Scene 자산 재생성

Scene과 `PanelSettings`/`UICatalog`/`UIChannel` 자산은
`Editor/NavigationTransitionSampleBuilder.cs`가 코드로 만듭니다. 메뉴
`Jeomseon/Tool/UI/Build Navigation Transition Sample Scene` 또는 배치모드
`-executeMethod Jeomseon.Samples.UI.Editor.NavigationTransitionSampleBuilder.Build`.

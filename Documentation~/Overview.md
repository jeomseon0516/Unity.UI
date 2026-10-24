# Jeomseon Unity UI

[English](Overview.en.md) · [설치](../README.md)

## 목적

UI Toolkit 화면 생성·등록·레이어별 스택과 선택적 Navigation/Transition을 제공합니다.

## 사용 흐름

UICatalog에 UIView UXML과 layer 구성 → UIStackManager에 Catalog/Channel 연결 → Channel로 열기·닫기 요청 → 알림을 Navigation/Transition이 소비. 구독과 transition 수명은 소비 코드와 샘플을 따릅니다.

## 계약과 제한

Core는 특정 View 구현을 직접 참조하지 않습니다. Screen/Popup/System은 독립 레이어이며 입력 차단은 UI Toolkit picking/backdrop 구성으로 처리합니다. Transition·Navigation은 namespace로 분리된 같은 어셈블리입니다.

## 패키지 요구 사항

- Package ID: `com.jeomseon.unity.ui`
- Version: `0.7.0`
- Unity: `6000.6` / `0f1`

## 샘플과 검증

Package Manager > Samples에서 다음 예제를 Import하세요. 포함된 Scene 또는 Editor 도구를 열고 샘플 README의 Setup 절차를 먼저 수행합니다.

- [Basic Usage](../Samples~/BasicUsage/README.md)
  - [UIBasicUsageSample](../Samples~/BasicUsage/UIBasicUsageSample.unity)
- [Navigation Transition Usage](../Samples~/NavigationTransitionUsage/README.md)
  - [NavigationTransitionSample](../Samples~/NavigationTransitionUsage/NavigationTransitionSample.unity)

[테스트 코드](../Tests)

소비 프로젝트 manifest의 `testables`에 패키지 ID를 추가하고 Unity Test Runner의 관련 어셈블리를 실행합니다. 위 링크는 테스트 위치이며 이번 문서 수정으로 새 테스트 실행을 주장하지 않습니다.

## 코드 지도

- [IUIRequester](../Runtime/Channels/IUIRequester.cs)
- [UICatalog](../Runtime/Channels/UICatalog.cs)
- [UIChannel](../Runtime/Channels/UIChannel.cs)
- [UIChannelListener](../Runtime/Channels/UIChannelListener.cs)
- [UIScreenEntry](../Runtime/Channels/UIScreenEntry.cs)
- [ScrollDragManipulator](../Runtime/Components/ScrollDragManipulator.cs)
- [UICarousel](../Runtime/Components/UICarousel.cs)
- [UIGrid](../Runtime/Components/UIGrid.cs)
- [UIScrollView](../Runtime/Components/UIScrollView.cs)
- [BackNavigationBinder](../Runtime/Navigation/BackNavigationBinder.cs)
- [NavigationAction, NavigationChange](../Runtime/Navigation/NavigationChange.cs)
- [NavigationStack](../Runtime/Navigation/NavigationStack.cs)
- [FadeTransition](../Runtime/Transition/FadeTransition.cs)
- [ITransition](../Runtime/Transition/ITransition.cs)
- [NoTransition](../Runtime/Transition/NoTransition.cs)
- [OpenScreenTracker](../Runtime/Transition/OpenScreenTracker.cs)
- [ScaleTransition](../Runtime/Transition/ScaleTransition.cs)
- [ScreenTransitions](../Runtime/Transition/ScreenTransitions.cs)
- [SlideTransition](../Runtime/Transition/SlideTransition.cs)
- [TransitionDirection, SlideEdge, TransitionContext](../Runtime/Transition/TransitionContext.cs)
- [UILayer](../Runtime/UILayer.cs)
- [UIStackManager](../Runtime/UIStackManager.cs)
- [UIView](../Runtime/UIView.cs)
- [UIStackManagerEditor](../Editor/UIStackManagerEditor.cs)

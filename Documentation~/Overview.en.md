# Jeomseon Unity UI

[한국어](Overview.md) · [Installation](../README.en.md)

## Purpose

Provides UI Toolkit view creation/registration/layer stacks and optional Navigation/Transition.

## Usage flow

Configure UIView UXML/layers in UICatalog, connect Catalog/Channel to UIStackManager, request open/close through Channel, and consume notifications in Navigation/Transition. Follow samples for subscription/transition lifetimes.

## Contracts and limitations

Core does not depend on concrete Views. Screen/Popup/System are independent layers; UI Toolkit picking/backdrops handle input blocking. Transition and Navigation are namespace layers in the same assembly.

## Package requirements

- Package ID: `com.jeomseon.unity.ui`
- Version: `0.7.0`
- Unity: `6000.6` / `0f1`

## Samples and verification

Import the following from Package Manager > Samples. Open the included Scene or Editor tool; follow any Setup step in its sample README.

- [Basic Usage](../Samples~/BasicUsage/README.md)
  - [UIBasicUsageSample](../Samples~/BasicUsage/UIBasicUsageSample.unity)
- [Navigation Transition Usage](../Samples~/NavigationTransitionUsage/README.md)
  - [NavigationTransitionSample](../Samples~/NavigationTransitionUsage/NavigationTransitionSample.unity)

[Test sources](../Tests)

Add the package ID to manifest `testables`, then run the relevant Unity Test Runner assemblies. These links describe available tests, not a new successful test run.

## Source map

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

using System;
using Jeomseon.Unity.UI;
using Jeomseon.Unity.UI.Channels;
using Jeomseon.Unity.UI.Navigation;
using Jeomseon.Unity.UI.Transition;
using UnityEngine;
using UnityEngine.UIElements;

namespace Jeomseon.Samples.UI
{
    /// <summary>
    /// <see cref="NavigationStack"/>(history/back)와 <see cref="ScreenTransitions"/>(enter/exit 연출)를
    /// 함께 씁니다. 화면 코드는 <c>Instance.Nav</c>로 이동을 요청합니다(<c>channel.RequestOpen</c>
    /// 직접 호출은 history에 잡히지 않으므로 — ADR-0009 §3).
    /// </summary>
    public sealed class NavigationTransitionSample : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
        [SerializeField] private UIChannel channel;

        public static NavigationTransitionSample Instance { get; private set; }

        public NavigationStack Nav { get; private set; }
        public ScreenTransitions Transitions { get; private set; }

        private BackNavigationBinder _backBinder;

        private void Awake() => Instance = this;

        private void OnEnable()
        {
            // enter는 ScreenTransitions가 ScreenOpened를 구독해 자동 재생합니다.
            Transitions = new ScreenTransitions(channel, new SlideTransition(SlideEdge.Right));

            // Back/Pop 시 닫히는 화면의 exit 연출을 델리게이트로 위임합니다(왼쪽으로 밀어냄).
            var backOut = new SlideTransition(SlideEdge.Left);
            Nav = new NavigationStack(channel, view => backOut.PlayExit(view, TransitionContext.Exit()));

            _backBinder = new BackNavigationBinder(document.rootVisualElement, Nav);

            Nav.Changed += change => Debug.Log(
                $"[Nav] {change.Action}: {change.From?.Name ?? "-"} -> {change.To?.Name} (depth {Nav.Depth})", this);
        }

        private void Start() => Nav.Push<MenuScreen>();

        private void OnDisable()
        {
            _backBinder?.Dispose();
            Nav?.Dispose();
            Transitions?.Dispose();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}

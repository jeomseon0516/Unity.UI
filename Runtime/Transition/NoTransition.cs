using UnityEngine;
using UnityEngine.UIElements;

namespace Jeomseon.Unity.UI.Transition
{
    /// <summary>연출 없이 즉시 완료되는 전략입니다. Transition 계층의 기본 no-op.</summary>
    public sealed class NoTransition : ITransition
    {
        public static readonly NoTransition Instance = new();

        public Awaitable PlayEnter(VisualElement view, TransitionContext context) => Completed();

        public Awaitable PlayExit(VisualElement view, TransitionContext context) => Completed();

        internal static Awaitable Completed()
        {
            var completion = new AwaitableCompletionSource();
            completion.SetResult();
            return completion.Awaitable;
        }
    }
}

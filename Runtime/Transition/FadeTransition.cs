using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Jeomseon.Unity.UI.Transition
{
    /// <summary>opacity를 0↔1로 트윈합니다. Transition 계층의 기본 전략.</summary>
    public sealed class FadeTransition : ITransition
    {
        private readonly int _durationMs;
        private readonly Func<float, float> _easing;

        public FadeTransition(int durationMs = 160, Func<float, float> easing = null)
        {
            _durationMs = durationMs;
            _easing = easing;
        }

        public Awaitable PlayEnter(VisualElement view, TransitionContext context)
            => Fade(view, 0f, 1f, context);

        public Awaitable PlayExit(VisualElement view, TransitionContext context)
            => Fade(view, 1f, 0f, context);

        private Awaitable Fade(VisualElement view, float from, float to, TransitionContext context)
        {
            int duration = context.DurationMs > 0 ? context.DurationMs : _durationMs;
            Func<float, float> easing = _easing ?? context.Easing;

            return ValueTween.Run(view, from, to, duration, easing,
                static (element, value) => element.style.opacity = value);
        }
    }
}

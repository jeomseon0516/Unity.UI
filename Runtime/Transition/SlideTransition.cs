using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Jeomseon.Unity.UI.Transition
{
    /// <summary>
    /// 화면을 지정한 가장자리 밖에서 밀어 넣고(enter) 다시 그 가장자리로 밀어 냅니다(exit).
    /// <c>style.translate</c>를 퍼센트로 트윈하므로 화면 크기와 무관합니다.
    /// </summary>
    public sealed class SlideTransition : ITransition
    {
        private readonly SlideEdge _edge;
        private readonly int _durationMs;
        private readonly Func<float, float> _easing;

        public SlideTransition(SlideEdge edge = SlideEdge.Right, int durationMs = 220, Func<float, float> easing = null)
        {
            _edge = edge;
            _durationMs = durationMs;
            _easing = easing;
        }

        public Awaitable PlayEnter(VisualElement view, TransitionContext context)
            => Slide(view, offscreenAtStart: true, context);

        public Awaitable PlayExit(VisualElement view, TransitionContext context)
            => Slide(view, offscreenAtStart: false, context);

        private Awaitable Slide(VisualElement view, bool offscreenAtStart, TransitionContext context)
        {
            (float offsetX, float offsetY) = _edge switch
            {
                SlideEdge.Left => (-100f, 0f),
                SlideEdge.Right => (100f, 0f),
                SlideEdge.Top => (0f, -100f),
                SlideEdge.Bottom => (0f, 100f),
                _ => (100f, 0f)
            };

            // amount: 1 = 완전히 화면 밖, 0 = 제자리.
            float from = offscreenAtStart ? 1f : 0f;
            float to = offscreenAtStart ? 0f : 1f;

            int duration = context.DurationMs > 0 ? context.DurationMs : _durationMs;
            Func<float, float> easing = _easing ?? context.Easing;

            return ValueTween.Run(view, from, to, duration, easing, (element, amount) =>
                element.style.translate = new StyleTranslate(new Translate(
                    Length.Percent(offsetX * amount),
                    Length.Percent(offsetY * amount))));
        }
    }
}

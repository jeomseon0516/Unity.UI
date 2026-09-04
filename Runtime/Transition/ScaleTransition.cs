using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Jeomseon.Unity.UI.Transition
{
    /// <summary>
    /// opacity와 scale을 함께 트윈합니다(enter: 살짝 작게→제자리, exit: 반대). 다이얼로그·팝업에 적합.
    /// </summary>
    public sealed class ScaleTransition : ITransition
    {
        private readonly float _fromScale;
        private readonly int _durationMs;
        private readonly Func<float, float> _easing;

        public ScaleTransition(float fromScale = 0.92f, int durationMs = 180, Func<float, float> easing = null)
        {
            _fromScale = fromScale;
            _durationMs = durationMs;
            _easing = easing;
        }

        public Awaitable PlayEnter(VisualElement view, TransitionContext context)
            => Animate(view, 0f, 1f, context);

        public Awaitable PlayExit(VisualElement view, TransitionContext context)
            => Animate(view, 1f, 0f, context);

        private Awaitable Animate(VisualElement view, float from, float to, TransitionContext context)
        {
            int duration = context.DurationMs > 0 ? context.DurationMs : _durationMs;
            Func<float, float> easing = _easing ?? context.Easing;

            return ValueTween.Run(view, from, to, duration, easing, (element, progress) =>
            {
                element.style.opacity = progress;
                float scale = Mathf.Lerp(_fromScale, 1f, progress);
                element.style.scale = new StyleScale(new Scale(new Vector3(scale, scale, 1f)));
            });
        }
    }
}

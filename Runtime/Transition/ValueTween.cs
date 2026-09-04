using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace Jeomseon.Unity.UI.Transition
{
    /// <summary>
    /// <see cref="IVisualElementScheduledItem"/>가 아니라 UI Toolkit의
    /// <c>experimental.animation</c>(<see cref="ValueAnimation{T}"/>)으로 float 값 하나를 트윈하고,
    /// 완료를 <see cref="Awaitable"/>로 돌려주는 내부 헬퍼입니다(ADR-0009 §2).
    /// </summary>
    internal static class ValueTween
    {
        public static Awaitable Run(
            VisualElement target,
            float from,
            float to,
            int durationMs,
            Func<float, float> easing,
            Action<VisualElement, float> apply)
        {
            // 초기 상태를 동기적으로 먼저 적용해 첫 프레임 깜빡임을 줄인다.
            apply(target, from);

            var completion = new AwaitableCompletionSource();

            if (durationMs <= 0 || target.panel == null)
            {
                apply(target, to);
                completion.SetResult();
                return completion.Awaitable;
            }

            easing ??= Easing.OutQuad;

            ValueAnimation<float> animation = null;

            void Finish()
            {
                target.UnregisterCallback<DetachFromPanelEvent>(OnDetach);
                completion.TrySetResult();
            }

            void OnDetach(DetachFromPanelEvent _)
            {
                animation?.Stop();
                apply(target, to);
                Finish();
            }

            target.RegisterCallback<DetachFromPanelEvent>(OnDetach);

            animation = target.experimental.animation
                .Start(from, to, durationMs, apply)
                .Ease(easing)
                .OnCompleted(Finish);

            return completion.Awaitable;
        }
    }
}

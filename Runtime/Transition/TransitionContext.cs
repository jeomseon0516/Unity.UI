using System;

namespace Jeomseon.Unity.UI.Transition
{
    public enum TransitionDirection
    {
        Enter,
        Exit
    }

    /// <summary>화면이 화면 밖으로 나가거나 들어오는 방향입니다.</summary>
    public enum SlideEdge
    {
        Left,
        Right,
        Top,
        Bottom
    }

    /// <summary>
    /// <see cref="ITransition"/> 호출에 넘기는 옵션입니다. 값이 기본값(<c>0</c> / <c>null</c>)이면
    /// 전략 구현의 자체 기본값을 씁니다.
    /// </summary>
    public readonly struct TransitionContext
    {
        public TransitionContext(TransitionDirection direction, int durationMs = 0, Func<float, float> easing = null)
        {
            Direction = direction;
            DurationMs = durationMs;
            Easing = easing;
        }

        public TransitionDirection Direction { get; }

        /// <summary>0이면 전략의 기본 지속시간을 사용합니다.</summary>
        public int DurationMs { get; }

        /// <summary>null이면 전략의 기본 easing을 사용합니다.</summary>
        public Func<float, float> Easing { get; }

        public static TransitionContext Enter(int durationMs = 0, Func<float, float> easing = null)
            => new(TransitionDirection.Enter, durationMs, easing);

        public static TransitionContext Exit(int durationMs = 0, Func<float, float> easing = null)
            => new(TransitionDirection.Exit, durationMs, easing);
    }
}

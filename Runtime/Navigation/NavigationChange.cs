using System;

namespace Jeomseon.Unity.UI.Navigation
{
    public enum NavigationAction
    {
        Push,
        Back,
        PopTo,
        Reset
    }

    /// <summary><see cref="NavigationStack.Changed"/>가 발행하는 이동 이벤트입니다.</summary>
    public readonly struct NavigationChange
    {
        public NavigationChange(NavigationAction action, Type from, Type to)
        {
            Action = action;
            From = from;
            To = to;
        }

        public NavigationAction Action { get; }

        /// <summary>이동 전 최상단 화면 타입입니다. 스택이 비어 있었으면 <c>null</c>.</summary>
        public Type From { get; }

        /// <summary>이동 후 최상단 화면 타입입니다.</summary>
        public Type To { get; }
    }
}

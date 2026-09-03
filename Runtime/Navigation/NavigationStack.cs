using System;
using System.Collections.Generic;
using Jeomseon.Unity.UI.Channels;
using UnityEngine;

namespace Jeomseon.Unity.UI.Navigation
{
    /// <summary>
    /// 사용자가 지나온 Screen 이동 경로(history)를 기록하고 <see cref="Back"/>/<see cref="PopTo{T}"/>/
    /// <see cref="ResetTo{T}"/>를 제공합니다. 순수 C#이며 Scene 없이 테스트할 수 있습니다.
    ///
    /// <para>Core의 <c>UIStackController</c>("레이어별 등록 상태")와 별개입니다. 이동은
    /// <see cref="UIChannel"/>의 <c>RequestOpen</c>/<c>RequestClose</c>를 <b>직접 호출</b>하며
    /// (ADR-0009 §3), 열린 <see cref="UIView"/> 인스턴스는 채널 알림 이벤트로 추적합니다.</para>
    ///
    /// <para>Push는 스택 위에 쌓는 동작이라 이전 화면을 닫지 않습니다(UI Toolkit picking이 트리
    /// 순서를 따르므로 위 화면이 아래를 덮습니다). <see cref="Back"/>은 최상단 화면만 닫고
    /// 그 아래 화면이 드러납니다.</para>
    ///
    /// <para><paramref name="exitAnimation"/>을 넘기면 화면을 닫기 전에 그 연출을 <c>await</c>합니다
    /// (연결이 없으면 즉시 닫힘). <see cref="Jeomseon.Unity.UI.Transition"/> 타입을 직접 참조하지
    /// 않도록 델리게이트로 받습니다 — 소비자가 <c>v => transition.PlayExit(v, ...)</c> 한 줄로
    /// 엮습니다.</para>
    /// </summary>
    public sealed class NavigationStack : IDisposable
    {
        private readonly struct Entry
        {
            public Entry(Type type, object args, Action open)
            {
                Type = type;
                Args = args;
                Open = open;
            }

            public Type Type { get; }
            public object Args { get; }
            public Action Open { get; }
        }

        private readonly UIChannel _channel;
        private readonly Func<UIView, Awaitable> _exitAnimation;
        private readonly List<Entry> _history = new();
        private readonly Dictionary<Type, UIView> _openViews = new();

        public NavigationStack(UIChannel channel, Func<UIView, Awaitable> exitAnimation = null)
        {
            _channel = channel ? channel : throw new ArgumentNullException(nameof(channel));
            _exitAnimation = exitAnimation;
            _channel.ScreenOpened += HandleOpened;
            _channel.ScreenClosed += HandleClosed;
            _channel.AllScreensClosed += HandleAllClosed;
        }

        public event Action<NavigationChange> Changed;

        /// <summary>현재 최상단 화면 타입입니다. 스택이 비어 있으면 <c>null</c>.</summary>
        public Type Current => _history.Count > 0 ? _history[^1].Type : null;

        /// <summary>현재 최상단 화면에 <see cref="Push{T}"/> 시 넘긴 인자입니다.</summary>
        public object CurrentArgs => _history.Count > 0 ? _history[^1].Args : null;

        public int Depth => _history.Count;

        public bool CanGoBack => _history.Count > 1;

        public void Push<T>(object args = null) where T : UIView
        {
            Type from = Current;
            PushCore(typeof(T), args, () => _channel.RequestOpen<T>());
            Raise(NavigationAction.Push, from, typeof(T));
        }

        /// <summary>최상단 화면을 닫고 직전 화면으로 돌아갑니다. 스택 깊이가 1 이하면 <c>false</c>.</summary>
        public bool Back()
        {
            if (!CanGoBack) return false;

            Type from = Current;
            _history.RemoveAt(_history.Count - 1);
            CloseByType(from);
            _history[^1].Open();
            Raise(NavigationAction.Back, from, Current);
            return true;
        }

        /// <summary>
        /// 스택에서 마지막으로 등장한 <typeparamref name="T"/>까지 pop합니다. <typeparamref name="T"/>가
        /// 없거나 이미 최상단이면 아무 것도 하지 않습니다.
        /// </summary>
        public void PopTo<T>() where T : UIView
        {
            int index = _history.FindLastIndex(entry => entry.Type == typeof(T));
            if (index < 0 || index == _history.Count - 1) return;

            Type from = Current;
            for (int i = _history.Count - 1; i > index; i--)
            {
                CloseByType(_history[i].Type);
                _history.RemoveAt(i);
            }

            _history[^1].Open();
            Raise(NavigationAction.PopTo, from, Current);
        }

        /// <summary>스택을 모두 비우고 <typeparamref name="T"/>를 새 루트로 엽니다.</summary>
        public void ResetTo<T>(object args = null) where T : UIView
        {
            Type from = Current;
            for (int i = _history.Count - 1; i >= 0; i--)
                CloseByType(_history[i].Type);
            _history.Clear();

            PushCore(typeof(T), args, () => _channel.RequestOpen<T>());
            Raise(NavigationAction.Reset, from, typeof(T));
        }

        private void PushCore(Type type, object args, Action open)
        {
            _history.Add(new Entry(type, args, open));
            open();
        }

        private void CloseByType(Type type)
        {
            if (!_openViews.TryGetValue(type, out UIView view) || view == null) return;

            if (_exitAnimation != null)
                _ = CloseAnimated(view);
            else
                _channel.RequestClose(view);
        }

        private async Awaitable CloseAnimated(UIView view)
        {
            await _exitAnimation(view);
            _channel.RequestClose(view);
        }

        private void HandleOpened(UIView view)
        {
            if (view != null) _openViews[view.GetType()] = view;
        }

        private void HandleClosed(UIView view)
        {
            if (view != null) _openViews.Remove(view.GetType());
        }

        private void HandleAllClosed()
        {
            _openViews.Clear();
            _history.Clear();
        }

        private void Raise(NavigationAction action, Type from, Type to)
            => Changed?.Invoke(new NavigationChange(action, from, to));

        public void Dispose()
        {
            _channel.ScreenOpened -= HandleOpened;
            _channel.ScreenClosed -= HandleClosed;
            _channel.AllScreensClosed -= HandleAllClosed;
            _history.Clear();
            _openViews.Clear();
        }
    }
}

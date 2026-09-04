using System;
using System.Collections.Generic;
using Jeomseon.Unity.UI.Channels;

namespace Jeomseon.Unity.UI.Transition
{
    /// <summary>
    /// <see cref="UIChannel"/>의 알림 이벤트만 구독해 "현재 열려 있는 화면"을 구체 타입별로 추적합니다.
    /// exit 연출은 화면이 닫히기 전에 그 <see cref="UIView"/> 인스턴스가 필요하므로, Core를 건드리지
    /// 않고 여기서 매핑을 유지합니다(ADR-0009 §2).
    /// </summary>
    public sealed class OpenScreenTracker : IDisposable
    {
        private readonly UIChannel _channel;
        private readonly Dictionary<Type, UIView> _open = new();

        public OpenScreenTracker(UIChannel channel)
        {
            _channel = channel ? channel : throw new ArgumentNullException(nameof(channel));
            _channel.ScreenOpened += HandleOpened;
            _channel.ScreenClosed += HandleClosed;
            _channel.AllScreensClosed += HandleAllClosed;
        }

        public bool TryGet(Type screenType, out UIView view) => _open.TryGetValue(screenType, out view);

        public bool TryGet<T>(out T view) where T : UIView
        {
            if (_open.TryGetValue(typeof(T), out UIView found))
            {
                view = (T)found;
                return true;
            }

            view = null;
            return false;
        }

        private void HandleOpened(UIView view)
        {
            if (view != null) _open[view.GetType()] = view;
        }

        private void HandleClosed(UIView view)
        {
            if (view != null) _open.Remove(view.GetType());
        }

        private void HandleAllClosed() => _open.Clear();

        public void Dispose()
        {
            _channel.ScreenOpened -= HandleOpened;
            _channel.ScreenClosed -= HandleClosed;
            _channel.AllScreensClosed -= HandleAllClosed;
            _open.Clear();
        }
    }
}

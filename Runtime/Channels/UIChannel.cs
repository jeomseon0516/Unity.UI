using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Jeomseon.Unity.UI.Channels
{
    // .. UIStackManager와 UIView가 서로를 직접 참조하지 않고 이 자산을 통해서만 상호작용합니다(ADR-0008).
    [CreateAssetMenu(fileName = "UIChannel", menuName = "Jeomseon/UI/UI Channel")]
    public sealed class UIChannel : ScriptableObject
    {
        [SerializeField] private List<UIScreenEntry> entries = new();

        public IReadOnlyList<UIScreenEntry> Entries => entries;

        internal event Action<Type> OpenRequested;
        internal event Action<UIView> CloseRequested;
        internal event Action CloseAllRequested;

        public event UnityAction<UIView> ScreenOpened;
        public event UnityAction<UIView> ScreenClosed;
        public event UnityAction AllScreensClosed;

        public void RequestOpen<T>() where T : UIView => OpenRequested?.Invoke(typeof(T));
        public void RequestClose(UIView screen) => CloseRequested?.Invoke(screen);
        public void RequestCloseAll() => CloseAllRequested?.Invoke();

        internal void NotifyScreenOpened(UIView screen)
            => ScreenOpened?.Invoke(screen);

        internal void NotifyScreenClosed(UIView screen)
            => ScreenClosed?.Invoke(screen);

        internal void NotifyAllScreensClosed() => AllScreensClosed?.Invoke();
    }
}

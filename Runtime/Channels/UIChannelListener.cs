using System;
using UnityEngine;
using UnityEngine.Events;

namespace Jeomseon.Unity.UI.Channels
{
    [DisallowMultipleComponent]
    public sealed class UIChannelListener : MonoBehaviour
    {
        [SerializeField] private UIChannel channel;
        [SerializeField] private UnityEvent<string> screenOpened = new();
        [SerializeField] private UnityEvent<string> screenClosed = new();
        [SerializeField] private UnityEvent allScreensClosed = new();

        private void OnEnable()
        {
            if (!channel) return;

            channel.ScreenOpened += OnScreenOpened;
            channel.ScreenClosed += OnScreenClosed;
            channel.AllScreensClosed += allScreensClosed.Invoke;
        }

        private void OnDisable()
        {
            if (!channel) return;

            channel.ScreenOpened -= OnScreenOpened;
            channel.ScreenClosed -= OnScreenClosed;
            channel.AllScreensClosed -= allScreensClosed.Invoke;
        }

        private void OnScreenOpened(Type screenType)
            => screenOpened.Invoke(screenType.AssemblyQualifiedName);

        private void OnScreenClosed(Type screenType)
            => screenClosed.Invoke(screenType.AssemblyQualifiedName);
    }
}

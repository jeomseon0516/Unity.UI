using UnityEngine;
using UnityEngine.Events;

namespace Jeomseon.Unity.UI.Channels
{
    [DisallowMultipleComponent]
    public sealed class UIChannelListener : MonoBehaviour
    {
        [SerializeField] private UIChannel channel;
        [SerializeField] private UnityEvent<UIView> screenOpened = new();
        [SerializeField] private UnityEvent<UIView> screenClosed = new();
        [SerializeField] private UnityEvent allScreensClosed = new();

        private void OnEnable()
        {
            if (!channel) return;

            channel.ScreenOpened += screenOpened.Invoke;
            channel.ScreenClosed += screenClosed.Invoke;
            channel.AllScreensClosed += allScreensClosed.Invoke;
        }

        private void OnDisable()
        {
            if (!channel) return;

            channel.ScreenOpened -= screenOpened.Invoke;
            channel.ScreenClosed -= screenClosed.Invoke;
            channel.AllScreensClosed -= allScreensClosed.Invoke;
        }
    }
}

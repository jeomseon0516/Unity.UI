using Jeomseon.Unity.UI;
using Jeomseon.Unity.UI.Channels;
using UnityEngine;

namespace Jeomseon.Samples.UI
{
    public sealed class UIBasicUsageSample : MonoBehaviour
    {
        [SerializeField] private UIChannel channel;

        private void Start() => channel.RequestOpen<HomeView>();

        public void OnScreenOpenedFromInspector(UIView screen)
            => Debug.Log($"UIChannelListener Dynamic UIView opened: {screen.GetType().Name}", this);

        public void OnScreenClosedFromInspector(UIView screen)
            => Debug.Log($"UIChannelListener Dynamic UIView closed: {screen.GetType().Name}", this);
    }
}

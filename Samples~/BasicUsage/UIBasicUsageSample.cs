using Jeomseon.Unity.UI.Channels;
using UnityEngine;

namespace Jeomseon.Samples.UI
{
    public sealed class UIBasicUsageSample : MonoBehaviour
    {
        [SerializeField] private UIChannel channel;

        private void Start() => channel.RequestOpen<HomeView>();
    }
}

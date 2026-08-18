using UnityEngine;
using UnityEngine.UIElements;

namespace Jeomseon.Unity.UI.Channels
{
    [System.Serializable]
    public sealed class UIScreenEntry
    {
        [SerializeField] private UILayer layer;
        [SerializeField] private VisualTreeAsset layout;

        public UILayer Layer => layer;
        public VisualTreeAsset Layout => layout;
    }
}

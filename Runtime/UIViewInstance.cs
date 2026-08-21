using UnityEngine.UIElements;

namespace Jeomseon.Unity.UI
{
    internal readonly struct UIViewInstance
    {
        public UIViewInstance(UILayer layer,
            UIView view,
            VisualElement host)
        {
            Layer = layer;
            View = view;
            Host = host;
        }

        public UILayer Layer { get; }
        public UIView View { get; }
        public VisualElement Host { get; }

        public void Deconstruct(out UILayer layer, out UIView view, out VisualElement host)
        {
            layer = Layer;
            view = View;
            host = Host;
        }
    }
}
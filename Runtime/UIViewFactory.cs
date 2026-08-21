using System.Linq;
using Jeomseon.Unity.UI.Channels;
using UnityEngine;
using UnityEngine.UIElements;

namespace Jeomseon.Unity.UI
{
    internal sealed class UIViewFactory
    {
        public bool TryCreate(
            UIScreenEntry entry,
            IUIRequester channel,
            Object logContext,
            out UIViewInstance instance)
        {
            instance = default;

            if (!entry.Layout)
            {
                Debug.LogWarning(
                    $"Skipping invalid {nameof(UIScreenEntry)}.",
                    logContext);
                return false;
            }

            TemplateContainer host = entry.Layout.Instantiate();
            var views = host.Children().OfType<UIView>().ToArray();

            if (views.Length != 1)
            {
                Debug.LogWarning(
                    $"Skipping layout '{entry.Layout.name}'. Its UXML root must contain exactly one {nameof(UIView)}.",
                    entry.Layout);
                return false;
            }

            UIView view = views[0];
            view.Initialize(channel);

            host.StretchToParentSize();
            host.pickingMode = PickingMode.Ignore;

            instance = new UIViewInstance(entry.Layer, view, host);
            return true;
        }
    }
}

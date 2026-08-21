using System;
using System.Collections.Generic;
using Jeomseon.Unity.UI.Channels;

namespace Jeomseon.Unity.UI
{
    internal sealed class UIStackController
    {
        private readonly Dictionary<UILayer, List<UIView>> _activeStacks = new();
        private readonly Dictionary<Type, UIView> _screenRegistry = new();
        private readonly Dictionary<UIView, UIViewInstance> _instances = new();

        public void AddLayer(UILayer layer)
        {
            _activeStacks.TryAdd(layer, new List<UIView>());
        }

        public bool TryRegister(UIViewInstance instance)
        {
            Type screenType = instance.View.GetType();

            if (!_screenRegistry.TryAdd(screenType, instance.View))
            {
                return false;
            }

            _instances.Add(instance.View, instance);
            instance.View.SetVisible(false);
            return true;
        }

        public bool TryGet(Type screenType, out UIView view)
            => _screenRegistry.TryGetValue(screenType, out view);

        public T Get<T>() where T : UIView
            => _screenRegistry.TryGetValue(typeof(T), out UIView view)
                ? view as T
                : null;

        public bool TryOpen(Type screenType, out UIView view)
        {
            if (!_screenRegistry.TryGetValue(screenType, out view))
            {
                return false;
            }

            UIViewInstance instance = _instances[view];
            List<UIView> stack = _activeStacks[instance.Layer];

            stack.Remove(view);
            stack.Add(view);

            instance.Host.BringToFront();
            view.SetVisible(true);
            return true;
        }

        public bool TryClose(UIView view)
        {
            if (view == null ||
                !_instances.TryGetValue(view, out UIViewInstance instance))
            {
                return false;
            }

            var stack = _activeStacks[instance.Layer];

            if (!stack.Remove(view))
            {
                return false;
            }

            view.SetVisible(false);
            return true;
        }

        public void CloseAll()
        {
            foreach (var stack in _activeStacks.Values)
            {
                foreach (UIView view in stack)
                {
                    view.SetVisible(false);
                }

                stack.Clear();
            }
        }

        public void SetChannel(IUIRequester channel)
        {
            foreach (UIView view in _screenRegistry.Values)
            {
                view.SetChannel(channel);
            }
        }

        public void Clear()
        {
            _activeStacks.Clear();
            _screenRegistry.Clear();
            _instances.Clear();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Jeomseon.Unity.UI.Channels;
using UnityEngine;
using UnityEngine.UIElements;

namespace Jeomseon.Unity.UI
{
    public sealed class UIStackManager : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
        [SerializeField] private UIChannel channel;

        private readonly Dictionary<UILayer, VisualElement> _layerContainers = new();
        private readonly Dictionary<UILayer, List<UIView>> _activeStacks = new();
        private readonly Dictionary<Type, UIView> _uiPool = new();
        private readonly Dictionary<UIView, UILayer> _screenLayers = new();
        private readonly Dictionary<UIView, VisualElement> _screenHosts = new();

        internal void ConfigureForTests(UIDocument uiDocument, UIChannel uiChannel)
        {
            document = uiDocument;
            channel = uiChannel;
        }

        public void Initialize(UIChannel uiChannel)
        {
            if (document == null)
            {
                Debug.LogError($"{nameof(UIStackManager)} requires a {nameof(UIDocument)} reference.", this);
                return;
            }

            if (channel != null)
            {
                channel.OpenRequested -= HandleOpenRequested;
                channel.CloseRequested -= HandleCloseRequested;
                channel.CloseAllRequested -= HandleCloseAllRequested;
            }

            channel = uiChannel;
            ClearCatalog();
            BuildCatalog();

            if (isActiveAndEnabled && channel != null)
            {
                channel.OpenRequested += HandleOpenRequested;
                channel.CloseRequested += HandleCloseRequested;
                channel.CloseAllRequested += HandleCloseAllRequested;
            }
        }

        private void Awake()
        {
            if (!document) return;
            BuildCatalog();
        }

        private void BuildCatalog()
        {
            foreach (var layer in (UILayer[])Enum.GetValues(typeof(UILayer)))
            {
                var container = new VisualElement { name = layer.ToString(), pickingMode = PickingMode.Ignore };
                container.StretchToParentSize();

                document.rootVisualElement.Add(container);
                _layerContainers[layer] = container;
                _activeStacks[layer] = new List<UIView>();
            }

            if (!channel) return;

            foreach (var entry in channel.Entries)
            {
                RegisterFromEntry(entry);
            }
        }

        internal void ClearCatalog()
        {
            foreach (var container in _layerContainers.Values)
            {
                container.RemoveFromHierarchy();
            }

            _layerContainers.Clear();
            _activeStacks.Clear();
            _uiPool.Clear();
            _screenLayers.Clear();
            _screenHosts.Clear();
        }

        private void OnEnable()
        {
            if (!channel) return;

            channel.OpenRequested += HandleOpenRequested;
            channel.CloseRequested += HandleCloseRequested;
            channel.CloseAllRequested += HandleCloseAllRequested;
        }

        private void OnDisable()
        {
            if (!channel) return;

            channel.OpenRequested -= HandleOpenRequested;
            channel.CloseRequested -= HandleCloseRequested;
            channel.CloseAllRequested -= HandleCloseAllRequested;
        }

        internal T GetUI<T>() where T : UIView
            => _uiPool.TryGetValue(typeof(T), out var screen) ? screen as T : null;

        private void CloseAllUI()
        {
            foreach (var stack in _activeStacks.Values)
            {
                foreach (var screen in stack) screen.SetVisible(false);
                stack.Clear();
            }
        }

        private void HandleOpenRequested(Type screenType)
        {
            if (!_uiPool.TryGetValue(screenType, out var screen)) return;

            OpenScreen(screen);
            channel.NotifyScreenOpened(screen);
        }

        private void HandleCloseRequested(UIView screen)
        {
            if (!TryCloseUI(screen)) return;

            channel.NotifyScreenClosed(screen);
        }

        private void HandleCloseAllRequested()
        {
            CloseAllUI();
            channel.NotifyAllScreensClosed();
        }

        private bool TryCloseUI(UIView screen)
        {
            // .. 등록되지 않았거나 다른 레이어 소속이면 무시(레이어에 없으면 이미 닫힌 것으로 취급)
            if (screen == null || !_screenLayers.TryGetValue(screen, out var layer)) return false;
            if (!_activeStacks[layer].Remove(screen)) return false;

            screen.SetVisible(false);
            return true;
        }

        private void OpenScreen(UIView screen)
        {
            var layer = _screenLayers[screen];
            var stack = _activeStacks[layer];

            stack.Remove(screen);
            stack.Add(screen); // .. 최상단으로

            _screenHosts[screen].BringToFront();
            screen.SetVisible(true);
        }

        private void RegisterFromEntry(UIScreenEntry entry)
        {
            if (!entry.Layout)
            {
                Debug.LogWarning($"Skipping invalid {nameof(UIScreenEntry)} on channel '{channel.name}'.", channel);
                return;
            }

            var host = entry.Layout.Instantiate();
            var screens = host.Children().OfType<UIView>().ToArray();
            if (screens.Length != 1)
            {
                Debug.LogWarning(
                    $"Skipping layout '{entry.Layout.name}'. Its UXML root must contain exactly one {nameof(UIView)}.",
                    entry.Layout);
                return;
            }

            var screen = screens[0];
            screen.Initialize(channel);
            host.StretchToParentSize();
            host.pickingMode = PickingMode.Ignore;

            RegisterScreen(entry.Layer, screen, host);
        }

        internal void RegisterScreen(UILayer layer, UIView screen)
            => RegisterScreen(layer, screen, screen);

        private void RegisterScreen(UILayer layer, UIView screen, VisualElement host)
        {
            var screenType = screen.GetType();
            if (!_uiPool.TryAdd(screenType, screen))
            {
                Debug.LogWarning($"Duplicate UIView registration ignored for type '{screenType.Name}'.", this);
                return;
            }

            _screenLayers[screen] = layer;
            _screenHosts[screen] = host;
            _layerContainers[layer].Add(host);
            screen.SetVisible(false);
        }
    }
}

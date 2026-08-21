using System;
using System.Collections.Generic;
using Jeomseon.Unity.UI.Channels;
using UnityEngine;
using UnityEngine.UIElements;

namespace Jeomseon.Unity.UI
{
    /// <summary>
    /// UIDocument에 Layer 컨테이너를 만들고 Catalog의 View를 등록한 뒤, Channel 요청을 스택 조작으로
    /// 연결하는 조합 루트입니다. View 인스턴스화는 <see cref="UIViewFactory"/>가, 스택과 등록 상태는
    /// <see cref="UIStackController"/>가 담당합니다.
    /// </summary>
    public sealed class UIStackManager : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
        [SerializeField] private UICatalog catalog;
        [SerializeField] private UIChannel channel;

        /// <summary>Layer별 루트 컨테이너입니다. UIDocument 계층에 직접 붙는 유일한 상태입니다.</summary>
        private readonly Dictionary<UILayer, VisualElement> _layerContainers = new();
        /// <summary>스택, 등록 목록과 View별 인스턴스 정보를 소유합니다.</summary>
        private readonly UIStackController _controller = new();
        /// <summary>UXML Layout에서 View 인스턴스를 만듭니다.</summary>
        private readonly UIViewFactory _factory = new();

        internal void ConfigureForTests(UIDocument uiDocument, UIChannel uiChannel)
        {
            document = uiDocument;
            channel = uiChannel;
        }

        /// <summary>
        /// 통신 Channel만 교체합니다. Catalog와 이미 등록된 View는 유지되며 생성 훅도 다시 실행되지
        /// 않습니다. Catalog 재구축은 <see cref="RebuildCatalog"/>가 담당하는 별개 책임입니다.
        /// </summary>
        public void SetChannel(UIChannel newChannel)
        {
            if (channel == newChannel) return;

            UnsubscribeFromChannel();
            channel = newChannel;
            // 이미 만들어진 View들도 새 Channel로 요청을 보내야 합니다.
            _controller.SetChannel(channel);
            if (isActiveAndEnabled) SubscribeToChannel();
        }

        /// <summary>
        /// 현재 Catalog로 Layer 컨테이너와 View를 다시 만듭니다. Channel 구독은 건드리지 않습니다.
        /// </summary>
        public void RebuildCatalog()
        {
            if (document == null)
            {
                Debug.LogError($"{nameof(UIStackManager)} requires a {nameof(UIDocument)} reference.", this);
                return;
            }

            ClearCatalog();
            BuildCatalog();
        }

        private void Awake()
        {
            if (!document) return;
            BuildCatalog();
        }

        private void OnEnable() => SubscribeToChannel();
        private void OnDisable() => UnsubscribeFromChannel();

        /// <summary>Layer 컨테이너를 만들고 Catalog 항목을 View로 등록합니다.</summary>
        private void BuildCatalog()
        {
            foreach (UILayer layer in (UILayer[])Enum.GetValues(typeof(UILayer)))
            {
                VisualElement container = new() { name = layer.ToString(), pickingMode = PickingMode.Ignore };
                container.StretchToParentSize();

                document.rootVisualElement.Add(container);
                _layerContainers[layer] = container;
                _controller.AddLayer(layer);
            }

            if (!catalog) return;

            foreach (UIScreenEntry entry in catalog.Entries)
            {
                if (!_factory.TryCreate(entry, channel, catalog, out UIViewInstance instance)) continue;
                Register(instance);
            }
        }

        internal void ClearCatalog()
        {
            foreach (VisualElement container in _layerContainers.Values)
            {
                container.RemoveFromHierarchy();
            }

            _layerContainers.Clear();
            _controller.Clear();
        }

        internal T GetUI<T>() where T : UIView => _controller.Get<T>();

        /// <summary>코드로 만든 View를 자기 자신을 host로 삼아 등록합니다.</summary>
        internal void RegisterScreen(UILayer layer, UIView screen)
        {
            if (screen == null) return;
            screen.Initialize(channel);
            Register(new UIViewInstance(layer, screen, screen));
        }

        /// <summary>인스턴스를 Layer 컨테이너에 붙이고 Controller에 등록합니다.</summary>
        private void Register(in UIViewInstance instance)
        {
            if (!_layerContainers.TryGetValue(instance.Layer, out VisualElement container))
            {
                Debug.LogWarning($"Layer '{instance.Layer}' has no container; registration ignored.", this);
                return;
            }

            if (!_controller.TryRegister(instance))
            {
                Debug.LogWarning(
                    $"Duplicate UIView registration ignored for type '{instance.View.GetType().Name}'.", this);
                return;
            }

            container.Add(instance.Host);
        }

        private void SubscribeToChannel()
        {
            if (!channel) return;

            channel.OpenRequested += HandleOpenRequested;
            channel.CloseRequested += HandleCloseRequested;
            channel.CloseAllRequested += HandleCloseAllRequested;
        }

        private void UnsubscribeFromChannel()
        {
            if (!channel) return;

            channel.OpenRequested -= HandleOpenRequested;
            channel.CloseRequested -= HandleCloseRequested;
            channel.CloseAllRequested -= HandleCloseAllRequested;
        }

        private void HandleOpenRequested(Type screenType)
        {
            if (!_controller.TryOpen(screenType, out UIView view)) return;
            channel.NotifyScreenOpened(view);
        }

        private void HandleCloseRequested(UIView screen)
        {
            if (!_controller.TryClose(screen)) return;
            channel.NotifyScreenClosed(screen);
        }

        private void HandleCloseAllRequested()
        {
            _controller.CloseAll();
            channel.NotifyAllScreensClosed();
        }
    }
}

using Jeomseon.Unity.UI.Channels;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace Jeomseon.Unity.UI.Tests
{
    /// <summary>
    /// Stack 상태 전이를 Scene·UIDocument 없이 검증합니다. <see cref="UIStackController"/>는 순수 C#
    /// 객체이므로 UXML Instantiate나 Play Mode 없이 등록·열기·닫기 규칙을 직접 확인할 수 있습니다.
    /// </summary>
    public sealed class UIStackControllerTests
    {
        private sealed class ViewA : UIView
        {
            /// <summary>protected Channel로 실제 요청이 나가는지 확인하기 위한 통로입니다.</summary>
            public void TriggerCloseAll() => Channel.RequestCloseAll();
        }

        private sealed class ViewB : UIView { }

        /// <summary>요청을 기록만 하는 테스트용 Requester입니다.</summary>
        private sealed class RecordingRequester : IUIRequester
        {
            public int CloseAllRequests { get; private set; }
            public void RequestOpen<T>() where T : UIView { }
            public void RequestClose(UIView screen) { }
            public void RequestCloseAll() => CloseAllRequests++;
        }

        private static UIStackController CreateController()
        {
            UIStackController controller = new();
            controller.AddLayer(UILayer.Screen);
            controller.AddLayer(UILayer.Popup);
            controller.AddLayer(UILayer.System);
            return controller;
        }

        private static UIViewInstance CreateInstance<T>(UILayer layer, out T view) where T : UIView, new()
        {
            view = new T();
            VisualElement host = new();
            host.Add(view);
            return new UIViewInstance(layer, view, host);
        }

        [Test]
        public void TryRegister_HidesViewAndMakesItRetrievableByType()
        {
            UIStackController controller = CreateController();
            UIViewInstance instance = CreateInstance<ViewA>(UILayer.Screen, out ViewA view);

            Assert.That(controller.TryRegister(instance), Is.True);
            Assert.That(controller.Get<ViewA>(), Is.SameAs(view));
            // 등록 직후에는 항상 숨겨진 상태여야 합니다.
            Assert.That(view.IsVisible, Is.False);
        }

        [Test]
        public void TryRegister_DuplicateType_IsRejectedAndKeepsFirstInstance()
        {
            UIStackController controller = CreateController();
            UIViewInstance first = CreateInstance<ViewA>(UILayer.Screen, out ViewA firstView);
            UIViewInstance second = CreateInstance<ViewA>(UILayer.Screen, out ViewA _);

            Assert.That(controller.TryRegister(first), Is.True);
            Assert.That(controller.TryRegister(second), Is.False);
            Assert.That(controller.Get<ViewA>(), Is.SameAs(firstView));
        }

        [Test]
        public void TryOpen_ThenTryClose_TogglesVisibility()
        {
            UIStackController controller = CreateController();
            controller.TryRegister(CreateInstance<ViewA>(UILayer.Screen, out ViewA view));

            Assert.That(controller.TryOpen(typeof(ViewA), out UIView opened), Is.True);
            Assert.That(opened, Is.SameAs(view));
            Assert.That(view.IsVisible, Is.True);

            Assert.That(controller.TryClose(view), Is.True);
            Assert.That(view.IsVisible, Is.False);
        }

        [Test]
        public void TryClose_ViewNotInStack_ReturnsFalse()
        {
            UIStackController controller = CreateController();
            controller.TryRegister(CreateInstance<ViewA>(UILayer.Screen, out ViewA view));

            // 열지 않은 View를 닫으려 하면 스택에 없으므로 실패해야 합니다.
            Assert.That(controller.TryClose(view), Is.False);
            Assert.That(controller.TryClose(null), Is.False);
        }

        [Test]
        public void TryOpen_UnregisteredType_ReturnsFalse()
        {
            UIStackController controller = CreateController();
            Assert.That(controller.TryOpen(typeof(ViewA), out UIView view), Is.False);
            Assert.That(view, Is.Null);
        }

        [Test]
        public void TryOpen_SameViewTwice_KeepsSingleStackEntry()
        {
            UIStackController controller = CreateController();
            controller.TryRegister(CreateInstance<ViewA>(UILayer.Screen, out ViewA view));

            controller.TryOpen(typeof(ViewA), out _);
            controller.TryOpen(typeof(ViewA), out _);

            // 재오픈은 최상단으로 옮기기만 하므로 첫 Close에서 스택이 비어야 합니다.
            Assert.That(controller.TryClose(view), Is.True);
            Assert.That(controller.TryClose(view), Is.False);
        }

        [Test]
        public void CloseAll_HidesEveryLayerAndClearsStacks()
        {
            UIStackController controller = CreateController();
            controller.TryRegister(CreateInstance<ViewA>(UILayer.Screen, out ViewA screenView));
            controller.TryRegister(CreateInstance<ViewB>(UILayer.Popup, out ViewB popupView));
            controller.TryOpen(typeof(ViewA), out _);
            controller.TryOpen(typeof(ViewB), out _);

            controller.CloseAll();

            Assert.That(screenView.IsVisible, Is.False);
            Assert.That(popupView.IsVisible, Is.False);
            // 스택이 비었으므로 다시 닫으려 하면 실패해야 합니다.
            Assert.That(controller.TryClose(screenView), Is.False);
            Assert.That(controller.TryClose(popupView), Is.False);
        }

        [Test]
        public void SetChannel_ReplacesRequesterOnEveryRegisteredView()
        {
            UIStackController controller = CreateController();
            controller.TryRegister(CreateInstance<ViewA>(UILayer.Screen, out ViewA view));
            RecordingRequester requester = new();

            controller.SetChannel(requester);
            // View가 새 Requester로 요청을 보내는지 확인합니다.
            view.TriggerCloseAll();

            Assert.That(requester.CloseAllRequests, Is.EqualTo(1));
        }

        [Test]
        public void Clear_RemovesRegistrationsAndStacks()
        {
            UIStackController controller = CreateController();
            controller.TryRegister(CreateInstance<ViewA>(UILayer.Screen, out _));

            controller.Clear();

            Assert.That(controller.Get<ViewA>(), Is.Null);
            Assert.That(controller.TryOpen(typeof(ViewA), out _), Is.False);
        }
    }
}

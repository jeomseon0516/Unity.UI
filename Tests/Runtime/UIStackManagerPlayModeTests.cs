using System;
using System.Collections;
using Jeomseon.Unity.UI;
using Jeomseon.Unity.UI.Channels;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Jeomseon.Tests
{
    public sealed class UIStackManagerPlayModeTests
    {
        private sealed class TestScreenA : UIView
        {
        }

        private sealed class TestScreenB : UIView
        {
        }

        private static UIStackManager CreateManager(UIChannel channel = null)
        {
            var go = new GameObject(nameof(UIStackManager));
            go.SetActive(false); // .. Awake(초기화) 전에 필드를 먼저 채우기 위해 비활성 상태로 구성

            var document = go.AddComponent<UIDocument>();
            document.panelSettings = ScriptableObject.CreateInstance<PanelSettings>();

            UIStackManager manager = go.AddComponent<UIStackManager>();
            manager.ConfigureForTests(document, channel);

            go.SetActive(true);
            return manager;
        }

        private static T CreateScreen<T>(UIChannel channel = null) where T : UIView, new()
        {
            var screen = new T();
            screen.Initialize(channel);

            return screen;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (UIStackManager manager in UnityEngine.Object.FindObjectsByType<UIStackManager>(FindObjectsInactive.Include))
            {
                if (manager) UnityEngine.Object.Destroy(manager.gameObject);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator OpenThenClose_TogglesVisibility()
        {
            var channel = ScriptableObject.CreateInstance<UIChannel>();
            UIStackManager manager = CreateManager(channel);
            TestScreenA screen = CreateScreen<TestScreenA>(channel);
            manager.RegisterScreen(UILayer.Screen, screen);

            channel.RequestOpen<TestScreenA>();
            yield return null;

            Assert.That(screen.IsVisible, Is.True);
            Assert.That(manager.GetUI<TestScreenA>(), Is.SameAs(screen));

            channel.RequestClose(screen);
            yield return null;

            Assert.That(screen.IsVisible, Is.False);
        }

        [UnityTest]
        public IEnumerator RegisterScreen_IgnoresDuplicateTypeWithoutThrowing()
        {
            UIStackManager manager = CreateManager();
            TestScreenA first = CreateScreen<TestScreenA>();
            TestScreenA second = CreateScreen<TestScreenA>();

            Assert.DoesNotThrow(() =>
            {
                manager.RegisterScreen(UILayer.Screen, first);
                manager.RegisterScreen(UILayer.Screen, second);
            });

            Assert.That(manager.GetUI<TestScreenA>(), Is.SameAs(first));

            yield return null;
        }

        [UnityTest]
        public IEnumerator ChannelClose_WithScreenNotInStack_DoesNotAffectUnrelatedOpenScreen()
        {
            var channel = ScriptableObject.CreateInstance<UIChannel>();
            UIStackManager manager = CreateManager(channel);
            TestScreenA a = CreateScreen<TestScreenA>(channel);
            TestScreenB b = CreateScreen<TestScreenB>(channel);
            manager.RegisterScreen(UILayer.Screen, a);
            manager.RegisterScreen(UILayer.Popup, b);

            channel.RequestOpen<TestScreenA>();
            yield return null;

            // .. b는 등록만 됐을 뿐 한 번도 열린 적이 없어 어느 레이어 스택에도 없는 상태
            channel.RequestClose(b);
            yield return null;

            Assert.That(a.IsVisible, Is.True, "스택에 없는 화면을 닫아도 이미 열려 있던 무관한 화면이 닫히면 안 됩니다.");
        }

        [UnityTest]
        public IEnumerator Channel_OpenAndCloseRequests_ReachManager()
        {
            var channel = ScriptableObject.CreateInstance<UIChannel>();
            UIStackManager manager = CreateManager(channel);
            TestScreenA screen = CreateScreen<TestScreenA>(channel);
            manager.RegisterScreen(UILayer.Screen, screen);
            Type openedType = null;
            Type closedType = null;
            channel.ScreenOpened += screenType => openedType = screenType;
            channel.ScreenClosed += screenType => closedType = screenType;

            channel.RequestOpen<TestScreenA>();
            yield return null;

            Assert.That(screen.IsVisible, Is.True);
            Assert.That(openedType, Is.EqualTo(typeof(TestScreenA)));

            channel.RequestClose(screen);
            yield return null;

            Assert.That(screen.IsVisible, Is.False);
            Assert.That(closedType, Is.EqualTo(typeof(TestScreenA)));
        }
    }
}

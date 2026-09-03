using System;
using System.Collections.Generic;
using Jeomseon.Unity.UI.Channels;
using Jeomseon.Unity.UI.Navigation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Jeomseon.Unity.UI.Tests
{
    /// <summary>
    /// <see cref="NavigationStack"/>는 순수 C#이므로 Scene·UIDocument 없이 history 전이와 채널 구동을
    /// 직접 검증합니다. 실제 <see cref="UIChannel"/> 인스턴스를 쓰되, Manager 대신 테스트가
    /// <c>OpenRequested</c>/<c>CloseRequested</c>를 기록하고 <c>NotifyScreenOpened</c>로 표시 상태를
    /// 흉내 냅니다.
    /// </summary>
    public sealed class NavigationStackTests
    {
        private sealed class ScreenA : UIView { }
        private sealed class ScreenB : UIView { }
        private sealed class ScreenC : UIView { }

        private UIChannel _channel;
        private NavigationStack _nav;
        private readonly List<Type> _opened = new();
        private readonly List<UIView> _closed = new();
        private readonly Dictionary<Type, UIView> _views = new();

        [SetUp]
        public void SetUp()
        {
            _opened.Clear();
            _closed.Clear();
            _views.Clear();
            _channel = ScriptableObject.CreateInstance<UIChannel>();
            _channel.OpenRequested += type =>
            {
                _opened.Add(type);
                // Manager가 화면을 표시하면 채널이 ScreenOpened를 발행하는 것을 흉내 낸다.
                _channel.NotifyScreenOpened(ViewFor(type));
            };
            _channel.CloseRequested += view =>
            {
                _closed.Add(view);
                _channel.NotifyScreenClosed(view);
            };
            _nav = new NavigationStack(_channel);
        }

        [TearDown]
        public void TearDown()
        {
            _nav.Dispose();
            UnityEngine.Object.DestroyImmediate(_channel);
        }

        private UIView ViewFor(Type type)
        {
            if (!_views.TryGetValue(type, out UIView view))
            {
                view = (UIView)Activator.CreateInstance(type);
                _views[type] = view;
            }

            return view;
        }

        [Test]
        public void Constructor_NullChannel_Throws()
            => Assert.Throws<ArgumentNullException>(() => new NavigationStack(null));

        [Test]
        public void Push_RecordsHistoryAndRequestsOpen()
        {
            _nav.Push<ScreenA>();
            _nav.Push<ScreenB>();

            Assert.That(_nav.Current, Is.EqualTo(typeof(ScreenB)));
            Assert.That(_nav.Depth, Is.EqualTo(2));
            Assert.That(_nav.CanGoBack, Is.True);
            Assert.That(_opened, Is.EqualTo(new[] { typeof(ScreenA), typeof(ScreenB) }));
        }

        [Test]
        public void Push_CarriesArgs()
        {
            var payload = new object();
            _nav.Push<ScreenA>(payload);

            Assert.That(_nav.CurrentArgs, Is.SameAs(payload));
        }

        [Test]
        public void Back_ClosesTopAndReopensPrevious()
        {
            _nav.Push<ScreenA>();
            _nav.Push<ScreenB>();
            _opened.Clear();

            bool moved = _nav.Back();

            Assert.That(moved, Is.True);
            Assert.That(_nav.Current, Is.EqualTo(typeof(ScreenA)));
            Assert.That(_nav.Depth, Is.EqualTo(1));
            Assert.That(_closed, Is.EqualTo(new[] { ViewFor(typeof(ScreenB)) }));
            Assert.That(_opened, Is.EqualTo(new[] { typeof(ScreenA) }));
        }

        [Test]
        public void Back_AtRoot_ReturnsFalseAndDoesNothing()
        {
            _nav.Push<ScreenA>();
            _closed.Clear();

            Assert.That(_nav.Back(), Is.False);
            Assert.That(_closed, Is.Empty);
            Assert.That(_nav.Current, Is.EqualTo(typeof(ScreenA)));
        }

        [Test]
        public void PopTo_ClosesEverythingAboveTarget()
        {
            _nav.Push<ScreenA>();
            _nav.Push<ScreenB>();
            _nav.Push<ScreenC>();
            _closed.Clear();
            _opened.Clear();

            _nav.PopTo<ScreenA>();

            Assert.That(_nav.Current, Is.EqualTo(typeof(ScreenA)));
            Assert.That(_nav.Depth, Is.EqualTo(1));
            Assert.That(_closed, Is.EqualTo(new[] { ViewFor(typeof(ScreenC)), ViewFor(typeof(ScreenB)) }));
            Assert.That(_opened, Is.EqualTo(new[] { typeof(ScreenA) }));
        }

        [Test]
        public void ResetTo_ClearsHistoryAndOpensNewRoot()
        {
            _nav.Push<ScreenA>();
            _nav.Push<ScreenB>();
            _closed.Clear();

            _nav.ResetTo<ScreenC>();

            Assert.That(_nav.Current, Is.EqualTo(typeof(ScreenC)));
            Assert.That(_nav.Depth, Is.EqualTo(1));
            Assert.That(_closed, Contains.Item(ViewFor(typeof(ScreenA))));
            Assert.That(_closed, Contains.Item(ViewFor(typeof(ScreenB))));
        }

        [Test]
        public void Changed_ReportsFromToAndAction()
        {
            NavigationChange last = default;
            _nav.Changed += change => last = change;

            _nav.Push<ScreenA>();
            Assert.That(last.Action, Is.EqualTo(NavigationAction.Push));
            Assert.That(last.From, Is.Null);
            Assert.That(last.To, Is.EqualTo(typeof(ScreenA)));

            _nav.Push<ScreenB>();
            _nav.Back();
            Assert.That(last.Action, Is.EqualTo(NavigationAction.Back));
            Assert.That(last.From, Is.EqualTo(typeof(ScreenB)));
            Assert.That(last.To, Is.EqualTo(typeof(ScreenA)));
        }

        [Test]
        public void ExitAnimation_IsInvokedForTheClosingView()
        {
            var animated = new List<UIView>();
            using var nav = new NavigationStack(_channel, view =>
            {
                animated.Add(view);
                return NoAwaitable();
            });

            nav.Push<ScreenA>();
            nav.Push<ScreenB>();
            nav.Back();

            Assert.That(animated, Is.EqualTo(new[] { ViewFor(typeof(ScreenB)) }));
        }

        private static Awaitable NoAwaitable()
        {
            var completion = new AwaitableCompletionSource();
            completion.SetResult();
            return completion.Awaitable;
        }
    }
}

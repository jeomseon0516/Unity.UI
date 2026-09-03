using System;
using Jeomseon.Unity.UI.Channels;
using UnityEngine;

namespace Jeomseon.Unity.UI.Transition
{
    /// <summary>
    /// <see cref="UIChannel"/> 위에 얇게 얹는 소비자용 진입점입니다. Core(UIView/UIChannel/
    /// UIStackManager)는 전혀 바꾸지 않습니다(ADR-0009 §2).
    ///
    /// <para><b>enter</b>: <see cref="UIChannel.ScreenOpened"/>를 구독해 화면이 표시되는 즉시
    /// 기본 전략의 <see cref="ITransition.PlayEnter"/>를 재생합니다(<paramref name="autoEnter"/>).</para>
    ///
    /// <para><b>exit</b>: <see cref="CloseAnimated{T}"/>로 닫습니다. 연출이 끝난 뒤 실제
    /// <see cref="IUIRequester.RequestClose"/>를 호출합니다. 평범한 <c>channel.RequestClose()</c>
    /// 직접 호출은 여전히 즉시 닫힙니다(연출 없음).</para>
    ///
    /// 사용이 끝나면 <see cref="Dispose"/>로 구독을 해제하세요.
    /// </summary>
    public sealed class ScreenTransitions : IDisposable
    {
        private readonly UIChannel _channel;
        private readonly OpenScreenTracker _tracker;
        private readonly bool _autoEnter;

        public ScreenTransitions(UIChannel channel, ITransition defaultTransition = null, bool autoEnter = true)
        {
            _channel = channel ? channel : throw new ArgumentNullException(nameof(channel));
            Default = defaultTransition ?? new FadeTransition();
            _tracker = new OpenScreenTracker(channel);
            _autoEnter = autoEnter;

            if (_autoEnter) _channel.ScreenOpened += HandleScreenOpened;
        }

        /// <summary>지정하지 않은 호출에 쓰이는 기본 전략입니다.</summary>
        public ITransition Default { get; }

        /// <summary>현재 열린 화면을 구체 타입별로 조회할 수 있는 추적기입니다.</summary>
        public OpenScreenTracker OpenScreens => _tracker;

        private void HandleScreenOpened(UIView view)
        {
            if (view == null) return;
            _ = Default.PlayEnter(view, TransitionContext.Enter());
        }

        /// <summary>
        /// 타입 <typeparamref name="T"/>의 열린 화면을 연출과 함께 닫습니다. 열려 있지 않으면 아무
        /// 것도 하지 않고 즉시 완료합니다.
        /// </summary>
        public Awaitable CloseAnimated<T>(ITransition transition = null) where T : UIView
            => _tracker.TryGet<T>(out T view) ? CloseAnimated(view, transition) : NoTransition.Completed();

        /// <summary>주어진 화면을 연출과 함께 닫습니다.</summary>
        public async Awaitable CloseAnimated(UIView view, ITransition transition = null)
        {
            if (view == null) return;
            await (transition ?? Default).PlayExit(view, TransitionContext.Exit());
            _channel.RequestClose(view);
        }

        public void Dispose()
        {
            if (_autoEnter) _channel.ScreenOpened -= HandleScreenOpened;
            _tracker.Dispose();
        }
    }
}

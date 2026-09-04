using System;
using UnityEngine.UIElements;

namespace Jeomseon.Unity.UI.Navigation
{
    /// <summary>
    /// panel 루트의 <see cref="NavigationCancelEvent"/>(ESC / 게임패드 B / Android 뒤로가기)를
    /// <see cref="NavigationStack.Back"/>에 연결합니다. Input System 의존성이 없습니다(ADR-0009 §3).
    /// <see cref="Back"/>가 실제로 뒤로 이동하면 이벤트 전파를 멈춥니다.
    /// </summary>
    public sealed class BackNavigationBinder : IDisposable
    {
        private readonly VisualElement _root;
        private readonly NavigationStack _stack;

        public BackNavigationBinder(VisualElement panelRoot, NavigationStack stack)
        {
            _root = panelRoot ?? throw new ArgumentNullException(nameof(panelRoot));
            _stack = stack ?? throw new ArgumentNullException(nameof(stack));
            _root.RegisterCallback<NavigationCancelEvent>(HandleCancel);
        }

        private void HandleCancel(NavigationCancelEvent evt)
        {
            if (_stack.Back()) evt.StopPropagation();
        }

        public void Dispose() => _root.UnregisterCallback<NavigationCancelEvent>(HandleCancel);
    }
}

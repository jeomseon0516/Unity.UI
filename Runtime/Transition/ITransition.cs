using UnityEngine;
using UnityEngine.UIElements;

namespace Jeomseon.Unity.UI.Transition
{
    /// <summary>
    /// 화면 하나가 열리거나 닫힐 때의 enter/exit 연출을 캡슐화한 전략입니다.
    /// 스택·history를 알지 못하며, 대상 <see cref="VisualElement"/>에만 애니메이션을 적용합니다.
    /// 근거: 하네스 ADR-0009.
    /// </summary>
    public interface ITransition
    {
        /// <summary>
        /// 화면이 표시된 직후 호출됩니다. 구현은 먼저 초기 상태(예: opacity 0)를 동기적으로 적용한 뒤
        /// 목표 상태로 애니메이션합니다. 반환된 <see cref="Awaitable"/>은 연출이 끝나면 완료됩니다.
        /// </summary>
        Awaitable PlayEnter(VisualElement view, TransitionContext context);

        /// <summary>
        /// 화면이 실제로 닫히기 전에 호출됩니다. 반환된 <see cref="Awaitable"/>이 완료된 뒤에야
        /// 호출자가 <c>UIChannel.RequestClose</c>로 화면을 닫습니다.
        /// </summary>
        Awaitable PlayExit(VisualElement view, TransitionContext context);
    }
}

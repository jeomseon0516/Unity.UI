using UnityEngine;
using UnityEngine.UIElements;

namespace Jeomseon.Unity.UI.Components
{
    // .. UI Toolkit ScrollView는 콘텐츠 드래그 스크롤과 고무줄 오버스크롤을 터치 입력에서만
    // .. 지원합니다(Unity의 의도된 설계). 이 Manipulator는 포인터 타입을 가리지 않고 uGUI
    // .. ScrollRect와 동등한 드래그·고무줄·관성 스크롤을 제공합니다. ScrollView뿐 아니라 내부에
    // .. ScrollView를 갖는 ListView 등에도 붙일 수 있습니다.
    public sealed class ScrollDragManipulator : Manipulator
    {
        // .. 경계를 넘어선 이동량에 곱하는 저항 계수입니다. 0에 가까울수록 덜 끌려갑니다.
        public float Elasticity { get; set; } = 0.3f;

        // .. 손을 뗀 뒤 경계로 되돌아오는 속도입니다.
        public float SpringSpeed { get; set; } = 0.2f;

        // .. 1초 뒤 남는 속도의 비율입니다(uGUI ScrollRect의 Deceleration Rate와 동일한 의미).
        public float DecelerationRate { get; set; } = 0.135f;

        // .. 이 거리를 넘겨야 드래그로 인정합니다. 넘는 순간 포인터를 캡처해 자식 Button의
        // .. 클릭을 취소하므로, 임계값 이하의 움직임은 그대로 클릭으로 처리됩니다.
        public float DragThreshold { get; set; } = 10f;

        public bool Inertia { get; set; } = true;

        // .. uGUI ScrollRect의 Horizontal/Vertical 체크박스에 해당합니다. 끈 축은 드래그로
        // .. 움직이지 않으며 드래그 인정 임계값 계산에서도 제외됩니다.
        public bool DragHorizontal { get; set; } = true;
        public bool DragVertical { get; set; } = true;

        // .. 축을 실제로 끌 수 있는지는 ScrollView.mode가 1차 기준이고, DragHorizontal/DragVertical이
        // .. 거기서 더 좁히는 역할입니다. mode가 허용하지 않는 축은 스크롤 범위가 0이라 스크롤도
        // .. 안 되지만, 막지 않으면 고무줄 로직이 그 축에서 따로 돌아 끌려다니게 됩니다.
        private bool CanDragHorizontal
            => DragHorizontal && _scrollView != null && _scrollView.mode != ScrollViewMode.Vertical;

        private bool CanDragVertical
            => DragVertical && _scrollView != null && _scrollView.mode != ScrollViewMode.Horizontal;

        private ScrollView _scrollView;
        private IVisualElementScheduledItem _tickHandle;

        private int _pointerId = PointerId.invalidPointerId;
        private bool _dragging;
        private bool _applyPending;

        private Vector2 _dragStartPointer;
        private Vector2 _dragStartOffset;
        private Vector2 _virtualOffset;
        private Vector2 _velocity;
        private float _lastSampleTime;
        private float _lastTickTime;

        protected override void RegisterCallbacksOnTarget()
        {
            _scrollView = target as ScrollView ?? target.Q<ScrollView>();

            // .. 자식이 이벤트를 소비하거나 먼저 포인터를 캡처해도 놓치지 않도록 trickle-down에서 받습니다.
            target.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove, TrickleDown.TrickleDown);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
            target.RegisterCallback<PointerCancelEvent>(OnPointerCancel, TrickleDown.TrickleDown);
            target.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
            target.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);

            // .. ScrollView가 휠을 처리하기 전에 먼저 받아 애니메이션을 취소해야 합니다.
            target.RegisterCallback<WheelEvent>(OnWheel, TrickleDown.TrickleDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove, TrickleDown.TrickleDown);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
            target.UnregisterCallback<PointerCancelEvent>(OnPointerCancel, TrickleDown.TrickleDown);
            target.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
            target.UnregisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            target.UnregisterCallback<WheelEvent>(OnWheel, TrickleDown.TrickleDown);

            StopTicking();
            _scrollView = null;
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (_scrollView == null || _dragging || _pointerId != PointerId.invalidPointerId) return;

            // .. 스크롤바 위에서 시작한 포인터는 스크롤바가 처리해야 합니다. 여기서 드래그를
            // .. 시작하면 임계값을 넘는 순간 포인터를 가로채 스크롤바 조작이 끊깁니다.
            if (evt.target is VisualElement pressed && IsWithinScrollers(pressed))
            {
                // .. 드래그를 시작하지 않더라도 애니메이션은 멈춰야 합니다. 그대로 두면 Tick이 매
                // .. 프레임 위치를 되돌려 스크롤바 조작이 튑니다(휠과 같은 원인).
                CancelAnimation();
                return;
            }

            Vector2 max = MaxOffset();
            bool canDrag = (CanDragHorizontal && max.x > 0f) || (CanDragVertical && max.y > 0f);
            if (!canDrag) return;

            _pointerId = evt.pointerId;
            _dragStartPointer = evt.position;

            // .. 휠·스크롤바로 외부에서 바뀐 위치를 기준으로 다시 시작합니다.
            _virtualOffset = _scrollView.scrollOffset;
            _dragStartOffset = _virtualOffset;
            _velocity = Vector2.zero;
            _lastSampleTime = Time.realtimeSinceStartup;
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (_scrollView == null || evt.pointerId != _pointerId) return;

            Vector2 delta = (Vector2)evt.position - _dragStartPointer;

            // .. 끌 수 없는 축은 이동량에서 제외합니다. 임계값 판정도 켜진 축만으로 계산해야, 예를
            // .. 들어 세로 전용 스크롤뷰에서 가로로만 흔들었을 때 드래그가 시작되지 않습니다.
            if (!CanDragHorizontal) delta.x = 0f;
            if (!CanDragVertical) delta.y = 0f;

            if (!_dragging)
            {
                if (delta.magnitude < DragThreshold) return;

                // .. 임계값을 넘은 시점에 포인터를 가로챕니다. 이때 자식 Button은
                // .. PointerCaptureOutEvent를 받아 클릭이 취소됩니다.
                _dragging = true;
                target.CapturePointer(_pointerId);
                StartTicking();
            }

            // .. 드래그 방향과 스크롤 오프셋은 반대입니다(오른쪽으로 끌면 오프셋 감소).
            Vector2 newOffset = _dragStartOffset - delta;

            float now = Time.realtimeSinceStartup;
            float dt = now - _lastSampleTime;
            if (dt > 0.0001f)
            {
                _velocity = Vector2.Lerp(_velocity, (newOffset - _virtualOffset) / dt, 0.5f);
                _lastSampleTime = now;
            }

            // .. 포인터 이동 이벤트는 한 프레임에 여러 번 올 수 있습니다. 매번 반영하면 그만큼
            // .. ScrollView 갱신이 중복되므로, 값만 갱신해두고 실제 반영은 Tick에서 프레임당 한
            // .. 번만 수행합니다.
            _virtualOffset = newOffset;
            _applyPending = true;
            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (evt.pointerId != _pointerId) return;

            if (_dragging && target.HasPointerCapture(_pointerId)) target.ReleasePointer(_pointerId);
            FinishDrag();
        }

        private void OnPointerCancel(PointerCancelEvent evt)
        {
            if (evt.pointerId != _pointerId) return;

            FinishDrag();
        }

        private void OnPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            if (evt.pointerId != _pointerId) return;

            FinishDrag();
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt) => StopTicking();

        private void OnWheel(WheelEvent evt)
        {
            if (_dragging) return;

            CancelAnimation();
        }

        // .. 탄성 복귀나 관성이 도는 중에는 Tick이 매 프레임 scrollOffset을 _virtualOffset 기준으로
        // .. 되돌립니다. 그 상태에서 휠이나 스크롤바로 위치를 옮기면 곧바로 덮어써져 조작이 먹지
        // .. 않거나 튑니다. 사용자의 직접 조작이 우선이므로 애니메이션을 즉시 끝냅니다.
        private void CancelAnimation()
        {
            if (_scrollView == null) return;

            _velocity = Vector2.zero;

            // .. 휠·스크롤바로 외부에서 바뀐 위치가 기준입니다. 여기서 _virtualOffset을 다시 읽지
            // .. 않으면 이전 드래그의 낡은 값으로 되돌아가 버립니다.
            _virtualOffset = _scrollView.scrollOffset;

            // .. 남아 있던 초과분 translate를 걷어냅니다.
            Apply();
            StopTicking();
        }

        private void FinishDrag()
        {
            bool wasDragging = _dragging;

            _dragging = false;
            _pointerId = PointerId.invalidPointerId;

            if (!wasDragging)
            {
                // .. 임계값을 넘지 않았으면 클릭이었으므로 아무것도 하지 않습니다.
                _velocity = Vector2.zero;
                return;
            }

            if (!Inertia) _velocity = Vector2.zero;
            StartTicking();
        }

        private void StartTicking()
        {
            if (_scrollView == null) return;

            _lastTickTime = Time.realtimeSinceStartup;
            if (_tickHandle == null) _tickHandle = _scrollView.schedule.Execute(Tick).Every(16);
            else _tickHandle.Resume();
        }

        private void StopTicking() => _tickHandle?.Pause();

        // .. 손을 뗀 뒤 경계 복귀(고무줄)와 관성 감속을 처리합니다. 둘 다 끝나면 스스로 멈춥니다.
        private void Tick()
        {
            if (_scrollView == null) return;

            if (_dragging)
            {
                // .. 드래그 중에는 물리를 돌리지 않고, 쌓인 포인터 이동을 프레임당 한 번만 반영합니다.
                if (!_applyPending) return;

                _applyPending = false;
                Apply();
                return;
            }

            float now = Time.realtimeSinceStartup;
            float dt = Mathf.Clamp(now - _lastTickTime, 0.001f, 0.1f);
            _lastTickTime = now;

            Vector2 clamped = ClampToRange(_virtualOffset);

            // .. 두 축을 반드시 따로 계산합니다. 합쳐서 판정하면 한 축만 경계를 벗어나도 스프링백
            // .. 분기를 타면서 두 축의 속도를 모두 0으로 만들어, 다른 축의 관성이 사라집니다.
            // .. 특히 VerticalAndHorizontal이면서 콘텐츠가 한 축으로는 넘치지 않는 경우(그 축의
            // .. 스크롤 범위가 0이라 조금만 끌려도 항상 경계 밖) 반대 축 관성이 아예 동작하지
            // .. 않습니다.
            bool horizontalActive = StepAxis(ref _virtualOffset.x, ref _velocity.x, clamped.x, dt);
            bool verticalActive = StepAxis(ref _virtualOffset.y, ref _velocity.y, clamped.y, dt);

            Apply();

            if (!horizontalActive && !verticalActive) StopTicking();
        }

        // .. 한 축의 경계 복귀와 관성 감속을 처리하고, 아직 움직임이 남아 있으면 true를 돌려줍니다.
        private bool StepAxis(ref float offset, ref float velocity, float clamped, float dt)
        {
            float overshoot = offset - clamped;

            if (Mathf.Abs(overshoot) > 0.01f)
            {
                // .. 경계를 벗어난 축은 관성을 버리고 경계로 되돌립니다.
                velocity = 0f;
                offset = Mathf.Lerp(offset, clamped, SpringSpeed);
                if (Mathf.Abs(offset - clamped) <= 0.01f) offset = clamped;

                return true;
            }

            if (Inertia && Mathf.Abs(velocity) > 1f)
            {
                offset += velocity * dt;
                velocity *= Mathf.Pow(DecelerationRate, dt);

                return true;
            }

            velocity = 0f;
            offset = clamped;

            return false;
        }

        // .. 범위 안은 scrollOffset으로, 범위를 넘어선 초과분은 contentContainer의 transform으로
        // .. 표현합니다. scrollOffset 자체는 ScrollView가 범위로 clamp하기 때문입니다.
        private void Apply()
        {
            Vector2 clamped = ClampToRange(_virtualOffset);
            Vector2 visibleOvershoot = (_virtualOffset - clamped) * Elasticity;

            // .. 탄성 구간에서는 clamped가 경계에 고정이라 값이 그대로입니다. 같은 값을 다시 쓰면
            // .. ScrollView가 스크롤러까지 불필요하게 갱신하므로 바뀔 때만 씁니다.
            if (_scrollView.scrollOffset != clamped) _scrollView.scrollOffset = clamped;

            // .. ScrollView는 스크롤 위치를 contentContainer의 translate에 -scrollOffset으로
            // .. 반영합니다. 초과분만 따로 더하면 ScrollView의 갱신과 서로 덮어쓰므로, 스크롤분과
            // .. 초과분을 합친 절대값을 한 번에 씁니다(상대값 누적 방식은 scrollOffset이 그대로일
            // .. 때 매 프레임 쌓이므로 쓰지 않습니다).
            _scrollView.contentContainer.style.translate = new Translate(
                -clamped.x - visibleOvershoot.x,
                -clamped.y - visibleOvershoot.y);
        }

        private Vector2 ClampToRange(Vector2 offset)
        {
            Vector2 max = MaxOffset();

            return new Vector2(
                Mathf.Clamp(offset.x, 0f, max.x),
                Mathf.Clamp(offset.y, 0f, max.y));
        }

        // .. ScrollView가 실제로 사용하는 스크롤 범위를 그대로 씁니다. contentContainer와 뷰포트의
        // .. resolvedStyle 차이로 직접 계산하면, contentContainer가 뷰포트 크기에 맞춰 늘어난
        // .. 경우 범위가 0에 가깝게 나와 드래그할 때마다 콘텐츠가 최상단으로 끌려갑니다.
        // .. 요소가 두 스크롤바 중 하나에 속하는지 확인합니다. ScrollView까지 올라오면 스크롤바가
        // .. 아니므로 거기서 멈춥니다.
        private bool IsWithinScrollers(VisualElement element)
        {
            for (VisualElement current = element; current != null; current = current.parent)
            {
                if (current == _scrollView) return false;
                if (current == _scrollView.horizontalScroller || current == _scrollView.verticalScroller) return true;
            }

            return false;
        }

        private Vector2 MaxOffset()
        {
            if (_scrollView == null) return Vector2.zero;

            return new Vector2(
                Mathf.Max(0f, _scrollView.horizontalScroller.highValue),
                Mathf.Max(0f, _scrollView.verticalScroller.highValue));
        }
    }
}

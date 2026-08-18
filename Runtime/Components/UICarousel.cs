using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Jeomseon.Unity.UI.Components
{
    // .. 옛 HorizontalSelector(버튼 전용 페이지 전환)와 HorizontalEnumeratedItem(드래그 스냅 캐러셀)을
    // .. 하나로 통합했습니다. Draggable로 드래그 지원 여부만 선택합니다.
    [UxmlElement]
    public sealed partial class UICarousel : VisualElement
    {
        [UxmlAttribute] public bool Draggable { get; set; } = true;
        [UxmlAttribute] public float ItemHeightRatio { get; set; } = 1f;
        [UxmlAttribute] public float ItemWidthToHeightRatio { get; set; } = 1f;
        [UxmlAttribute] public float SpacingRatio { get; set; } = 0.1f;
        [UxmlAttribute] public float Elasticity { get; set; } = 0.3f;
        [UxmlAttribute] public float SnapSpeed { get; set; } = 0.3f;

        public event Action<int> SelectedIndexChanged;

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                var clamped = Mathf.Clamp(value, 0, Mathf.Max(0, _content.childCount - 1));
                if (clamped == _selectedIndex) return;

                SetSelectedIndexWithoutNotify(clamped);
                SelectedIndexChanged?.Invoke(_selectedIndex);
            }
        }

        private readonly VisualElement _content;
        private IVisualElementScheduledItem _tickHandle;

        private int _selectedIndex;
        private float _currentOffset;
        private float _targetOffset;

        private bool _dragging;
        private float _dragStartPointerX;
        private float _dragStartOffset;

        public UICarousel()
        {
            style.overflow = Overflow.Hidden;

            _content = new VisualElement { name = "content" };
            _content.style.flexDirection = FlexDirection.Row;
            hierarchy.Add(_content);

            RegisterCallback<GeometryChangedEvent>(_ => Reflow());
            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                if (_tickHandle == null) _tickHandle = schedule.Execute(Tick).Every(16);
                else _tickHandle.Resume();
            });
            RegisterCallback<DetachFromPanelEvent>(_ => _tickHandle?.Pause());

            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<PointerCancelEvent>(OnPointerCancel);
            RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
        }

        public void SetSelectedIndexWithoutNotify(int index)
        {
            _selectedIndex = Mathf.Clamp(index, 0, Mathf.Max(0, _content.childCount - 1));
            _targetOffset = GetOffsetForIndex(_selectedIndex);
        }

        public void SelectPrevious() => SelectedIndex -= 1;
        public void SelectNext() => SelectedIndex += 1;

        public void AddItem(VisualElement item)
        {
            _content.Add(item);
            Reflow();
        }

        public void AddItems(IEnumerable<VisualElement> items)
        {
            foreach (var item in items) _content.Add(item);
            Reflow();
        }

        public void ClearItems()
        {
            _content.Clear();
            Reflow();
        }

        private void Reflow()
        {
            if (float.IsNaN(resolvedStyle.height) || float.IsNaN(resolvedStyle.width)) return;

            // .. 높이가 0이면 아이템 크기도 0이 되어 전부 한 점에 겹칩니다. 레이아웃이 아직
            // .. 확정되지 않았거나 부모 flex가 이 영역을 눌러 없앤 상태이므로, 기존 크기를
            // .. 유지하고 다음 GeometryChangedEvent를 기다립니다.
            if (resolvedStyle.height <= 0f) return;

            var itemHeight = resolvedStyle.height * ItemHeightRatio;
            var itemWidth = itemHeight * ItemWidthToHeightRatio;
            var spacing = itemWidth * SpacingRatio;

            foreach (var item in _content.Children())
            {
                item.style.width = itemWidth;
                item.style.height = itemHeight;
                item.style.marginRight = spacing;
            }

            _targetOffset = GetOffsetForIndex(_selectedIndex);
            if (!_dragging)
            {
                _currentOffset = _targetOffset;
                _content.style.translate = new Translate(_currentOffset, 0);
            }
        }

        private float GetOffsetForIndex(int index)
        {
            var count = _content.childCount;
            if (count == 0) return 0f;

            var stride = GetItemStride();
            var itemWidth = stride / (1f + SpacingRatio);
            var itemCenter = index * stride + itemWidth * 0.5f;

            return resolvedStyle.width * 0.5f - itemCenter;
        }

        private int GetNearestIndex(float offset)
        {
            var count = _content.childCount;
            var stride = GetItemStride();
            if (count == 0 || stride <= 0f) return _selectedIndex;

            var itemWidth = stride / (1f + SpacingRatio);
            var viewportCenter = resolvedStyle.width * 0.5f;
            var rawIndex = (viewportCenter - offset - itemWidth * 0.5f) / stride;

            return Mathf.Clamp(Mathf.RoundToInt(rawIndex), 0, count - 1);
        }

        private float GetItemStride()
        {
            var itemHeight = resolvedStyle.height * ItemHeightRatio;
            var itemWidth = itemHeight * ItemWidthToHeightRatio;

            return itemWidth * (1f + SpacingRatio);
        }

        private float ApplyElasticity(float rawOffset)
        {
            var maxOffset = GetOffsetForIndex(0);
            var minOffset = GetOffsetForIndex(_content.childCount - 1);

            if (rawOffset > maxOffset) return maxOffset + (rawOffset - maxOffset) * Elasticity;
            if (rawOffset < minOffset) return minOffset + (rawOffset - minOffset) * Elasticity;

            return rawOffset;
        }

        private void Tick()
        {
            if (_dragging) return;

            _currentOffset = Mathf.Lerp(_currentOffset, _targetOffset, SnapSpeed);
            _content.style.translate = new Translate(_currentOffset, 0);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (!Draggable) return;

            _dragging = true;
            _dragStartPointerX = evt.position.x;
            _dragStartOffset = _currentOffset;
            this.CapturePointer(evt.pointerId);
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_dragging || !this.HasPointerCapture(evt.pointerId)) return;

            var delta = evt.position.x - _dragStartPointerX;
            _currentOffset = ApplyElasticity(_dragStartOffset + delta);
            _content.style.translate = new Translate(_currentOffset, 0);
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!_dragging) return;

            this.ReleasePointer(evt.pointerId);
            FinishDrag();
        }

        private void OnPointerCancel(PointerCancelEvent evt)
        {
            if (!_dragging) return;

            FinishDrag();
        }

        private void OnPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            if (!_dragging) return;

            FinishDrag();
        }

        private void FinishDrag()
        {
            _dragging = false;
            SelectedIndex = GetNearestIndex(_currentOffset);
        }
    }
}

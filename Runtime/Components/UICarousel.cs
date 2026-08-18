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
                int clamped = Mathf.Clamp(value, 0, Mathf.Max(0, _content.childCount - 1));
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
            foreach (VisualElement item in items) _content.Add(item);
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

            float itemHeight = resolvedStyle.height * ItemHeightRatio;
            float itemWidth = itemHeight * ItemWidthToHeightRatio;
            float spacing = itemWidth * SpacingRatio;

            foreach (VisualElement item in _content.Children())
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
            int count = _content.childCount;
            if (count == 0) return 0f;

            float stride = GetItemStride();
            float itemWidth = stride / (1f + SpacingRatio);
            float itemCenter = index * stride + itemWidth * 0.5f;

            return resolvedStyle.width * 0.5f - itemCenter;
        }

        private int GetNearestIndex(float offset)
        {
            int count = _content.childCount;
            float stride = GetItemStride();
            if (count == 0 || stride <= 0f) return _selectedIndex;

            float itemWidth = stride / (1f + SpacingRatio);
            float viewportCenter = resolvedStyle.width * 0.5f;
            float rawIndex = (viewportCenter - offset - itemWidth * 0.5f) / stride;

            return Mathf.Clamp(Mathf.RoundToInt(rawIndex), 0, count - 1);
        }

        private float GetItemStride()
        {
            float itemHeight = resolvedStyle.height * ItemHeightRatio;
            float itemWidth = itemHeight * ItemWidthToHeightRatio;

            return itemWidth * (1f + SpacingRatio);
        }

        private float ApplyElasticity(float rawOffset)
        {
            float maxOffset = GetOffsetForIndex(0);
            float minOffset = GetOffsetForIndex(_content.childCount - 1);

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

            float delta = evt.position.x - _dragStartPointerX;
            _currentOffset = ApplyElasticity(_dragStartOffset + delta);
            _content.style.translate = new Translate(_currentOffset, 0);
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!_dragging) return;

            _dragging = false;
            this.ReleasePointer(evt.pointerId);

            SelectedIndex = GetNearestIndex(_currentOffset);
        }
    }
}

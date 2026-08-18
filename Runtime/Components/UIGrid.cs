using UnityEngine;
using UnityEngine.UIElements;

namespace Jeomseon.Unity.UI.Components
{
    // .. GridLayoutGroup 없이, 콘텐츠 너비 대비 비율로 셀 크기·간격·패딩을 계산하는 반응형 그리드입니다.
    // .. 옛 EnumeratedElements(uGUI)를 대체합니다.
    [UxmlElement]
    public sealed partial class UIGrid : VisualElement
    {
        private int _columnCount = 3;
        private float _itemWidthToHeightRatio = 1f;
        private float _itemSpacingRatio = 0.1f;
        private float _paddingRatio;

        // .. 아래 setter들은 계산 결과를 바꾸므로 캐시를 무효화하고 즉시 다시 배치합니다.
        // .. 그래야 UI Builder에서 값을 바꿨을 때 바로 반영됩니다.
        [UxmlAttribute]
        public int ColumnCount
        {
            get => _columnCount;
            set { _columnCount = value; InvalidateLayout(); }
        }

        [UxmlAttribute]
        public float ItemWidthToHeightRatio
        {
            get => _itemWidthToHeightRatio;
            set { _itemWidthToHeightRatio = value; InvalidateLayout(); }
        }

        [UxmlAttribute]
        public float ItemSpacingRatio
        {
            get => _itemSpacingRatio;
            set { _itemSpacingRatio = value; InvalidateLayout(); }
        }

        [UxmlAttribute]
        public float PaddingRatio
        {
            get => _paddingRatio;
            set { _paddingRatio = value; InvalidateLayout(); }
        }

        // .. 마지막으로 반영한 입력값입니다. 같은 값이면 style을 다시 쓰지 않습니다.
        private float _lastContentWidth = float.NaN;
        private int _lastChildCount = -1;

        public UIGrid()
        {
            style.flexDirection = FlexDirection.Row;
            style.flexWrap = Wrap.Wrap;

            RegisterCallback<GeometryChangedEvent>(_ => Reflow());
        }

        public void AddItem(VisualElement item)
        {
            Add(item);
            Reflow();
        }

        private void InvalidateLayout()
        {
            _lastContentWidth = float.NaN;
            Reflow();
        }

        public void Reflow()
        {
            var contentWidth = resolvedStyle.width;
            if (float.IsNaN(contentWidth) || ColumnCount <= 0) return;

            // .. style을 쓰면 레이아웃이 dirty가 되고, 그 결과 GeometryChangedEvent가 다시 올 수
            // .. 있습니다. 폭과 아이템 수가 그대로면 결과가 같으므로 다시 쓰지 않습니다. 이 가드가
            // .. 없으면 스크롤처럼 잦은 레이아웃 갱신마다 모든 자식의 style을 재작성하게 됩니다.
            if (Mathf.Approximately(contentWidth, _lastContentWidth) && childCount == _lastChildCount) return;

            _lastContentWidth = contentWidth;
            _lastChildCount = childCount;

            var padding = contentWidth * PaddingRatio;
            style.paddingLeft = padding;
            style.paddingRight = padding;
            style.paddingTop = padding;
            style.paddingBottom = padding;

            // .. 가로 폭과 간격은 퍼센트로 지정합니다. 퍼센트는 wrap 판정이 사용하는 것과 같은
            // .. 콘텐츠 박스를 기준으로 풀리므로, 픽셀로 계산할 때처럼 반올림 오차나 테마 기본
            // .. 여백 때문에 마지막 열이 다음 줄로 밀려나지 않습니다. 한 행 합계는
            // .. ColumnCount * width + (ColumnCount - 1) * spacing = 100% - spacing 으로 항상
            // .. 100% 미만입니다.
            var stridePercent = 100f / ColumnCount;
            var spacingPercent = stridePercent * ItemSpacingRatio;
            var widthPercent = stridePercent - spacingPercent;

            // .. 세로 값은 폭 기준 비율이라 퍼센트로 표현할 수 없어 픽셀로 계산합니다.
            var availableWidth = contentWidth - padding * 2f;
            var itemWidth = availableWidth * widthPercent * 0.01f;
            var itemHeight = itemWidth / ItemWidthToHeightRatio;
            var rowSpacing = availableWidth * spacingPercent * 0.01f;

            var index = 0;
            foreach (var item in Children())
            {
                // .. 각 행의 마지막 열에는 오른쪽 여백을 붙이지 않습니다.
                var isLastInRow = (index + 1) % ColumnCount == 0;

                item.style.width = Length.Percent(widthPercent);
                item.style.height = itemHeight;
                // .. 테마 기본 여백이 폭 계산에 끼어들지 않도록 명시적으로 지웁니다.
                item.style.marginLeft = 0f;
                item.style.marginTop = 0f;
                item.style.marginRight = Length.Percent(isLastInRow ? 0f : spacingPercent);
                item.style.marginBottom = rowSpacing;
                index++;
            }
        }
    }
}

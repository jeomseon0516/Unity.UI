using UnityEngine.UIElements;

namespace Jeomseon.Unity.UI.Components
{
    // .. GridLayoutGroup 없이, 콘텐츠 너비 대비 비율로 셀 크기·간격·패딩을 계산하는 반응형 그리드입니다.
    // .. 옛 EnumeratedElements(uGUI)를 대체합니다.
    [UxmlElement]
    public sealed partial class UIGrid : VisualElement
    {
        [UxmlAttribute] public int ColumnCount { get; set; } = 3;
        [UxmlAttribute] public float ItemWidthToHeightRatio { get; set; } = 1f;
        [UxmlAttribute] public float ItemSpacingRatio { get; set; } = 0.1f;
        [UxmlAttribute] public float PaddingRatio { get; set; }

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

        public void Reflow()
        {
            float contentWidth = resolvedStyle.width;
            if (float.IsNaN(contentWidth) || ColumnCount <= 0) return;

            float padding = contentWidth * PaddingRatio;
            style.paddingLeft = padding;
            style.paddingRight = padding;
            style.paddingTop = padding;
            style.paddingBottom = padding;

            float availableWidth = contentWidth - padding * 2f;
            float itemStride = availableWidth / ColumnCount;
            float spacing = itemStride * ItemSpacingRatio;
            float itemWidth = itemStride - spacing;
            float itemHeight = itemWidth * ItemWidthToHeightRatio;

            foreach (VisualElement item in Children())
            {
                item.style.width = itemWidth;
                item.style.height = itemHeight;
                item.style.marginRight = spacing;
                item.style.marginBottom = spacing;
            }
        }
    }
}

using Jeomseon.Unity.UI;
using Jeomseon.Unity.UI.Components;
using UnityEngine.UIElements;

namespace Jeomseon.Samples.UI
{
    [UxmlElement]
    public sealed partial class HomeView : UIView
    {
        private Label _selectionLabel;

        protected override void OnScreenCreated()
        {
            _selectionLabel = this.Q<Label>("selection-label");

            var carousel = new UICarousel
            {
                name = "sample-carousel",
                Draggable = true,
                ItemHeightRatio = 0.8f,
                ItemWidthToHeightRatio = 1.4f,
                SpacingRatio = 0.08f
            };
            carousel.AddToClassList("sample-carousel");

            for (var index = 0; index < 5; index++)
            {
                var item = new Label($"Item {index + 1}");
                item.AddToClassList("carousel-item");
                carousel.AddItem(item);
            }

            carousel.SelectedIndexChanged += index => _selectionLabel.text = $"Selected: {index + 1}";
            this.Q<Button>("previous-button").clicked += carousel.SelectPrevious;
            this.Q<Button>("next-button").clicked += carousel.SelectNext;
            this.Q<Button>("popup-button").clicked += Channel.RequestOpen<PopupView>;
            this.Q<Button>("loading-button").clicked += Channel.RequestOpen<LoadingView>;
            this.Q("carousel-host").Add(carousel);

            var grid = new UIGrid
            {
                ColumnCount = 4,
                ItemWidthToHeightRatio = 1f,
                ItemSpacingRatio = 0.04f,
                PaddingRatio = 0.03f
            };
            grid.AddToClassList("sample-grid");

            for (var index = 0; index < 12; index++)
            {
                var cell = new Label((index + 1).ToString());
                cell.AddToClassList("grid-item");
                grid.AddItem(cell);
            }

            this.Q("grid-host").Add(grid);
        }
    }
}

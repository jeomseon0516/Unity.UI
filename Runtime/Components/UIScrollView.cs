using UnityEngine.UIElements;

namespace Jeomseon.Unity.UI.Components
{
    // .. ScrollDragManipulator를 기본으로 붙인 ScrollView입니다. UXML/UI Builder에서 바로 배치할
    // .. 수 있으며, 마우스·터치 구분 없이 드래그 스크롤과 고무줄 오버스크롤, 관성이 동작합니다.
    // .. 기존 ScrollView나 ListView에 같은 동작이 필요하면 ScrollDragManipulator를 직접 붙입니다.
    // .. 속성 이름에 Drag 접두사를 두어 ScrollView가 이미 가진 elasticity·scrollDecelerationRate
    // .. (둘 다 터치 전용)와 UXML 속성 이름이 겹치지 않게 합니다.
    [UxmlElement]
    public sealed partial class UIScrollView : ScrollView
    {
        private readonly ScrollDragManipulator _dragManipulator = new();

        [UxmlAttribute]
        public float DragElasticity
        {
            get => _dragManipulator.Elasticity;
            set => _dragManipulator.Elasticity = value;
        }

        [UxmlAttribute]
        public float DragSpringSpeed
        {
            get => _dragManipulator.SpringSpeed;
            set => _dragManipulator.SpringSpeed = value;
        }

        [UxmlAttribute]
        public float DragDecelerationRate
        {
            get => _dragManipulator.DecelerationRate;
            set => _dragManipulator.DecelerationRate = value;
        }

        [UxmlAttribute]
        public float DragThreshold
        {
            get => _dragManipulator.DragThreshold;
            set => _dragManipulator.DragThreshold = value;
        }

        [UxmlAttribute]
        public bool DragInertia
        {
            get => _dragManipulator.Inertia;
            set => _dragManipulator.Inertia = value;
        }

        // .. uGUI ScrollRect의 Horizontal/Vertical 체크박스에 해당합니다. 단 축 허용의 1차 기준은
        // .. ScrollView의 mode이며, 이 값은 거기서 더 좁히는 역할만 합니다(mode가 Vertical이면
        // .. DragHorizontal을 켜도 가로로 끌리지 않습니다).
        [UxmlAttribute]
        public bool DragHorizontal
        {
            get => _dragManipulator.DragHorizontal;
            set => _dragManipulator.DragHorizontal = value;
        }

        [UxmlAttribute]
        public bool DragVertical
        {
            get => _dragManipulator.DragVertical;
            set => _dragManipulator.DragVertical = value;
        }

        public UIScrollView() => this.AddManipulator(_dragManipulator);
    }
}

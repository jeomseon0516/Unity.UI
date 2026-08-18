using Jeomseon.Unity.UI.Channels;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Jeomseon.Unity.UI.Editor
{
    // .. Play Mode 없이 UIStackManager를 선택하면 Edit Mode에서 즉시 카탈로그를 미리 볼 수 있게
    // .. 합니다. UIStackManager 런타임 코드는 건드리지 않고 기존 Initialize/ClearCatalog만
    // .. Editor에서 호출합니다.
    [CustomEditor(typeof(UIStackManager))]
    public sealed class UIStackManagerEditor : UnityEditor.Editor
    {
        private UIStackManager _manager;

        // .. Play Mode 진입 전환 중에는 Application.isPlaying이 아직 false인데 UIDocument는 이미
        // .. 정리돼 rootVisualElement가 null입니다. 이때 미리보기를 만들거나 지우면 진입 직후
        // .. Awake가 구성한 카탈로그를 건드리게 되므로 전환 상태 전체를 제외합니다.
        private static bool CanPreview
            => !Application.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode;

        private void OnEnable()
        {
            _manager = (UIStackManager)target;
            Preview();
        }

        private void OnDisable()
        {
            if (!_manager || !CanPreview) return;

            _manager.ClearCatalog();
            InternalEditorUtility.RepaintAllViews();
        }

        public override void OnInspectorGUI()
        {
            bool changed = DrawDefaultInspector();

            if (!CanPreview) return;

            if (changed) Preview();

            EditorGUILayout.HelpBox(
                "Selecting this object previews its catalog in Edit Mode. Deselect to clear the preview.",
                MessageType.Info);
        }

        private void Preview()
        {
            if (!CanPreview) return;

            var document = serializedObject.FindProperty("document").objectReferenceValue as UIDocument;
            if (!document || document.rootVisualElement == null) return;

            var channel = serializedObject.FindProperty("channel").objectReferenceValue as UIChannel;
            _manager.Initialize(channel);
            ShowFirstScreen(document);
            InternalEditorUtility.RepaintAllViews();
        }

        // .. 등록된 화면은 전부 SetVisible(false) 상태로 시작하고, 실제로 화면을 여는 것은
        // .. Play Mode에서만 도는 채널 요청입니다. 미리보기에서는 Screen 레이어의 첫 화면만
        // .. 직접 표시합니다. 채널 요청 대신 직접 표시하는 이유는 Edit Mode에서 소비자 콜백
        // .. (ScreenOpened 등)을 실행시키지 않기 위해서입니다.
        private static void ShowFirstScreen(UIDocument document)
        {
            VisualElement layer = document.rootVisualElement.Q(UILayer.Screen.ToString());
            layer?.Q<UIView>()?.SetVisible(true);
        }
    }
}

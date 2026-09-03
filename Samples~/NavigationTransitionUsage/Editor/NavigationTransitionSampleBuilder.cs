#if UNITY_EDITOR
using System;
using System.IO;
using Jeomseon.Unity.UI;
using Jeomseon.Unity.UI.Channels;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Jeomseon.Samples.UI.Editor
{
    /// <summary>
    /// <c>Samples~</c>에 커밋할 Scene·자산을 코드로 생성합니다. 배치모드에서
    /// <c>-executeMethod Jeomseon.Samples.UI.Editor.NavigationTransitionSampleBuilder.Build</c>로
    /// 호출하거나, 메뉴로 실행한 뒤 결과물(<c>.unity</c>/<c>.asset</c>)을 패키지 폴더로 옮깁니다.
    /// </summary>
    public static class NavigationTransitionSampleBuilder
    {
        private const string Dir = "Assets/NavigationTransitionUsage";

        [MenuItem("Jeomseon/Tool/UI/Build Navigation Transition Sample Scene")]
        public static void Build()
        {
            Directory.CreateDirectory(Dir);

            // 빈 씬을 먼저 연다. 자산 생성 뒤 NewScene을 호출하면 배치모드에서
            // 방금 만든 자산 래퍼가 해제되어 참조가 null로 직렬화되는 사례가 있다.
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // --- 자산 생성 ---------------------------------------------------
            // CreateAsset는 인스턴스를 즉시 영속화하므로, Refresh/재로드 없이
            // 이 참조를 그대로 씬 컴포넌트에 물린다(외부 자산 참조로 직렬화됨).
            var panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            // 샘플과 함께 커밋되는 테마(unity 기본 런타임 테마를 @import). 프로젝트 로컬
            // UnityDefaultRuntimeTheme.tss는 프로젝트마다 GUID가 달라 설치처에서 참조가 깨지므로 쓰지 않는다.
            var theme = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>($"{Dir}/NavigationTransitionSample.tss");
            if (theme == null)
            {
                string[] themes = AssetDatabase.FindAssets("t:ThemeStyleSheet");
                if (themes.Length > 0)
                    theme = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(
                        AssetDatabase.GUIDToAssetPath(themes[0]));
            }
            panelSettings.themeStyleSheet = theme;
            AssetDatabase.CreateAsset(panelSettings, $"{Dir}/PanelSettings.asset");

            var channel = ScriptableObject.CreateInstance<UIChannel>();
            AssetDatabase.CreateAsset(channel, $"{Dir}/UIChannel.asset");

            var catalog = ScriptableObject.CreateInstance<UICatalog>();
            var catalogSo = new SerializedObject(catalog);
            SerializedProperty entries = catalogSo.FindProperty("entries");
            AddEntry(entries, $"{Dir}/MenuScreen.uxml");
            AddEntry(entries, $"{Dir}/DetailScreen.uxml");
            AddEntry(entries, $"{Dir}/SettingsScreen.uxml");
            catalogSo.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(catalog, $"{Dir}/UICatalog.asset");

            // --- 씬 구성 ---------------------------------------------------
            var cameraGo = new GameObject("Main Camera", typeof(Camera));
            cameraGo.tag = "MainCamera";
            Camera camera = cameraGo.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;

            // EventSystem은 UI Toolkit 런타임 입력에 필요합니다. Input System / 레거시 어느 쪽이든
            // 프로젝트에 있는 것을 씁니다(이 Sample asmdef에 입력 패키지 의존성을 강제하지 않으려고
            // 타입 이름으로 붙입니다).
            var eventSystemType = Type.GetType("UnityEngine.EventSystems.EventSystem, UnityEngine.UI");
            if (eventSystemType != null)
            {
                var eventSystemGo = new GameObject("EventSystem", eventSystemType);
                Type inputModule =
                    Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem")
                    ?? Type.GetType("UnityEngine.EventSystems.StandaloneInputModule, UnityEngine.UI");
                if (inputModule != null) eventSystemGo.AddComponent(inputModule);
            }

            var uiGo = new GameObject("UI");
            var document = uiGo.AddComponent<UIDocument>();
            document.panelSettings = panelSettings;

            var manager = uiGo.AddComponent<UIStackManager>();
            var managerSo = new SerializedObject(manager);
            managerSo.FindProperty("document").objectReferenceValue = document;
            managerSo.FindProperty("catalog").objectReferenceValue = catalog;
            managerSo.FindProperty("channel").objectReferenceValue = channel;
            managerSo.ApplyModifiedPropertiesWithoutUndo();

            var sample = uiGo.AddComponent<NavigationTransitionSample>();
            var sampleSo = new SerializedObject(sample);
            sampleSo.FindProperty("document").objectReferenceValue = document;
            sampleSo.FindProperty("channel").objectReferenceValue = channel;
            sampleSo.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, $"{Dir}/NavigationTransitionSample.unity");
            AssetDatabase.SaveAssets();

            // 검증: 참조가 실제로 물렸는지 배치 로그로 확인한다.
            var check = new SerializedObject(uiGo.GetComponent<UIStackManager>());
            Debug.Log($"[Builder] manager.catalog={check.FindProperty("catalog").objectReferenceValue} " +
                      $"manager.channel={check.FindProperty("channel").objectReferenceValue} " +
                      $"panelSettings={document.panelSettings}");
            Debug.Log($"NavigationTransition sample built under {Dir}");
        }

        private static void AddEntry(SerializedProperty entries, string uxmlPath)
        {
            var layout = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            entries.arraySize++;
            SerializedProperty entry = entries.GetArrayElementAtIndex(entries.arraySize - 1);
            entry.FindPropertyRelative("layer").enumValueIndex = (int)UILayer.Screen;
            entry.FindPropertyRelative("layout").objectReferenceValue = layout;
        }
    }
}
#endif

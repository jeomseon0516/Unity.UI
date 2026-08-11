#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Jeomseon.UI.Components;
using static Jeomseon.Editor.GUI.EditorGUILayoutActions;
using static Jeomseon.Editor.SerializedPropertyReflection;
using static Jeomseon.UI.Components.HorizontalEnumeratedItem;

namespace Jeomseon.UI.Editor
{
[CustomEditor(typeof(HorizontalEnumeratedItem))]
internal sealed class HorizontalEnumeratedItemEditor : UnityEditor.Editor
{
    private HorizontalEnumeratedItem _horizontalEnumeratedItem;

    private SerializedProperty _viewport;
    private SerializedProperty _content;

    private SerializedProperty _spacingRatio;
    private SerializedProperty _itemHeightRatioFromContentHeight;
    private SerializedProperty _itemWidthRatioFromHeight;
    private SerializedProperty _onPointerUpCorrection;
    private SerializedProperty _elasticity;

    private SerializedProperty _selectedIndex;
    private SerializedProperty _onChangedValue;

    [MenuItem("GameObject/Create/UI/Horizontal Enumerated Item")]
    public static void CreateHorizontalEnumeratedItem(MenuCommand menuCommand)
    {
        GameObject horizontalEnumeratedPrefab = Resources.Load<GameObject>("HorizontalEnumeratedItem");
        GameObject newUIInstance = Instantiate(horizontalEnumeratedPrefab, (menuCommand.context as GameObject)?.transform);
        newUIInstance.name = "HorizontalEnumeratedItem";

        Undo.RegisterCreatedObjectUndo(newUIInstance, "Create Horizontal Enumerated Item");
        Selection.activeObject = newUIInstance;
    }

    private void OnEnable()
    {
        _horizontalEnumeratedItem = target as HorizontalEnumeratedItem;
        _viewport = serializedObject.FindProperty(GetBackingFieldName("Viewport"));
        _content = serializedObject.FindProperty(GetBackingFieldName("Content"));
        _onChangedValue = serializedObject.FindProperty(GetBackingFieldName("OnChangedValue"));
        _spacingRatio = serializedObject.FindProperty("spacingRatio");
        _itemHeightRatioFromContentHeight = serializedObject.FindProperty("itemHeightRatioFromContentHeight");
        _itemWidthRatioFromHeight = serializedObject.FindProperty("itemWidthRatioFromHeight");
        _onPointerUpCorrection = serializedObject.FindProperty("onPointerUpCorrection");
        _elasticity = serializedObject.FindProperty("elasticity");
        _selectedIndex = serializedObject.FindProperty("selectedIndex");
    }

    public override void OnInspectorGUI()
    {
        using (new EditorGUILayout.VerticalScope(GUI.skin.box))
        {
            using EditorGUI.ChangeCheckScope contentChangeScope = new();
            EditorGUILayout.PropertyField(
                _viewport,
                new("Viewport", "열거된 아이템을 보여줄 UI 입니다 RectMask2D와 EventTrigger를 사용해주세요"), true);
            EditorGUILayout.PropertyField(
                _content,
                new("Content", "실제 열거되어있는 아이템을 보관할 컨텐츠 입니다. 스크롤에 관한 기능은 컨텐츠를 사용합니다."), true);

            if (contentChangeScope.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
        
        serializedObject.Update();

        using EditorGUI.ChangeCheckScope optionChangeScope = new();
        if (_viewport.objectReferenceValue && _content.objectReferenceValue)
        {
            RectTransform content = (RectTransform)_content.objectReferenceValue;

            EditorGUILayout.LabelField("Selected Index");
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                _selectedIndex.intValue = EditorGUILayout.IntSlider(_selectedIndex.intValue, 0, content.childCount - 1);
            }

            EditorGUILayout.LabelField("Content Option");
            using (EditorGUILayout.VerticalScope _ = new(GUI.skin.box))
            {
                EditorGUILayout.Slider(
                    _onPointerUpCorrection,
                    OnPointerUpCorrectionLimit.Min,
                    OnPointerUpCorrectionLimit.Max,
                    new GUIContent("OnPointerUpCorrection", "드래그 하다 놓을 시 중앙에 가까운 위치에 있는 아이템으로의 보정 속도"));

                EditorGUILayout.Slider(
                    _elasticity,
                    ElasticityLimit.Min,
                    ElasticityLimit.Max,
                    new GUIContent("Elasticity", "뷰포트 내부의 콘텐츠가 뷰포트 범위에서 벗어났을때 원래대로 돌아가려는 힘"));
            }

            EditorGUILayout.LabelField("Item Option");
            using (EditorGUILayout.VerticalScope _ = new(GUI.skin.box))
            {
                EditorGUILayout.Slider(
                    _spacingRatio,
                    SpacingRatioLimit.Min,
                    SpacingRatioLimit.Max,
                    new GUIContent("SpacingRatio", "각 아이템은 어느정도의 간격으로 배치될건지의 비율"));

                EditorGUILayout.Slider(
                    _itemHeightRatioFromContentHeight,
                    ItemHeightRatioFromContentHeightLimit.Min,
                    ItemHeightRatioFromContentHeightLimit.Max,
                    new GUIContent("ItemHeightRatioFromContentHeight", "아이템의 높이를 Content 높이의 기준의 비율로 정하는 값"));

                EditorGUILayout.Slider(
                    _itemWidthRatioFromHeight,
                    ItemWidthRatioFromHeightLimit.Min,
                    ItemWidthRatioFromHeightLimit.Max,
                    new GUIContent("ItemWidthRatioFromHeight", "아이템의 넓이를 높이 기준의 비율로 정하는 값"));

                if (!Application.isPlaying)
                {
                    _horizontalEnumeratedItem.Init((RectTransform)_viewport.objectReferenceValue, content);

                    if (content.childCount > 0)
                    {
                        content.localPosition = GetContentLocalPositionFromSelectedIndex(
                            _selectedIndex.intValue,
                            content.childCount,
                            content.sizeDelta.x);
                    }
                }
            }
        }

        EditorGUILayout.LabelField("Event");
        EditorGUILayout.PropertyField(_onChangedValue, new GUIContent("OnChangedValue", "선택된 아이템이 변경될시 해당 아이템의 인덱스를 이벤트로 넘겨줍니다"), true);

        if (optionChangeScope.changed)
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}
}
#endif

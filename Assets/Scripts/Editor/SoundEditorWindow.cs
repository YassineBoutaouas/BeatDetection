#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SoundEditorWindow : EditorWindow
{
    private const string _configuratorPath = "Assets/Scripts/Editor/UI/SoundEditor.uxml";
    private const string _configuratorStyle = "Assets/Scripts/Editor/UI/SoundEditor_Style.uss";

    [MenuItem("Sound/SoundEditor")]
    public static void OpenWindow()
    {
        SoundEditorWindow wnd = GetWindow<SoundEditorWindow>();
        wnd.titleContent = new GUIContent("Sound Editor");
    }

    public void CreateGUI()
    {
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(_configuratorPath);
        visualTree.CloneTree(rootVisualElement);

        StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(_configuratorStyle);
        rootVisualElement.styleSheets.Add(styleSheet);
    }
}
#endif
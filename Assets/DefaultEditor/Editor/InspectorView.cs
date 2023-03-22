#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class InspectorView : VisualElement
{
    public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits> { }

    private Editor editor;

    public InspectorView() { }

    public void UpdateSelection(UnityEngine.Object targetObj)
    {
        Clear();

        UnityEngine.Object.DestroyImmediate(editor);

        editor = Editor.CreateEditor(targetObj);
        IMGUIContainer container = new IMGUIContainer(() => 
        { 
            if(editor.target)
                editor.OnInspectorGUI(); 
        });
        Add(container);
    }
}
#endif
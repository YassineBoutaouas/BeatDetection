#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
public class Scrapper : Scroller
{
    public new class UxmlFactory : UxmlFactory<Scrapper, UxmlTraits> { }

    public new class UxmlTraits : Scroller.UxmlTraits
    {
        UxmlFloatAttributeDescription DraggerHeight = new UxmlFloatAttributeDescription { name = "BodyHeight", defaultValue = 1 };

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
        }
    }

    private const string _configuratorStyle = "Assets/Scripts/Editor/UI/Scrapper_Style.uss";

    public float dragger_height { get; set; }

    public Scrapper()
    {
        VisualElement dragger = this.Q<VisualElement>("unity-dragger");
        VisualElement draggerBody = new VisualElement { name = "dragger-body" };
        draggerBody.AddToClassList("dragger-body");

        VisualElement lowButton = this.Q<RepeatButton>("unity-low-button");
        VisualElement highButton = this.Q<RepeatButton>("unity-high-button");

        Remove(lowButton);
        Remove(highButton);

        Debug.Log(lowButton);

        dragger.Add(draggerBody);

        styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(_configuratorStyle));
    }

    
}
#endif
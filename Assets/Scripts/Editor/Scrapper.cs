#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        Remove(this.Q<RepeatButton>("unity-low-button"));
        Remove(this.Q<RepeatButton>("unity-high-button"));

        dragger.Add(draggerBody);

        dragger.name = "scrapper-dragger";

        VisualElement sliderParent = this.Q<VisualElement>("unity-slider");
        StyleEnum<Overflow> overflow = sliderParent.style.overflow;
        overflow.value = Overflow.Visible;
        sliderParent.style.overflow = overflow;

        draggerBody.SendToBack();

        VisualElement slidercontainer = sliderParent.Children().ElementAt(0);
        slidercontainer.name = "unity-slider-container";

        styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(_configuratorStyle));
    }
}
#endif
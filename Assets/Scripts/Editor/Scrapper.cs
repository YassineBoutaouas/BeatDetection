#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

/// <summary>
/// This class provides a scroller for the sound editor window
/// </summary>
public class Scrapper : Scroller
{
    public new class UxmlFactory : UxmlFactory<Scrapper, UxmlTraits> { }

    public new class UxmlTraits : Scroller.UxmlTraits
    {
        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
        }
    }

    private const string _configuratorStyle = "Assets/Scripts/Editor/UI/Scrapper_Style.uss";

    public VisualElement Dragger { get; }
    public VisualElement DraggerBody { get; }

    public Scrapper()
    {
        Dragger = this.Q<VisualElement>("unity-dragger");
        DraggerBody = new VisualElement { name = "dragger-body" };
        DraggerBody.AddToClassList("dragger-body");

        Remove(this.Q<RepeatButton>("unity-low-button"));
        Remove(this.Q<RepeatButton>("unity-high-button"));

        Dragger.Add(DraggerBody);

        Dragger.name = "scrapper-dragger";

        VisualElement sliderParent = this.Q<VisualElement>("unity-slider");
        StyleEnum<Overflow> overflow = sliderParent.style.overflow;
        overflow.value = Overflow.Visible;
        sliderParent.style.overflow = overflow;

        VisualElement slidercontainer = sliderParent.Children().ElementAt(0);
        slidercontainer.name = "unity-slider-container";

        styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(_configuratorStyle));
    }
}
#endif
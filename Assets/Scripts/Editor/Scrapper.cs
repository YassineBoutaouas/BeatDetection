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
}
#endif
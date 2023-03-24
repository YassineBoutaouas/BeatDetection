#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class DragAndDropManipulator : PointerManipulator
{
    private VisualElement _window;
    private VisualElement _draggerBody;
    private bool enabled { get; set; }

    public DragAndDropManipulator(VisualElement dragger, VisualElement draggerBody, VisualElement draggerWindow)
    {
        target = dragger;
        _draggerBody = draggerBody;
        _window = draggerWindow;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerDownEvent>(PointerDownHandler);
        target.RegisterCallback<PointerMoveEvent>(PointerMoveHandler);
        target.RegisterCallback<PointerUpEvent>(PointerUpHandler);
        target.RegisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<PointerDownEvent>(PointerDownHandler);
        target.UnregisterCallback<PointerMoveEvent>(PointerMoveHandler);
        target.UnregisterCallback<PointerUpEvent>(PointerUpHandler);
        target.UnregisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
    }

    private void PointerDownHandler(PointerDownEvent evt)
    {
        target.CapturePointer(evt.pointerId);
        enabled = true;
        _draggerBody.transform.scale = new Vector3(1, _window.resolvedStyle.scale.value.y); 
    }

    private void PointerMoveHandler(PointerMoveEvent evt)
    {
        if (!(enabled && target.HasPointerCapture(evt.pointerId))) return;

        _draggerBody.transform.scale = new Vector3(1, _window.resolvedStyle.scale.value.y);
    }

    private void PointerUpHandler(PointerUpEvent evt)
    {
        if (enabled && target.HasPointerCapture(evt.pointerId))
        {
            target.ReleasePointer(evt.pointerId);
        }
    }

    private void PointerCaptureOutHandler(PointerCaptureOutEvent evt)
    {
        enabled = false;
    }
}
#endif
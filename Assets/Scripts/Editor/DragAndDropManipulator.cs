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
    private VisualElement _dragger;
    private bool enabled { get; set; }

    public DragAndDropManipulator(VisualElement draggerBody, VisualElement dragger)
    {
        _dragger = dragger;
        target = draggerBody;
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
        target.transform.position = new Vector3(_dragger.transform.position.x, target.transform.position.y);
    }

    private void PointerMoveHandler(PointerMoveEvent evt)
    {
        if (!(enabled && target.HasPointerCapture(evt.pointerId))) return;

        target.transform.position = new Vector3(_dragger.transform.position.x, target.transform.position.y);
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
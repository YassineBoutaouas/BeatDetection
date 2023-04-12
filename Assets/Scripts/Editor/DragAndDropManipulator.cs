#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UIElements;

namespace SoundElements.Editor
{
    /// <summary>
    /// This class provides functionality to move a sound event and to change the time it is invoked
    /// </summary>
    public class DragAndDropManipulator : PointerManipulator
    {
        private VisualElement _referenceContainer;
        private ScrollView _scrollView;
        private SoundElement _soundElement;

        private bool _enabled;
        private Vector3 _pointerStartPosition;
        private Vector3 _targetStartPosition;

        public DragAndDropManipulator(EventElement soundevt, VisualElement referenceContainer, ScrollView scrollView, SoundElement soundElement)
        {
            target = soundevt;
            _referenceContainer = referenceContainer;
            _soundElement = soundElement;

            _scrollView = scrollView;
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
            if (!(evt.ctrlKey || evt.commandKey)) return;

            _targetStartPosition = target.transform.position;
            _pointerStartPosition = evt.position;
            target.CapturePointer(evt.pointerId);
            _enabled = true;
        }

        private void PointerMoveHandler(PointerMoveEvent evt)
        {
            if (!(_enabled && target.HasPointerCapture(evt.pointerId))) return;

            Vector3 pointerDelta = evt.position - _pointerStartPosition;
            float x = Mathf.Clamp(_targetStartPosition.x + pointerDelta.x, 0, _referenceContainer.worldBound.width - target.worldBound.width);

            target.transform.position = new Vector3(x, 0);

            _scrollView.ScrollTo(target);
        }

        private void PointerUpHandler(PointerUpEvent evt)
        {
            if (_enabled && target.HasPointerCapture(evt.pointerId))
            {
                Vector3 pointerDelta = evt.position - _pointerStartPosition;

                float pos = Mathf.Clamp(_targetStartPosition.x + pointerDelta.x, 0, _referenceContainer.worldBound.width - target.worldBound.width);
                float x = (pos / _referenceContainer.style.width.value.value) * _soundElement.AudioClip.length;

                ((EventElement)target)._soundEvent.TimeStamp = x;
                target.ReleasePointer(evt.pointerId);
            }
        }

        private void PointerCaptureOutHandler(PointerCaptureOutEvent evt) { _enabled = false; }
    }
}
#endif
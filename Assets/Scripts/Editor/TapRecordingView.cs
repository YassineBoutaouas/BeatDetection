#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using static SoundElements.Editor.ConfigureRhythmWindow;

namespace SoundElements.Editor
{
    /// <summary>
    /// This class provides information of a tap recording
    /// </summary>
    public class TapRecordingView : VisualElement
    {
        public TapRecording _tapRecording;

        protected const string _elementPath = "Assets/Scripts/Editor/UI/TapRecordingElement.uxml";
        protected const string _stylePath = "Assets/Scripts/Editor/UI/TapRecording_Style.uss";

        private FloatField _duration;
        private IntegerField _taps;
        private IntegerField _bpm;
        private Button _removeButton;

        private List<TapRecording> _tapRecordings;
        private List<TapRecordingView> _elements;

        private VisualElement _container;

        public TapRecordingView(VisualElement container, TapRecording tapRecording, List<TapRecording> tapRecordings, List<TapRecordingView> elements)
        {
            _tapRecording = tapRecording;
            _tapRecordings = tapRecordings;
            _elements = elements;

            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(_elementPath);
            visualTree.CloneTree(this);

            _duration = this.Q<FloatField>("duration");
            _taps = this.Q<IntegerField>("taps");
            _bpm = this.Q<IntegerField>("bpm");
            _removeButton = this.Q<Button>();

            _duration.value = tapRecording.Duration;
            _taps.value = tapRecording.Taps;
            _bpm.value = tapRecording.BPM;

            _removeButton.clicked += OnRemoveTapRecording;

            _container = container;
            _container.Add(this);
        }

        public void OnRemoveTapRecording()
        {
            _tapRecordings.Remove(_tapRecording);

            _elements.Remove(this);
            _container.Remove(this);
            Release();
        }

        public void Release()
        {
            _removeButton.clicked -= OnRemoveTapRecording;
            _tapRecording = null;
        }
    }
}
#endif
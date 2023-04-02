#if UNITY_EDITOR
using Codice.Client.Common;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace SoundElements.Editor
{
    public class ConfigureRhythmWindow : SoundEditor
    {
        #region Static members
        private Color _toggleColor = new Color(0.35f, 0, 0, 0.33333f);
        private Color _defaultColor;

        public static void OpenWindow(SerializedObject obj, SoundElement soundElement, VisualElement waveFormContainer, float[] waves, float[] samples, int sampleSize, Rect pos)
        {
            ConfigureRhythmWindow wnd = CreateInstance<ConfigureRhythmWindow>();
            wnd.titleContent = new GUIContent("Rhythm Editor");

            wnd.CreateSoundObject();
            wnd._soundElement = soundElement;
            wnd._audioSource.clip = soundElement.AudioClip;

            wnd._serializedObject = obj;

            wnd.SetPaths("Assets/Scripts/Editor/UI/ConfigureRhythmEditor.uxml", "Assets/Scripts/Editor/UI/ConfigureRhythm_Style.uss");
            wnd.position = pos;

            wnd.InitFields();

            #region Waveform Container Cloning
            Texture2D waveFormTex = new Texture2D(wnd._waveFormResolution.x, wnd._waveFormResolution.y);
            Graphics.CopyTexture(waveFormContainer.style.backgroundImage.value.texture, waveFormTex);

            wnd._waveFormContainer.style.width = waveFormContainer.style.width;

            Background bg = waveFormContainer.style.backgroundImage.value.texture;
            wnd._waveFormContainer.style.backgroundImage = bg;
            wnd._waveFormContainer.pickingMode = PickingMode.Ignore;
            #endregion

            wnd.Show();
            wnd.Focus();
        }
        #endregion

        private SoundEditorWindow _parentView;

        #region VisualElements
        private FloatField _sampleLength;
        private FloatField _sampleStart;
        private FloatField _sampleEnd;

        private MinMaxSlider _sequenceSlider;
        private VisualElement _sequenceMaxHandle;
        private VisualElement _sequenceMinHandle;

        private Button _recordButton;
        private Button _calcBPMButton;

        private IntegerField _averageBPM;
        private Button _calculateAverage;
        private Button _clearRecordingsButton;

        private IntegerField _bpmField;
        private VisualElement _tapRecordingsContainer;

        private Button _okButton;
        #endregion

        #region recording members
        private Vector4 _timeSteps = new Vector4(7.5f, 15f, 30f, 60f);

        private float _sequenceDuration;
        private float _sequenceStart;
        private float _sequenceEnd;

        private bool _isRecording = false;
        private List<TapRecording> _tapRecordings = new List<TapRecording>();
        private List<TapRecordingView> _tapRecordingElements = new List<TapRecordingView>();
        private TapRecording _currentRecording;
        #endregion

        public class TapRecording
        {
            public int Taps;
            public float Duration;
            public int BPM;

            public void Add() { Taps++; }
        }

        protected override void InitFields()
        {
            rootVisualElement.focusable = true;
            base.InitFields();
            _audioField.value = _soundElement.Name;

            _sampleLength = rootVisualElement.Q<FloatField>("sample-length");
            _sampleStart = rootVisualElement.Q<FloatField>("start-sample");
            _sampleEnd = rootVisualElement.Q<FloatField>("end-sample");

            _sequenceSlider = rootVisualElement.Q<MinMaxSlider>("sequence-slider");
            _sequenceMinHandle = _sequenceSlider.Q<VisualElement>("unity-thumb-min");
            _sequenceMaxHandle = _sequenceSlider.Q<VisualElement>("unity-thumb-max");

            _recordButton = rootVisualElement.Q<Button>("manual-configuration");
            _calcBPMButton = rootVisualElement.Q<Button>("automatic-configuration");
            _bpmField = rootVisualElement.Q<IntegerField>("bpm-field");

            _okButton = rootVisualElement.Q<Button>("ok-button");

            _tapRecordingsContainer = rootVisualElement.Q<VisualElement>("tap-recordings-container");
            _averageBPM = rootVisualElement.Q<IntegerField>("average-bpm");
            _calculateAverage = rootVisualElement.Q<Button>("calculate-average");
            _clearRecordingsButton = rootVisualElement.Q<Button>("clear");

            SetDefaultValues();

            SetEvents();
        }

        protected override void SetEvents()
        {
            base.SetEvents();

            _sequenceSlider.RegisterCallback<ChangeEvent<Vector2>>(OnSequenceSliderChange);
            _scrapper.RegisterCallback<ChangeEvent<float>>(ClampAudioTime);
            _bpmField.RegisterCallback<ChangeEvent<int>>(OnBPMChange);

            _recordButton.clicked += OnRecordChange;
            _calcBPMButton.clicked += OnBPMCalculate;

            rootVisualElement.RegisterCallback<KeyDownEvent>(OnTap);

            _calculateAverage.clicked += CalculateAverageBPM;
            _clearRecordingsButton.clicked += ClearTapRecordings;

            _okButton.clicked += OnClose;
        }

        private void SetDefaultValues()
        {
            _defaultColor = _recordButton.style.backgroundColor.value;
            StyleColor styleColor = _recordButton.style.backgroundColor;
            styleColor.value = _defaultColor;
            _recordButton.style.backgroundColor = styleColor;

            _bpmField.value = _soundElement.BPM;

            _sequenceSlider.value = new Vector2(0, _timeSteps.x / _soundElement.AudioClip.length);

            _sequenceEnd = _timeSteps.x;
            _sequenceDuration = _timeSteps.x;

            _sampleStart.value = _sequenceStart;
            _sampleEnd.value = _sequenceEnd;
            _sampleLength.value = _sequenceDuration;
        }

        protected override void ReleaseEvents()
        {
            base.ReleaseEvents();

            _sequenceSlider.UnregisterCallback<ChangeEvent<Vector2>>(OnSequenceSliderChange);
            _scrapper.UnregisterCallback<ChangeEvent<float>>(ClampAudioTime);
            _bpmField.UnregisterCallback<ChangeEvent<int>>(OnBPMChange);

            _recordButton.clicked -= OnRecordChange;
            _calcBPMButton.clicked -= OnBPMCalculate;

            rootVisualElement.UnregisterCallback<KeyDownEvent>(OnTap);

            _calculateAverage.clicked -= CalculateAverageBPM;
            _clearRecordingsButton.clicked -= ClearTapRecordings;

            _okButton.clicked -= OnClose;
        }

        private void OnBPMCalculate() { _bpmField.value = RhythmReader.CalculateBPM(_soundElement); }

        #region Recording methods
        private void OnRecordChange()
        {
            _isRecording = !_isRecording;

            StyleColor styleColor = _recordButton.style.backgroundColor;
            styleColor.value = _isRecording ? _toggleColor : _defaultColor;
            _recordButton.style.backgroundColor = styleColor;

            if (!_isRecording)
            {
                if (_currentRecording.Taps == 0) return;

                _currentRecording.BPM = (int)((_currentRecording.Taps / _sequenceDuration) * 60);
                _tapRecordingElements.Add(new TapRecordingView(_tapRecordingsContainer, _currentRecording, _tapRecordings, _tapRecordingElements));

                return;
            }

            _currentRecording = new TapRecording();
            _currentRecording.Duration = _sequenceDuration;
            _tapRecordings.Add(_currentRecording);
        }

        private void CalculateAverageBPM()
        {
            int bpmSum = 0;

            foreach (TapRecordingView tapRecordingView in _tapRecordingElements)
                bpmSum += tapRecordingView._tapRecording.BPM;

            _averageBPM.value = bpmSum / _tapRecordingElements.Count;
        }

        private void ClearTapRecordings()
        {
            _tapRecordingElements.Clear();
            _tapRecordingsContainer.Clear();
            _tapRecordings.Clear();
            _currentRecording = null;

            OnStopSoundFile();
            _isRecording = false;
        }

        private void OnTap(KeyDownEvent keyDownEvent)
        {
            if (keyDownEvent.keyCode == KeyCode.Space) return;

            if (_isRecording && _isPlaying)
                _currentRecording.Add();
        }
        #endregion

        private void OnBPMChange(ChangeEvent<int> evtChange)
        {
            _serializedObject.Update();
            _soundElement.BPM = evtChange.newValue;
            _serializedObject.ApplyModifiedProperties();
        }

        private void OnSequenceSliderChange(ChangeEvent<Vector2> evtCallback)
        {
            if (evtCallback.previousValue.x != evtCallback.newValue.x)
                _scrollView.ScrollTo(_sequenceMinHandle);
            else
                _scrollView.ScrollTo(_sequenceMaxHandle);

            _sequenceDuration = (evtCallback.newValue.y - evtCallback.newValue.x) * _soundElement.AudioClip.length;
            _sequenceStart = evtCallback.newValue.x * _soundElement.AudioClip.length;
            _sequenceEnd = evtCallback.newValue.y * _soundElement.AudioClip.length;

            _sampleStart.value = _sequenceStart;
            _sampleEnd.value = _sequenceEnd;
            _sampleLength.value = _sequenceDuration;

            _scrapper.value = evtCallback.newValue.x;
        }

        private void ClampAudioTime(ChangeEvent<float> evtCallback)
        {
            _scrapper.value = Mathf.Clamp(_scrapper.value, _sequenceSlider.value.x, _sequenceSlider.value.y);

            if (!_isPlaying) return;

            float relativeTime = _audioSource.time / _soundElement.AudioClip.length;
            if (relativeTime > _sequenceSlider.value.y)
            {
                OnUpdateCurrentTime(_sequenceSlider.value.x);
                _scrollView.horizontalScroller.value = _sequenceSlider.value.x;
                _audioSource.Stop();
                _isPlaying = false;

                //OnPlaySoundFile();
            }

        }

        private void OnClose()
        {
            _serializedObject.Update();
            _serializedObject.ApplyModifiedProperties();

            ClearTapRecordings();

            Close();
        }
    }
}
#endif
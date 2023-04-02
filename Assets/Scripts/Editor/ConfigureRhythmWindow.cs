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

        private IntegerField _bpmField;

        private Button _okButton;
        #endregion

        private Vector4 _timeSteps = new Vector4(7.5f, 15f, 30f, 60f);

        private float _sequenceDuration;
        private float _sequenceStart;
        private float _sequenceEnd;

        private bool _isRecording = false;
        private List<float> _taps = new List<float>();

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

            _okButton.clicked += OnClose;
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

            _okButton.clicked -= OnClose;
        }

        private void OnBPMCalculate() { _bpmField.value = RhythmReader.CalculateBPM(_soundElement); }

        private void OnRecordChange()
        {
            _isRecording = !_isRecording;

            StyleColor styleColor = _recordButton.style.backgroundColor;
            styleColor.value = _isRecording ? _toggleColor : _defaultColor;
            _recordButton.style.backgroundColor = styleColor;

            if (_taps.Count == 0) return;

            for (int i = 0; i < _taps.Count; i++)
            {
                Debug.Log(_taps[i]);
            }

            Debug.Log((_taps.Count / _sequenceDuration) * 60); 

            _taps.Clear();
        }

        private void OnTap(KeyDownEvent keyDownEvent)
        {
            if (keyDownEvent.keyCode == KeyCode.Space) return;

            if (_isRecording && _isPlaying)
            {
                _taps.Add(_currentTimeStamp);

                //Debug.Log($"{_currentTimeStamp} ; {_scrapper.value} ; {_scrapper.value * _soundElement.AudioClip.length}");
            }
        }

        private void OnBPMChange(ChangeEvent<int> evtChange)
        {
            _serializedObject.Update();
            _soundElement.BPM = evtChange.newValue;
            _serializedObject.ApplyModifiedProperties();
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
            Close();
        }
    }
}
#endif
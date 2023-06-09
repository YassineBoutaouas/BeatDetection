#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SoundElements.Editor
{
    /// <summary>
    /// This is a wrapper class that contains common functionality for an audio editor
    /// </summary>
    public class SoundEditor : EditorWindow
    {
        public string uxmlPath;
        public string ussPath;

        protected SerializedObject _serializedObject;

        #region Visual Elements
        protected VisualElement _waveFormContainer;
        protected Button _pauseButton;
        protected Button _playButton;
        protected Button _stopButton;

        protected Scrapper _scrapper;
        protected ScrollView _scrollView;
        protected Slider _volumeSlider;
        protected TextField _audioField;
        #endregion

        #region Sound element references
        protected SoundElement _soundElement;
        protected AudioSource _audioSource;
        protected GameObject _audioObject;

        protected float _currentTimeStamp;
        protected bool _isPlaying;
        #endregion

        protected Vector2Int _waveFormResolution = new Vector2Int(4096, 1024);
        protected Color _waveColor = new Color(0.78f, 0.65f, 0.34f, 1);

        protected void SetPaths(string uxmlPath, string ussPath)
        {
            this.uxmlPath = uxmlPath;
            this.ussPath = ussPath;
        }

        #region Serialized Object
        protected void UpdateSerializedObject() { _serializedObject.Update(); }
        protected void ApplySerializedObject() { _serializedObject.ApplyModifiedProperties(); }
        #endregion

        /// <summary>
        /// Initializes all of the fields needed for the editor
        /// </summary>
        protected virtual void InitFields()
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            visualTree.CloneTree(rootVisualElement);

            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);
            rootVisualElement.styleSheets.Add(styleSheet);

            _waveFormContainer = rootVisualElement.Q<VisualElement>("wave-container");
            _scrollView = rootVisualElement.Q<ScrollView>("amplitude-scroller");

            _pauseButton = rootVisualElement.Q<Button>("pause-button");
            _playButton = rootVisualElement.Q<Button>("play-button");
            _stopButton = rootVisualElement.Q<Button>("stop-button");

            _audioField = rootVisualElement.Q<TextField>("audio-field");
            _audioField.focusable = false;

            _volumeSlider = rootVisualElement.Q<Slider>("volume-slider");
            _scrapper = rootVisualElement.Q<Scrapper>();
        }

        /// <summary>
        /// Creates a temporary gameObject with an audio source to be played - it is destroyed when the window is closed
        /// </summary>
        protected void CreateSoundObject()
        {
            _audioObject = new GameObject("Temp_AudioObject");
            _audioSource = _audioObject.AddComponent<AudioSource>();
            _audioSource.loop = false;
        }

        protected virtual void CreateGUI() { }

        /// <summary>
        /// Subscribe events
        /// </summary>
        protected virtual void SetEvents()
        {
            _volumeSlider.RegisterCallback<ChangeEvent<float>>(ChangeVolume);
            _playButton.clicked += OnPlaySoundFile;
            _pauseButton.clicked += OnPauseSoundFile;
            _stopButton.clicked += OnStopSoundFile;

            _scrapper.valueChanged += OnUpdateCurrentTime;

            EditorApplication.playModeStateChanged += OnPlayModeChange;
        }

        protected void ResetView()
        {
            _scrapper.value = 0;
            OnUpdateCurrentTime(0);
            _scrollView.horizontalScroller.value = 0;
        }

        protected float CalculateRelativePositionInWindow(float t) { return _waveFormContainer.style.width.value.value * (t / _soundElement.AudioClip.length); }

        protected virtual void OnGUI() { _scrapper.DraggerBody.transform.scale = new Vector3(_scrapper.DraggerBody.transform.scale.x, _waveFormContainer.resolvedStyle.height); }

        /// <summary>
        /// Updates the scrapper position to correspond to the current time of the audio that is currently playing
        /// </summary>
        protected virtual void Update()
        {
            if (!_isPlaying || _audioSource.clip == null) return;

            _scrapper.value = _audioSource.time / _soundElement.AudioClip.length;
        }

        /// <summary>
        /// Is invoked when the current time of the audio is updated
        /// </summary>
        protected void OnUpdateCurrentTime(float time)
        {
            if (_soundElement == null) return;

            _currentTimeStamp = _soundElement.AudioClip.length * time;

            _scrollView.ScrollTo(_scrapper.Dragger);
        }

        #region Audio methods
        protected void ChangeVolume(ChangeEvent<float> volume)
        {
            if (_audioSource == null) return;

            _audioSource.volume = volume.newValue;
        }

        protected void OnPlayStateChange(KeyDownEvent keyEvent)
        {
            if (keyEvent.keyCode != KeyCode.Space) return;

            if (_isPlaying)
            {
                OnPauseSoundFile();
                return;
            }

            OnPlaySoundFile();
        }

        protected void OnPlaySoundFile()
        {
            if (_audioSource == null) return;

            _audioSource.time = _currentTimeStamp;
            _audioSource.Play();

            _isPlaying = true;
        }

        protected void OnPauseSoundFile()
        {
            if (_audioSource == null) return;

            _audioSource.Pause();

            _isPlaying = false;
        }

        protected void OnStopSoundFile()
        {
            if (_audioSource == null) return;

            _audioSource.Stop();

            _scrapper.value = 0;
            OnUpdateCurrentTime(0);
            _scrollView.horizontalScroller.value = 0;

            _isPlaying = false;
        }
        #endregion

        protected virtual void OnPlayModeChange(PlayModeStateChange playMode)
        {
            switch (playMode)
            {
                case PlayModeStateChange.EnteredEditMode:
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    if (_audioObject != null) DestroyImmediate(_audioObject);
                    Close();
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    if (_audioObject != null) DestroyImmediate(_audioObject);
                    Close();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Releases all of the bound events
        /// </summary>
        protected virtual void ReleaseEvents()
        {
            OnStopSoundFile();

            _volumeSlider?.UnregisterCallback<ChangeEvent<float>>(ChangeVolume);

            if(_playButton != null) _playButton.clicked -= OnPlaySoundFile;
            if (_pauseButton != null) _pauseButton.clicked -= OnPauseSoundFile;
            if (_stopButton != null) _stopButton.clicked -= OnStopSoundFile;

            if (_scrapper != null) _scrapper.valueChanged -= OnUpdateCurrentTime;

            EditorApplication.playModeStateChanged -= OnPlayModeChange;
        }

        protected virtual void OnDisable()
        {
            ReleaseEvents();
            GameObject.DestroyImmediate(_audioObject);
        }
    }
}
#endif
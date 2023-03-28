#if UNITY_EDITOR
using SoundElements;
using SoundElements.Editor;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class ConfigureRhythmWindow : EditorWindow
{
    #region Static members
    private const string _configuratorPath = "Assets/Scripts/Editor/UI/ConfigureRhythmEditor.uxml";
    private const string _configuratorStyle = "Assets/Scripts/Editor/UI/ConfigureRhythm_Style.uss";

    public static void OpenWindow(SerializedObject obj, SoundElement soundElement, AudioSource source, VisualElement waveFormContainer, float[] waves, float[] samples, int sampleSize, Rect pos)
    {
        ConfigureRhythmWindow wnd = CreateWindow<ConfigureRhythmWindow>("Rhythm Editor");

        wnd._soundElement = soundElement;
        wnd._audioSource = source;

        wnd._waveFormContainer = new VisualElement { name = "wave-container" };

        #region Waveform Container Cloning
        Texture2D waveFormTex = new Texture2D(4096, 1024);
        Graphics.CopyTexture(waveFormContainer.style.backgroundImage.value.texture, waveFormTex);

        wnd._waveFormContainer.style.width = waveFormContainer.style.width;

        Background bg = waveFormContainer.style.backgroundImage.value.texture;
        wnd._waveFormContainer.style.backgroundImage = bg;
        wnd._waveFormContainer.pickingMode = PickingMode.Ignore;
        #endregion

        wnd._wavesArray = waves;
        wnd._sampleArray = samples;
        wnd._sampleSize = sampleSize;
        wnd.position = pos;

        wnd._serializedObject = obj;

        wnd.Init();
        wnd.Focus();
    }
    #endregion

    private SerializedObject _serializedObject;

    #region Elements
    private VisualElement _waveFormContainer;

    private Button _pauseButton;
    private Button _playButton;
    private Button _stopButton;

    private Slider _volumeSlider;

    private TextField _audioField;

    private ScrollView _scrollView;
    #endregion

    #region Sound element references
    private SoundElement _soundElement;
    private AudioSource _audioSource;

    private float _upperBound;
    private float _lowerBound;

    private bool _isPlaying;
    #endregion

    #region Wave form
    private int[] _waveFormResolution = new int[] { 4096, 1024 };

    private float[] _wavesArray;
    private float[] _sampleArray;

    private int _sampleSize;
    #endregion

    private void InitFields()
    {
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(_configuratorPath);
        visualTree.CloneTree(rootVisualElement);

        StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(_configuratorStyle);
        rootVisualElement.styleSheets.Add(styleSheet);

        _scrollView = rootVisualElement.Q<ScrollView>();

        _pauseButton = rootVisualElement.Q<Button>("pause-button");
        _playButton = rootVisualElement.Q<Button>("play-button");
        _stopButton = rootVisualElement.Q<Button>("stop-button");

        _scrollView.contentContainer.Insert(1, _waveFormContainer);

        _audioField = rootVisualElement.Q<TextField>();
        _audioField.focusable = false;

        _volumeSlider = rootVisualElement.Q<Slider>("volume-slider");
    }

    public void Init()
    {
        InitFields();

        _volumeSlider.RegisterCallback<ChangeEvent<float>>(ChangeVolume);
        _playButton.clicked += OnPlaySoundFile;
        _pauseButton.clicked += OnPauseSoundFile;
        _stopButton.clicked += OnStopSoundFile;
    }

    #region Audio methods
    private void ChangeVolume(ChangeEvent<float> volume)
    {
        if (_audioSource == null) return;

        _audioSource.volume = volume.newValue;
    }

    private void OnPlaySoundFile()
    {
        if (_audioSource == null) return;

        _audioSource.time = _lowerBound;
        _audioSource.Play();

        _isPlaying = true;
    }

    private void OnPauseSoundFile()
    {
        if (_audioSource == null) return;

        _audioSource.Pause();

        _isPlaying = false;
    }

    private void OnStopSoundFile()
    {
        if (_audioSource == null) return;

        _audioSource.Stop();

        //_scrapper.value = 0;
        //OnUpdateCurrentTime(0);
        _scrollView.horizontalScroller.value = 0;

        _isPlaying = false;
    }
    #endregion

    private void OnDisable()
    {
        _volumeSlider.UnregisterCallback<ChangeEvent<float>>(ChangeVolume);
        _playButton.clicked -= OnPlaySoundFile;
        _pauseButton.clicked -= OnPauseSoundFile;
        _stopButton.clicked -= OnStopSoundFile;

        OnStopSoundFile();
    }
}
#endif
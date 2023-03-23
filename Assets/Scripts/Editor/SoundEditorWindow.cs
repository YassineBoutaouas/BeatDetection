#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public class SoundEditorWindow : EditorWindow
{
    #region Static members
    private const string _configuratorPath = "Assets/Scripts/Editor/UI/SoundEditor.uxml";
    private const string _configuratorStyle = "Assets/Scripts/Editor/UI/SoundEditor_Style.uss";

    [MenuItem("Sound/SoundEditor")]
    public static void OpenWindow()
    {
        SoundEditorWindow wnd = GetWindow<SoundEditorWindow>();
        wnd.titleContent = new GUIContent("Sound Editor");
    }
    #endregion

    #region Visual Elements
    private VisualElement _mainWindow;
    private VisualElement _scrapper;

    private Scroller _amplitudeScroller;
    private VisualElement _waveFormContainer;

    private Button _skipButton;
    private Button _pauseButton;
    private Button _playButton;
    private Button _stopButton;

    private Button _selectButton;

    private TextField _audioField;
    #endregion

    private SoundFileSearchProvider _soundFileSearchProvider;

    private int[] _waveFormResolution = new int[] { 4096, 1024 };
    private Color _waveBackDrop = new Color(0, 0, 0, 0);
    private Color _waveColor = new Color(0.78f, 0.65f, 0.34f, 1);

    private AudioClip _audioClip;
    private Texture2D _waveFormTexture;

    private float[] _wavesArray;
    private float[] _sampleArray;

    private int _sampleSize;

    public void CreateGUI()
    {
        #region Visual element
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(_configuratorPath);
        visualTree.CloneTree(rootVisualElement);

        StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(_configuratorStyle);
        rootVisualElement.styleSheets.Add(styleSheet);

        _mainWindow = rootVisualElement.Q<VisualElement>("main-window");

        _scrapper = rootVisualElement.Q<VisualElement>("scrapper");

        _amplitudeScroller = rootVisualElement.Q<Scroller>("amplitude-scroller");
        _waveFormContainer = rootVisualElement.Q<VisualElement>("wave-container");

        _skipButton = rootVisualElement.Q<Button>("skip-button");
        _pauseButton = rootVisualElement.Q<Button>("pause-button");
        _playButton = rootVisualElement.Q<Button>("play-button");
        _stopButton = rootVisualElement.Q<Button>("stop-button");

        _audioField = rootVisualElement.Q<TextField>();
        _audioClip = null;
        _audioField.focusable = false;
        _audioField.value = null;

        _selectButton = rootVisualElement.Q<Button>("audio-select");
        #endregion

        _selectButton.clicked -= OnOpenSearchTree;
        _selectButton.clicked += OnOpenSearchTree;

        _pauseButton.clicked += DrawWaveForm;

        _soundFileSearchProvider = ScriptableObject.CreateInstance<SoundFileSearchProvider>();
        _soundFileSearchProvider.OnSelectedSoundFile = null;
        _soundFileSearchProvider.OnSelectedSoundFile += OnSelectSoundFile;

        _waveFormTexture = new Texture2D(_waveFormResolution[0], _waveFormResolution[1], TextureFormat.RGBA32, true);
        _wavesArray = new float[_waveFormResolution[0]];
    }

    private void OnOpenSearchTree() { SearchWindow.Open(new SearchWindowContext(Mouse.current.position.ReadValue()), _soundFileSearchProvider); }

    private void OnSelectSoundFile(AudioClip audioClip)
    {
        _audioClip = audioClip;
        _audioField.value = _audioClip.name;
    }

    private void DrawWaveForm()
    {
        _sampleSize = _audioClip.samples * _audioClip.channels;
        _sampleArray = new float[_sampleSize];

        _audioClip.GetData(_sampleArray, 0);

        int compressedSize = _sampleSize / _waveFormResolution[0];

        for (int x = 0; x < _waveFormResolution[0]; x++)
        {
            _wavesArray[x] = Mathf.Abs(_sampleArray[x * compressedSize]);
            for (int y = 0; y < _waveFormResolution[1]; y++)
            {
                _waveFormTexture.SetPixel(x, y, _waveBackDrop);
            }
        }

        IStyle rect = _waveFormContainer.style;
        rect.width = compressedSize;

        int height = (int)(_waveFormResolution[1] / 2);
        for (int x = 0; x < _waveFormResolution[0]; x++)
        {
            for (int y = 0; y < _wavesArray[x] * _waveFormResolution[1]; y++)
            {
                _waveFormTexture.SetPixel(x, height + y, _waveColor);
                _waveFormTexture.SetPixel(x, height - y, _waveColor);
            }
        }

        _waveFormTexture.Apply();

        _waveFormContainer.style.backgroundImage = _waveFormTexture;
    }

    private void OnDisable()
    {
        _selectButton.clicked -= OnOpenSearchTree;
        _pauseButton.clicked -= DrawWaveForm;
        _soundFileSearchProvider.OnSelectedSoundFile = null;
    }
}

public class SoundFileSearchProvider : ScriptableObject, ISearchWindowProvider
{
    public System.Action<AudioClip> OnSelectedSoundFile;
    private List<SearchTreeEntry> _searchTree;

    public SoundFileSearchProvider()
    {
        _searchTree = new List<SearchTreeEntry>();
    }

    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        _searchTree.Clear();

        _searchTree.Add(new SearchTreeGroupEntry(new GUIContent("AudioClips"), 0));

        foreach (string guid in AssetDatabase.FindAssets("t:" + typeof(AudioClip).Name))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            AudioClip soundFile = (AudioClip)AssetDatabase.LoadAssetAtPath(path, typeof(AudioClip));

            SearchTreeEntry entry = new SearchTreeEntry(new GUIContent(soundFile.name));
            entry.level = 1;
            entry.userData = soundFile;
            _searchTree.Add(entry);
        }

        return _searchTree;
    }

    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
    {
        OnSelectedSoundFile?.Invoke((AudioClip)SearchTreeEntry.userData);
        return true;
    }
}
#endif
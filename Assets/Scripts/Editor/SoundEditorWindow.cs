#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace SoundElements.Editor
{
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

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            if (Selection.activeObject is SoundElement)
            {
                SoundEditorWindow wnd = GetWindow<SoundEditorWindow>();
                wnd.titleContent = new GUIContent("Sound Editor");
                wnd.OnSelectSoundFile((SoundElement)Selection.activeObject);
                return true;
            }
            return false;
        }
        #endregion

        #region Visual Elements
        private VisualElement _mainWindow;
        private VisualElement _waveFormContainer;

        private VisualElement _eventContainer;

        private Button _pauseButton;
        private Button _playButton;
        private Button _stopButton;

        private ToolbarButton _createButton;
        private ToolbarButton _selectButton;

        private Scrapper _scrapper;
        private ScrollView _scrollView;
        private Slider _volumeSlider;

        private TextField _audioField;


        private InspectorView _inspectorView;
        private EventProperties _eventProperties;

        private class EventProperties
        {
            public PropertyField MethodName;
            public PropertyField FloatValue;
            public PropertyField IntValue;
            public PropertyField StringValue;
            public PropertyField BoolValue;

            public EventProperties(InspectorView container)
            {
                MethodName = new PropertyField { name = "method-property" };
                MethodName.style.marginTop = 10;
                FloatValue = new PropertyField { name = "float-property" };
                IntValue = new PropertyField { name = "int-property" };
                StringValue = new PropertyField { name = "string-property" };
                BoolValue = new PropertyField { name = "bool-property" };

                container.Add(MethodName);
                container.Add(FloatValue);
                container.Add(IntValue);
                container.Add(StringValue);
                container.Add(BoolValue);
            }

            public void BindObject(SerializedObject obj)
            {
                MethodName.Bind(obj);
                FloatValue.Bind(obj);
                IntValue.Bind(obj);
                StringValue.Bind(obj);
                BoolValue.Bind(obj);
            }

            public void BindProperties(SerializedProperty method, SerializedProperty floatProp, SerializedProperty intProp, SerializedProperty stringProp, SerializedProperty boolProp)
            {
                MethodName.BindProperty(method);
                FloatValue.BindProperty(floatProp);
                IntValue.BindProperty(intProp);
                StringValue.BindProperty(stringProp);
                BoolValue.BindProperty(boolProp);
            }
        }
        #endregion

        #region Wave form
        private int[] _waveFormResolution = new int[] { 4096, 1024 };
        private Color _waveBackDrop = new Color(0, 0, 0, 0);
        private Color _waveColor = new Color(0.78f, 0.65f, 0.34f, 1);

        private Texture2D _waveFormTexture;

        private float[] _wavesArray;
        private float[] _sampleArray;

        private int _sampleSize;
        #endregion

        #region Sound element references
        private SoundElement _soundElement;
        private AudioSource _audioSource;
        private GameObject _audioObject;

        private float _currentTimeStamp;
        private bool _isPlaying;
        #endregion

        #region Subeditors and manipulators
        private SoundFileSearchProvider _soundFileSearchProvider;
        private ContextualMenuManipulator _contextManipulator;
        private Clickable _clickable;
        #endregion

        private SerializedObject _serializedObject;

        private void InitFields()
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(_configuratorPath);
            visualTree.CloneTree(rootVisualElement);

            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(_configuratorStyle);
            rootVisualElement.styleSheets.Add(styleSheet);

            _mainWindow = rootVisualElement.Q<VisualElement>("main-window");
            _waveFormContainer = rootVisualElement.Q<VisualElement>("wave-container");
            _eventContainer = rootVisualElement.Q<VisualElement>("event-container");

            _scrollView = rootVisualElement.Q<ScrollView>();

            _pauseButton = rootVisualElement.Q<Button>("pause-button");
            _playButton = rootVisualElement.Q<Button>("play-button");
            _stopButton = rootVisualElement.Q<Button>("stop-button");

            _createButton = rootVisualElement.Q<ToolbarButton>("create-audio");
            _selectButton = rootVisualElement.Q<ToolbarButton>("audio-select");

            _audioField = rootVisualElement.Q<TextField>();
            _audioField.focusable = false;

            _volumeSlider = rootVisualElement.Q<Slider>();
            _scrapper = rootVisualElement.Q<Scrapper>();

            _inspectorView = rootVisualElement.Q<InspectorView>();
            _eventProperties = new EventProperties(_inspectorView);

            _soundFileSearchProvider = ScriptableObject.CreateInstance<SoundFileSearchProvider>();

            _eventContainer.RegisterCallback<KeyDownEvent>(RemoveEvent);
            _eventContainer.RegisterCallback<KeyDownEvent>(JumpTo);
        }

        public void CreateGUI()
        {
            InitFields();

            _soundFileSearchProvider.Initialize(OnSelectSoundFile);

            #region Audio Waves
            _waveFormTexture = new Texture2D(_waveFormResolution[0], _waveFormResolution[1], TextureFormat.RGBA32, true);
            _wavesArray = new float[_waveFormResolution[0]];

            _audioObject = new GameObject("Temp_AudioObject");
            _audioSource = _audioObject.AddComponent<AudioSource>();
            _audioSource.loop = false;
            #endregion

            #region SounElement Buttons Callbacks
            _createButton.clicked += CreateSoundElement;
            _selectButton.clicked += OnOpenSearchTree;
            #endregion

            #region Audio Buttons callbacks
            _volumeSlider.RegisterCallback<ChangeEvent<float>>(ChangeVolume);
            _playButton.clicked += OnPlaySoundFile;
            _pauseButton.clicked += OnPauseSoundFile;
            _stopButton.clicked += OnStopSoundFile;
            #endregion

            _scrapper.valueChanged += OnUpdateCurrentTime;

            _contextManipulator = new ContextualMenuManipulator(CreateEventContext);
            _eventContainer.AddManipulator(_contextManipulator);
            _clickable = new Clickable(SelectEvent, 0, 0);
            _eventContainer.AddManipulator(_clickable);

            EditorApplication.playModeStateChanged += OnPlayModeChange;
        }

        public void OnGUI() { _scrapper.DraggerBody.transform.scale = new Vector3(_scrapper.DraggerBody.transform.scale.x, _waveFormContainer.resolvedStyle.height); }

        public void Update()
        {
            if (!_isPlaying || _audioSource.clip == null) return;

            _scrapper.value = _audioSource.time / _soundElement.AudioClip.length;
        }

        private void OnPlayModeChange(PlayModeStateChange playMode)
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
                    if(_audioObject != null) DestroyImmediate(_audioObject);
                    Close();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
                default:
                    break;
            }
        }

        #region Event methods
        private void CreateEventContext(ContextualMenuPopulateEvent menuBuilder)
        {
            menuBuilder.menu.AppendAction("Add Sound Event", AddEvent, DropdownMenuAction.Status.Normal);
            menuBuilder.menu.AppendAction("Remove Sound Event", RemoveEvent, DropdownMenuAction.Status.Normal);
            menuBuilder.menu.AppendAction("Jump to Sound Event", JumpTo, DropdownMenuAction.Status.Normal);
        }

        private void AddEvent(DropdownMenuAction menuAction)
        {
            if (_soundElement == null) return;

            SoundEvent soundEvent = new SoundEvent(_currentTimeStamp);
            _soundElement.SoundEvents.Add(soundEvent);

            EventElement evtElement = new EventElement(soundEvent, _waveFormContainer, _scrollView, _soundElement);

            evtElement.transform.position = new Vector3(_scrapper.Dragger.transform.position.x, 0);
            evtElement.AddToClassList("event-element");

            _eventContainer.Add(evtElement);

            evtElement.Focus();

            _serializedObject.Update();
            _serializedObject.ApplyModifiedProperties();

            SelectEvent();
        }

        private void RemoveEvent(KeyDownEvent keydown)
        {
            if (keydown.keyCode != KeyCode.Delete) return;

            RemoveEvent(menuAction: null);
        }

        private void RemoveEvent(DropdownMenuAction menuAction)
        {
            EventElement focus = (EventElement)_eventContainer.focusController.focusedElement;

            if (focus == null) return;

            _soundElement.SoundEvents.Remove(focus._soundEvent);
            _eventContainer.Remove(focus);

            SelectEvent();
        }

        private void JumpTo(KeyDownEvent keydown)
        {
            if (keydown.keyCode != KeyCode.F) return;

            JumpTo(menuAction: null);
        }

        private void JumpTo(DropdownMenuAction menuAction)
        {
            EventElement focus = (EventElement)_eventContainer.focusController.focusedElement;

            if (focus == null) return;

            _scrapper.value = focus.transform.position.x / _waveFormContainer.style.width.value.value;
        }

        private void SelectEvent()
        {
            EventElement focus = (EventElement)_eventContainer.focusController.focusedElement;

            if (focus == null)
            {
                _inspectorView.visible = false;
                return;
            }

            int i = _soundElement.SoundEvents.IndexOf(focus._soundEvent);

            SerializedProperty prop = _serializedObject.FindProperty(nameof(SoundElement.SoundEvents)).GetArrayElementAtIndex(i);

            _eventProperties.BindProperties(
                prop.FindPropertyRelative(nameof(SoundEvent.MethodName)),
                prop.FindPropertyRelative(nameof(SoundEvent.FloatValue)),
                prop.FindPropertyRelative(nameof(SoundEvent.IntValue)),
                prop.FindPropertyRelative(nameof(SoundEvent.StringValue)),
                prop.FindPropertyRelative(nameof(SoundEvent.BoolValue)));

            _serializedObject.ApplyModifiedProperties();

            _inspectorView.visible = true;
        }

        private void PopulateEvents()
        {
            _eventContainer.Clear();
            _inspectorView.visible = false;

            if (_soundElement == null) return;

            foreach (SoundEvent evt in _soundElement.SoundEvents)
            {
                EventElement evtElement = new EventElement(evt, _waveFormContainer, _scrollView, _soundElement);
                evtElement.transform.position = new Vector3(CalculateRelativePositionInWindow(evt.TimeStamp), 0);
                evtElement.AddToClassList("event-element");

                _eventContainer.Add(evtElement);
            }
        }
        #endregion

        private void OnUpdateCurrentTime(float time)
        {
            if (_soundElement == null) return;

            _currentTimeStamp = _soundElement.AudioClip.length * time;

            _scrollView.ScrollTo(_scrapper.Dragger);
        }

        #region Select methods
        private float CalculateRelativePositionInWindow(float t)
        {
            return _waveFormContainer.style.width.value.value * (t / _soundElement.AudioClip.length);
        }

        private void OnOpenSearchTree() { SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Mouse.current.position.ReadValue())), _soundFileSearchProvider); }

        private void CreateSoundElement() { CreateSoundElementWindow.OpenWindow(OnSelectSoundFile); }

        private void OnSelectSoundFile(SoundElement soundElement)
        {
            if (soundElement != null)
            {
                _audioSource.Stop();
                _isPlaying = false;
            }

            _soundElement = soundElement;
            _audioField.value = _soundElement.name;
            _audioSource.clip = soundElement.AudioClip;

            DrawWaveForm();

            _scrapper.value = 0;

            OnUpdateCurrentTime(0);

            _scrollView.horizontalScroller.value = 0;

            _serializedObject = new SerializedObject(_soundElement);
            _eventProperties.BindObject(_serializedObject);

            PopulateEvents();
        }

        private void DrawWaveForm()
        {
            float t = Time.realtimeSinceStartup;

            _sampleSize = _soundElement.AudioClip.samples * _soundElement.AudioClip.channels;
            _sampleArray = new float[_sampleSize];

            _soundElement.AudioClip.GetData(_sampleArray, 0);

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

            Debug.Log(Time.realtimeSinceStartup - t);
        }
        #endregion

        #region Audio methods
        private void ChangeVolume(ChangeEvent<float> volume)
        {
            if (_audioSource == null) return;

            _audioSource.volume = volume.newValue;
        }

        private void OnPlaySoundFile()
        {
            if (_audioSource == null) return;

            _audioSource.time = _currentTimeStamp;
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

            _scrapper.value = 0;
            OnUpdateCurrentTime(0);
            _scrollView.horizontalScroller.value = 0;

            _isPlaying = false;
        }
        #endregion

        private void OnDisable()
        {
            _volumeSlider.UnregisterCallback<ChangeEvent<float>>(ChangeVolume);

            _selectButton.clicked -= OnOpenSearchTree;
            _playButton.clicked -= OnPlaySoundFile;
            _pauseButton.clicked -= OnPauseSoundFile;
            _stopButton.clicked -= OnStopSoundFile;

            _soundFileSearchProvider.OnRelease();

            _eventContainer.UnregisterCallback<KeyDownEvent>(RemoveEvent);

            EditorApplication.playModeStateChanged -= OnPlayModeChange;

            GameObject.DestroyImmediate(_audioObject);
        }
    }

    public class EventElement : VisualElement
    {
        public SoundEvent _soundEvent;
        private DragAndDropManipulator _dragManipulator;

        public EventElement(SoundEvent soundEvent, VisualElement referenceContainer, ScrollView scrollView, SoundElement soundElement)
        {
            name = "event-element";
            style.flexGrow = 0;
            style.width = 3;
            style.height = 100;

            style.position = Position.Absolute;

            focusable = true;
            usageHints = UsageHints.DynamicTransform;

            _soundEvent = soundEvent;

            _dragManipulator = new DragAndDropManipulator(this, referenceContainer, scrollView, soundElement);
            this.AddManipulator(_dragManipulator);
        }
    }

    public class SoundFileSearchProvider : ScriptableObject, ISearchWindowProvider
    {
        public System.Action<SoundElement> OnSelectedSoundFile;
        private List<SearchTreeEntry> _searchTree;

        public void Initialize(params Action<SoundElement>[] actions)
        {
            _searchTree = new List<SearchTreeEntry>();

            OnSelectedSoundFile = null;

            foreach (Action<SoundElement> a in actions)
                OnSelectedSoundFile += a;
        }

        public void OnRelease() { OnSelectedSoundFile = null; }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            _searchTree.Clear();

            _searchTree.Add(new SearchTreeGroupEntry(new GUIContent("SoundElements"), 0));

            foreach (string guid in AssetDatabase.FindAssets("t:" + typeof(SoundElement).Name))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                SoundElement soundFile = (SoundElement)AssetDatabase.LoadAssetAtPath(path, typeof(SoundElement));

                SearchTreeEntry entry = new SearchTreeEntry(new GUIContent(soundFile.name));
                entry.level = 1;
                entry.userData = soundFile;
                _searchTree.Add(entry);
            }

            return _searchTree;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            OnSelectedSoundFile?.Invoke((SoundElement)SearchTreeEntry.userData);
            return true;
        }
    }
}
#endif
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class CreateSoundElementWindow : EditorWindow
{
    private const string _createSoundElementPath = "Assets/Scripts/Editor/UI/CreateSoundElementEditor.uxml";
    private const string _createElementStyle = "Assets/Scripts/Editor/UI/CreateSoundElement_Style.uss";

    private TextField _objectName;
    private ObjectField _audioField;

    private Label _pathLabel;
    private Button _changePath;

    private Button _saveAsset;
    private Button _cancel;

    private string _selectedPath;

    private System.Action<SoundElement> OnSoundElementSaved;

    public static void OpenWindow(System.Action<SoundElement> onSoundElementSaved)
    {
        CreateSoundElementWindow wnd = GetWindow<CreateSoundElementWindow>();
        wnd.titleContent = new GUIContent("Create new configurator");

        wnd.position = new Rect(Screen.width / 2, Screen.height / 2, Screen.width / 4, Screen.height / 4);
        wnd.ShowPopup();

        wnd.OnSoundElementSaved = onSoundElementSaved;
    }

    public void CreateGUI()
    {
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(_createSoundElementPath);
        visualTree.CloneTree(rootVisualElement);
        rootVisualElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(_createElementStyle));

        _objectName = rootVisualElement.Q<TextField>();

        _pathLabel = rootVisualElement.Q<Label>("path-label");
        _changePath = rootVisualElement.Q<Button>("path-button");

        _saveAsset = rootVisualElement.Q<Button>("save-button");
        _cancel = rootVisualElement.Q<Button>("cancel-button");

        _audioField = rootVisualElement.Q<ObjectField>("audio-object");

        _changePath.clicked += SelectPath;
        _saveAsset.clicked += SaveNewSoundElement;
        _cancel.clicked += Close;
    }

    private void SelectPath()
    {
        string absolutePath = EditorUtility.OpenFolderPanel("Select folder", "Assets/Scripts", "");
        if (absolutePath == null || absolutePath == "") return;

        _selectedPath = absolutePath.Substring(absolutePath.IndexOf("Assets/"));
        _pathLabel.text = _selectedPath;
    }

    private void SaveNewSoundElement()
    {
        if (_selectedPath == null)
        {
            EditorUtility.DisplayDialog("Path empty error", "Please select a valid path", "Ok");
            return;
        }

        if(_audioField.value == null)
        {
            EditorUtility.DisplayDialog("No Audio clip selected", "Please select a valid Audio clip", "Ok");
            return;
        }

        SoundElement obj = ScriptableObject.CreateInstance<SoundElement>();
        obj.Name = _objectName.text;
        obj.AudioClip = (AudioClip)_audioField.value;

        AssetDatabase.CreateAsset(obj, _selectedPath + $"/{_objectName.text}.asset");
        AssetDatabase.SaveAssets();

        OnSoundElementSaved(obj);

        Close();
    }

    private void OnDisable()
    {
        _changePath.clicked -= SelectPath;
        _saveAsset.clicked -= SaveNewSoundElement;
        _cancel.clicked -= Close;
    }
}
#endif
#if UNITY_EDITOR
using System;
using Editor.Scripts.Settings;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Scripts.Windows
{

public class MegaPintAutoSaveSettings : MegaPintEditorWindowBase
{
    #region Private Methods

    private void OnIntervalChange(ChangeEvent <int> evt)
    {
        if (evt.newValue < 1)
            _interval.value = 1;
    }

    private void OnPathChange()
    {
        var path = EditorUtility.OpenFolderPanel("Set folder for duplicates", "Assets/", "");

        if (!path.StartsWith(Application.dataPath))
        {
            EditorUtility.DisplayDialog(
                "Folder not valid",
                "The specified folder must be inside the unity Assets folder",
                "OK");
        }
        else
        {
            path = path.Replace(Application.dataPath, "");
            MegaPintAutoSaveData.DuplicatePathValue = path.Insert(0, "Assets");

            EditorUtility.DisplayDialog(
                "New folder",
                $"All duplicates will now be saved to:\n{MegaPintAutoSaveData.DuplicatePathValue}",
                "OK");

            OnUpdateDuplicatePathGUI(null);
        }
    }

    private void OnUpdateDuplicatePathGUI(ChangeEvent <string> evt)
    {
        _duplicatePath.style.display = _saveMode.index == 1 ? DisplayStyle.Flex : DisplayStyle.None;

        UpdateWindowSize();

        var duplicatePathValue = MegaPintAutoSaveData.DuplicatePathValue;
        
        var cleanPath = duplicatePathValue[(duplicatePathValue.IndexOf("/", StringComparison.Ordinal) + 1)..];

        if (cleanPath.Length <= 20)
            _path.text = duplicatePathValue;
        else
        {
            var text = "Assets/";

            if (cleanPath.Contains("/"))
            {
                text += ".../";
                var end = cleanPath.Split("/")[^1];
                text += end[..(end.Length >= 16 ? 16 : end.Length)];
            }
            else
                text += cleanPath[..(cleanPath.Length >= 16 ? 16 : cleanPath.Length)];

            _path.text = text;
        }
        
        _path.tooltip = duplicatePathValue;
    }

    private void UpdateWindowSize()
    {
        var y = _saveMode.index == 1 ? 150 : 130;

        minSize = new Vector2(300, y);
        maxSize = new Vector2(300, y);
    }

    #endregion

    #region Visual References

    /// <summary> Reference to the int field displaying the current interval </summary>
    private IntegerField _interval;

    private VisualElement _intervalOverwrite;

    /// <summary> Reference to the dropdown displaying the current saveMode </summary>
    private DropdownField _saveMode;
    
    private VisualElement _saveModeOverwrite;

    /// <summary> Reference to the toggle displaying the warning settings </summary>
    private Toggle _warning;
    
    private VisualElement _warningOverwrite;

    /// <summary> Reference to the GroupBox containing the path settings </summary>
    private GroupBox _duplicatePath;
    
    private VisualElement _duplicatePathOverwrite;

    /// <summary> Reference to the label displaying the duplicatePath </summary>
    private Label _path;

    /// <summary> Reference to the change button </summary>
    private Button _change;

    private Button _btnDone;

    private GroupBox _overwrites;

    #endregion

    #region Private

    /// <summary> Loaded reference of the uxml </summary>
    private VisualTreeAsset _baseWindow;

    private bool _intervalOverwritten;
    private bool _saveModeOverwritten;
    private bool _duplicatePathOverwritten;
    private bool _warningOverwritten;

    private bool _hasOverwrites;
    
    #endregion

    #region Overrides

    protected override string BasePath()
    {
        return "AutoSave/User Interface/MegaPintAutoSaveSettings";
    }

    public override MegaPintEditorWindowBase ShowWindow()
    {
        titleContent.text = "AutoSave - Settings";

        return this;
    }

    protected override void CreateGUI()
    {
        base.CreateGUI();

        VisualElement root = rootVisualElement;

        VisualElement content = _baseWindow.Instantiate();

        #region References

        _interval = content.Q <IntegerField>("Interval");
        _intervalOverwrite = content.Q <VisualElement>("Interval_Overwrite");
        
        _path = content.Q <Label>("Path");
        _duplicatePath = content.Q <GroupBox>("DuplicatePath");
        _duplicatePathOverwrite = content.Q <VisualElement>("DuplicatePath_Overwrite");

        _saveMode = content.Q <DropdownField>("SaveMode");
        _saveModeOverwrite = content.Q <VisualElement>("SaveMode_Overwrite");
        
        _warning = content.Q <Toggle>("Warning");
        _warningOverwrite = content.Q <VisualElement>("Warning_Overwrite");

        _change = content.Q <Button>("BTN_Change");

        _overwrites = content.Q <GroupBox>("Overwrites");

        _btnDone = content.Q <Button>("BTN_Done");

        #endregion

        RegisterCallbacks();
        
        _interval.value = MegaPintAutoSaveData.IntervalValue;
        _saveMode.index = MegaPintAutoSaveData.SaveModeValue;
        _warning.value = MegaPintAutoSaveData.WarningValue;

        // TODO load overwrites when there are some
        _intervalOverwritten = false;
        _saveModeOverwritten = false;
        _duplicatePathOverwritten = false;
        _warningOverwritten = false;
        _hasOverwrites = true;
        
        SetOverwrites();
        
        OnUpdateDuplicatePathGUI(null);

        root.Add(content);
    }

    private void SetOverwrites()
    {
        _overwrites.style.display = _hasOverwrites ? DisplayStyle.Flex : DisplayStyle.None;
        
        _intervalOverwrite.style.display = _intervalOverwritten ? DisplayStyle.Flex : DisplayStyle.None;
        _saveModeOverwrite.style.display = _saveModeOverwritten ? DisplayStyle.Flex : DisplayStyle.None;
        _duplicatePathOverwrite.style.display = _duplicatePathOverwritten ? DisplayStyle.Flex : DisplayStyle.None;
        _warningOverwrite.style.display = _warningOverwritten ? DisplayStyle.Flex : DisplayStyle.None;
    }

    protected override void OnDestroy()
    {
        var settings = MegaPintSettings.instance;

        if (settings == null)
            return;

        MegaPintAutoSaveData.IntervalValue = _interval.value;
        MegaPintAutoSaveData.SaveModeValue = _saveMode.index;
        MegaPintAutoSaveData.WarningValue = _warning.value;

        base.OnDestroy();
    }

    protected override bool LoadResources()
    {
        _baseWindow = Resources.Load <VisualTreeAsset>(BasePath());

        return _baseWindow != null;
    }

    protected override void RegisterCallbacks()
    {
        _interval.RegisterValueChangedCallback(OnIntervalChange);
        _saveMode.RegisterValueChangedCallback(OnUpdateDuplicatePathGUI);
        _btnDone.clickable = new Clickable(
            _ =>
            {
                Close();
            });

        _change.clicked += OnPathChange;
    }

    protected override void UnRegisterCallbacks()
    {
        _interval.RegisterValueChangedCallback(OnIntervalChange);
        _saveMode.UnregisterValueChangedCallback(OnUpdateDuplicatePathGUI);
        _btnDone.clickable = null;
        
        _change.clicked -= OnPathChange;
    }

    #endregion
}

}
#endif

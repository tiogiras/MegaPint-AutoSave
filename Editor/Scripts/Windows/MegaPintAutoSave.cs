#if UNITY_EDITOR
using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Editor.Scripts.Windows
{

public class MegaPintAutoSave : MegaPintEditorWindowBase
{
    #region Public Methods

    public override MegaPintEditorWindowBase ShowWindow()
    {
        minSize = new Vector2(300, 65);
        maxSize = new Vector2(300, 65);

        titleContent.text = "AutoSave";

        return this;
    }

    #endregion

    #region Protected Methods

    protected override string BasePath()
    {
        return "AutoSave/User Interface/MegaPintAutoSave";
    }

    protected override void CreateGUI()
    {
        base.CreateGUI();

        VisualElement root = rootVisualElement;

        VisualElement content = _baseWindow.Instantiate();

        #region References

        _nextSave = content.Q <Label>("NextSave");
        _nextSaveProgress = content.Q <ProgressBar>("NextSaveProgress");

        _playMode = content.Q <GroupBox>("PlayMode");
        _editMode = content.Q <GroupBox>("EditMode");

        _lastSave = content.Q <Label>("LastSave");
        _interval = content.Q <Label>("Interval");

        #endregion

        RegisterCallbacks();

        UpdateStaticGUI();

        if (EditorApplication.isPlaying)
        {
            _playMode.style.display = DisplayStyle.Flex;
            _editMode.style.display = DisplayStyle.None;
        }
        else
        {
            _playMode.style.display = DisplayStyle.None;
            _editMode.style.display = DisplayStyle.Flex;
            Task _ = Timer();
        }

        root.Add(content);
    }

    protected override bool LoadResources()
    {
        _baseWindow = Resources.Load <VisualTreeAsset>(BasePath());

        return _baseWindow != null;
    }

    protected override void RegisterCallbacks()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChange;

        MegaPintAutoSaveData.onSettingsChanged += UpdateStaticGUI;
    }

    protected override void UnRegisterCallbacks()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeChange;
        
        MegaPintAutoSaveData.onSettingsChanged -= UpdateStaticGUI;
    }

    #endregion

    #region Private Methods

    private void OnPlayModeChange(PlayModeStateChange state)
    {
        switch (state)
        {
            case PlayModeStateChange.EnteredEditMode:
                _breakTimer = false;
                Task _ = Timer();

                _playMode.style.display = DisplayStyle.None;
                _editMode.style.display = DisplayStyle.Flex;

                break;

            case PlayModeStateChange.ExitingEditMode:
                _breakTimer = true;

                _playMode.style.display = DisplayStyle.Flex;
                _editMode.style.display = DisplayStyle.None;

                break;
        }
    }

    private void Save()
    {
        var saveModeValue = MegaPintAutoSaveData.SaveModeValue;

        Scene scene = SceneManager.GetActiveScene();

        var destination = saveModeValue == 0
            ? scene.path
            : $"{MegaPintAutoSaveData.DuplicatePathValue}/{scene.name} ({DateTime.Today:MM.dd.yy})({DateTime.Now:HH.mm.ss}).unity";

        EditorSceneManager.SaveScene(scene, destination, saveModeValue == 1);
    }

    private async Task Timer()
    {
        while (!_breakTimer && this != null)
        {
            _currentSecond++;
            UpdateGUI();

            await Task.Delay(1000);
        }

        var warningValue = MegaPintAutoSaveData.WarningValue;

        if (this != null)
        {
            if (warningValue)
                EditorApplication.Beep();

            return;
        }

        if (warningValue)
            EditorApplication.Beep();
    }

    private void UpdateGUI()
    {
        var intervalValue = MegaPintAutoSaveData.IntervalValue;

        if (_currentSecond >= intervalValue)
        {
            Save();
            _currentSecond = 0;
            _lastSave.text = DateTime.Now.ToString("HH:mm:ss");
            _nextSave.text = DateTime.Now.AddSeconds(intervalValue).ToString("HH:mm:ss");
        }

        _nextSaveProgress.highValue = intervalValue;
        _nextSaveProgress.value = _currentSecond;
    }

    private void UpdateStaticGUI()
    {
        var intervalValue = MegaPintAutoSaveData.IntervalValue;

        _nextSave.text = DateTime.Now.AddSeconds(intervalValue).ToString("HH:mm:ss");
        _nextSaveProgress.highValue = intervalValue;
        _interval.text = $"{intervalValue} Seconds";
    }

    #endregion

    #region Visual References

    /// <summary> Reference to the progressbar displaying _currentSecond </summary>
    private ProgressBar _nextSaveProgress;

    /// <summary> Reference to the playMode group </summary>
    private GroupBox _playMode;

    /// <summary> Reference to the editMode group </summary>
    private GroupBox _editMode;

    /// <summary> Reference to the label displaying the time of the next save </summary>
    private Label _nextSave;

    /// <summary> Reference to the label displaying the time of the last save </summary>
    private Label _lastSave;

    /// <summary> Reference to the label displaying the current interval </summary>
    private Label _interval;

    #endregion

    #region Private

    /// <summary> Loaded reference of the uxml </summary>
    private VisualTreeAsset _baseWindow;

    private int _currentSecond;

    private bool _breakTimer;

    #endregion
}

}
#endif

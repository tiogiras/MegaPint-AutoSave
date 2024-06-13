#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaPint.Editor.Scripts.GUI.Utility;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using GUIUtility = MegaPint.Editor.Scripts.GUI.Utility.GUIUtility;

namespace MegaPint.Editor.Scripts.Windows
{

/// <summary> Window based on the <see cref="EditorWindowBase" /> to display and handle the autoSave </summary>
internal class AutoSave : EditorWindowBase
{
    private VisualTreeAsset _baseWindow;

    private bool _breakTimer;

    private Button _btnPlay;

    private Button _btnStop;

    private int _currentSecond;

    private GroupBox _editMode;

    private Label _interval;

    private Label _lastSave;

    private Label _nextSave;

    private ProgressBar _nextSaveProgress;

    private GroupBox _playMode;
    private bool _stopTimer;

    private bool _timerActive;

    #region Public Methods

    /// <summary> Show the window </summary>
    /// <returns> Window instance </returns>
    public override EditorWindowBase ShowWindow()
    {
        minSize = new Vector2(300, 90);
        maxSize = new Vector2(300, 90);

        titleContent.text = "AutoSave";

        SaveValues.AutoSave.WasActive = true;

        if (!SaveValues.AutoSave.ApplyPSAutoSaveWindow)
            return this;

        this.CenterOnMainWin();
        SaveValues.AutoSave.ApplyPSAutoSaveWindow = false;

        return this;
    }

    #endregion

    #region Protected Methods

    protected override string BasePath()
    {
        return Constants.AutoSave.UserInterface.AutoSaveWindow;
    }

    protected override void CreateGUI()
    {
        base.CreateGUI();

        VisualElement root = rootVisualElement;

        VisualElement content = GUIUtility.Instantiate(_baseWindow, root);
        content.style.flexGrow = 1f;
        content.style.flexShrink = 1f;

        #region References

        _nextSave = content.Q <Label>("NextSave");
        _nextSaveProgress = content.Q <ProgressBar>("NextSaveProgress");

        _playMode = content.Q <GroupBox>("PlayMode");
        _editMode = content.Q <GroupBox>("EditMode");

        _lastSave = content.Q <Label>("LastSave");
        _interval = content.Q <Label>("Interval");

        _btnPlay = _editMode.Q <Button>("BTN_Play");
        _btnStop = _editMode.Q <Button>("BTN_Stop");

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

            rootVisualElement.schedule.Execute(TryStartTimer);
        }
    }

    protected override bool LoadResources()
    {
        _baseWindow = Resources.Load <VisualTreeAsset>(BasePath());

        return _baseWindow != null;
    }

    protected override void RegisterCallbacks()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChange;

        SaveValues.AutoSave.onSettingsChanged += UpdateStaticGUI;

        _btnPlay.clicked += OnPlay;
        _btnStop.clicked += OnStop;
    }

    protected override void UnRegisterCallbacks()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeChange;

        SaveValues.AutoSave.onSettingsChanged -= UpdateStaticGUI;

        _btnPlay.clicked -= OnPlay;
        _btnStop.clicked -= OnStop;
    }

    #endregion

    #region Private Methods

    /// <summary> Get all currently opened scenes </summary>
    /// <returns> All collected scenes </returns>
    private static IEnumerable <Scene> GetAllScenes()
    {
        var countLoaded = SceneManager.sceneCount;

        Scene[] loadedScenes = new Scene[countLoaded];

        for (var i = 0; i < countLoaded; i++)
            loadedScenes[i] = SceneManager.GetSceneAt(i);

        return loadedScenes;
    }

    /// <summary> Save the opened scenes </summary>
    private static void Save()
    {
        var saveModeValue = SaveValues.AutoSave.SaveMode;

        IEnumerable <Scene> scenes = GetAllScenes();

        foreach (Scene scene in scenes)
        {
            var destination = saveModeValue == 0
                ? scene.path
                : $"{SaveValues.AutoSave.DuplicatePath}/{scene.name} ({DateTime.Today:MM.dd.yy})({DateTime.Now:HH.mm.ss}).unity";

            EditorSceneManager.SaveScene(scene, destination, saveModeValue == 1);
        }
    }

    /// <summary> Change the button styles based on the current active state </summary>
    /// <param name="active"> If the autoSave feature is active </param>
    private void ChangeButtonStates(bool active)
    {
        _btnPlay.pickingMode = active ? PickingMode.Ignore : PickingMode.Position;
        _btnPlay.style.opacity = active ? .5f : 1f;
        _btnPlay.focusable = !active;

        _btnStop.pickingMode = active ? PickingMode.Position : PickingMode.Ignore;
        _btnStop.style.opacity = active ? 1f : .5f;
        _btnStop.focusable = active;
    }

    /// <summary> Play button callback </summary>
    private void OnPlay()
    {
        if (_timerActive)
            return;

        SaveValues.AutoSave.WasActive = true;

#pragma warning disable CS4014
        Timer();
#pragma warning restore CS4014
    }

    /// <summary> Callback for when the editor changes playmode </summary>
    /// <param name="state"> New playmode </param>
    private void OnPlayModeChange(PlayModeStateChange state)
    {
        switch (state)
        {
            case PlayModeStateChange.EnteredEditMode:
                _breakTimer = false;
                TryStartTimer();

                _playMode.style.display = DisplayStyle.None;
                _editMode.style.display = DisplayStyle.Flex;

                break;

            case PlayModeStateChange.ExitingEditMode:
                _breakTimer = true;

                _playMode.style.display = DisplayStyle.Flex;
                _editMode.style.display = DisplayStyle.None;

                break;

            case PlayModeStateChange.EnteredPlayMode:
                return;

            case PlayModeStateChange.ExitingPlayMode:
                return;

            default:
                return;
        }
    }

    /// <summary> Stop button callback </summary>
    private void OnStop()
    {
        _stopTimer = true;
    }

    /// <summary> Timer method for the autoSave feature </summary>
    private async Task Timer()
    {
        _timerActive = true;

        ToggleGUI(true);
        ChangeButtonStates(true);

        while (!_breakTimer && !_stopTimer && this != null)
        {
            _currentSecond++;
            UpdateGUI();

            await TryWaitOneSecond();
        }

        _stopTimer = false;
        _timerActive = false;
        _currentSecond = 0;

        ToggleGUI(false);
        ChangeButtonStates(false);

        if (_breakTimer && !_stopTimer)
            return;

        if (SaveValues.AutoSave.Warning)
        {
            EditorApplication.Beep();
            Debug.LogWarning("AutoSave feature was stopped!");
        }

        SaveValues.AutoSave.WasActive = false;
    }

    /// <summary> Toggle specific parts GUI </summary>
    /// <param name="active"> Active state of the ui parts </param>
    private void ToggleGUI(bool active)
    {
        if (active)
            UpdateGUI(true);

        _lastSave.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
        _nextSave.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
        _nextSaveProgress.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
    }

    /// <summary> Try to start the timer, fails if the timer was not playing previously </summary>
    private void TryStartTimer()
    {
        var wasActive = SaveValues.AutoSave.WasActive;

        ToggleGUI(wasActive);
        ChangeButtonStates(wasActive);

        if (!wasActive)
            return;

        Task _ = Timer();
    }

    /// <summary> Try to wait one second break if the timer should be stopped </summary>
    private async Task TryWaitOneSecond()
    {
        for (var i = 0; i < 10; i++)
        {
            if (_breakTimer || _stopTimer)
                return;

            await Task.Delay(100);
        }
    }

    /// <summary> Update the gui </summary>
    /// <param name="forceUpdateNextSave"> Forces the gui to update after the next auto save </param>
    private void UpdateGUI(bool forceUpdateNextSave = false)
    {
        var intervalValue = SaveValues.AutoSave.Interval;

        if (_currentSecond >= intervalValue)
        {
            Save();
            _currentSecond = 0;
            _lastSave.text = DateTime.Now.ToString("HH:mm:ss");
            _nextSave.text = DateTime.Now.AddSeconds(intervalValue).ToString("HH:mm:ss");
        }

        if (forceUpdateNextSave)
            _nextSave.text = DateTime.Now.AddSeconds(intervalValue).ToString("HH:mm:ss");

        _nextSaveProgress.highValue = intervalValue;
        _nextSaveProgress.value = _currentSecond;
    }

    /// <summary> Update all static gui parts </summary>
    private void UpdateStaticGUI()
    {
        var intervalValue = SaveValues.AutoSave.Interval;

        _nextSave.text = DateTime.Now.AddSeconds(intervalValue).ToString("HH:mm:ss");
        _nextSaveProgress.highValue = intervalValue;
        _interval.text = $"{intervalValue} Seconds";
    }

    #endregion
}

}
#endif

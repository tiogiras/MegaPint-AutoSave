#if UNITY_EDITOR
using System;
using MegaPint.Editor.Scripts.GUI.Utility;
using MegaPint.Editor.Scripts.Logic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using GUIUtility = MegaPint.Editor.Scripts.GUI.Utility.GUIUtility;

namespace MegaPint.Editor.Scripts.Windows
{

/// <summary> Window based on the <see cref="EditorWindowBase" /> to display and handle the autoSave </summary>
internal class AutoSave : EditorWindowBase
{
    private VisualTreeAsset _baseWindow;

    private Button _btnPlay;
    private Button _btnStop;

    private GroupBox _playMode;
    private GroupBox _editMode;

    private Label _interval;
    private Label _lastSave;
    private Label _nextSave;

    private ProgressBar _nextSaveProgress;

    #region Public Methods

    /// <summary> Show the window </summary>
    /// <returns> Window instance </returns>
    public override EditorWindowBase ShowWindow()
    {
        minSize = new Vector2(300, 90);
        maxSize = new Vector2(300, 90);

        titleContent.text = "AutoSave";

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

        _nextSaveProgress.style.display = DisplayStyle.None;
        _nextSave.style.display = DisplayStyle.None;

        UpdatePlayEditModeGUI();
        UpdateStaticGUI();
        
        ChangeButtonStates(SaveValues.AutoSave.IsActive);
    }

    private void UpdatePlayEditModeGUI()
    {
        if (EditorApplication.isPlaying)
        {
            _playMode.style.display = DisplayStyle.Flex;
            _editMode.style.display = DisplayStyle.None;
        }
        else
        {
            _playMode.style.display = DisplayStyle.None;
            _editMode.style.display = DisplayStyle.Flex;
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
        
        AutoSaveTimer.onTimerTick += OnTimerTick;
        AutoSaveTimer.onTimerStarted += OnTimerStarted;
        AutoSaveTimer.onTimerStopped += OnTimerStopped;
    }

    protected override void UnRegisterCallbacks()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeChange;

        SaveValues.AutoSave.onSettingsChanged -= UpdateStaticGUI;
        
        _btnPlay.clicked -= OnPlay;
        _btnStop.clicked -= OnStop;
        
        AutoSaveTimer.onTimerTick -= OnTimerTick;
        AutoSaveTimer.onTimerStarted -= OnTimerStarted;
        AutoSaveTimer.onTimerStopped -= OnTimerStopped;
    }

    private static void OnPlay()
    {
        SaveValues.AutoSave.IsActive = true;
    }

    private static void OnStop()
    {
        SaveValues.AutoSave.IsActive = false;
    }

    private void OnTimerStarted()
    {
        ChangeButtonStates(true);
        
        UpdateStaticGUI();
        UpdateGUI(0, true);
        
        _nextSaveProgress.style.display = DisplayStyle.Flex;
        _nextSave.style.display = DisplayStyle.Flex;
    }

    private void OnTimerStopped()
    {
        ChangeButtonStates(false);
        
        UpdateGUI(0, false);
        
        _nextSaveProgress.style.display = DisplayStyle.None;
        _nextSave.style.display = DisplayStyle.None;  
    }

    #endregion

    private void OnTimerTick(int tick)
    {
        if (_nextSaveProgress.style.display == DisplayStyle.None)
        {
            _nextSaveProgress.style.display = DisplayStyle.Flex;
            _nextSave.style.display = DisplayStyle.Flex;
            
            var remainingSeconds = SaveValues.AutoSave.Interval - tick;
            _nextSave.text = DateTime.Now.AddSeconds(remainingSeconds).ToString("HH:mm:ss");
        }
        
        UpdateGUI(tick, false);
    }
    
    #region Private Methods

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

    /// <summary> Callback for when the editor changes playmode </summary>
    /// <param name="state"> New playmode </param>
    private void OnPlayModeChange(PlayModeStateChange state)
    {
        UpdatePlayEditModeGUI();
    }

    /// <summary> Update the gui </summary>
    /// <param name="currentSecond"> Current second of the timer </param>
    /// <param name="forceUpdateNextSave"> Forces the gui to update after the next auto save </param>
    private void UpdateGUI(int currentSecond, bool forceUpdateNextSave)
    {
        var intervalValue = SaveValues.AutoSave.Interval;

        if (currentSecond >= intervalValue)
        {
            _lastSave.text = DateTime.Now.ToString("HH:mm:ss");
            _nextSave.text = DateTime.Now.AddSeconds(intervalValue).ToString("HH:mm:ss");
        }

        if (forceUpdateNextSave)
            _nextSave.text = DateTime.Now.AddSeconds(intervalValue).ToString("HH:mm:ss");

        _nextSaveProgress.highValue = intervalValue;
        _nextSaveProgress.value = currentSecond;
    }

    /// <summary> Update all static gui parts </summary>
    private void UpdateStaticGUI()
    {
        var intervalValue = SaveValues.AutoSave.Interval;
        
        _nextSaveProgress.highValue = intervalValue;
        _interval.text = $"{intervalValue} Seconds";
    }

    #endregion
}

}
#endif

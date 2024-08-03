#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace MegaPint.Editor.Scripts.Logic
{

/// <summary> Handles the timer of the autosave feature </summary>
[InitializeOnLoad]
internal static class AutoSaveTimer
{
    public static Action onTimerStarted;
    public static Action onTimerStopped;
    public static Action <int> onTimerTick;

    public static Action onTimerSaving;
    public static Action onTimerSaved;

    private static int s_currentSecond;

    private static int s_interval;
    private static int s_saveMode;
    private static string s_duplicatePath;

    private static bool s_timerRunning;

    private static bool s_breakTimer;

    static AutoSaveTimer()
    {
        OnSettingsChanged();

        SaveValues.AutoSave.onIsActiveChanged += OnIsActiveChanged;
        SaveValues.AutoSave.onSettingsChanged += OnSettingsChanged;
        EditorApplication.playModeStateChanged += OnPlayModeChange;

        TryStartRunning();
    }

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

    /// <summary> Callback for when the toggle state changed </summary>
    /// <param name="newValue"> New toggle state </param>
    private static void OnIsActiveChanged(bool newValue)
    {
        if (newValue)
            StartTimer();
        else
            StopTimer();
    }

    /// <summary> Callback for changing the playmode </summary>
    /// <param name="evt"> Callback event </param>
    /// <exception cref="ArgumentOutOfRangeException"> State not found </exception>
    private static void OnPlayModeChange(PlayModeStateChange evt)
    {
        switch (evt)
        {
            case PlayModeStateChange.EnteredEditMode:
                TryStartRunning();

                break;

            case PlayModeStateChange.ExitingEditMode:
                break;

            case PlayModeStateChange.EnteredPlayMode:
                StopTimer();

                break;

            case PlayModeStateChange.ExitingPlayMode:
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(evt), evt, null);
        }
    }

    /// <summary> Callback for when any setting is changed </summary>
    private static void OnSettingsChanged()
    {
        s_interval = SaveValues.AutoSave.Interval;
        s_saveMode = SaveValues.AutoSave.SaveMode;
        s_duplicatePath = SaveValues.AutoSave.DuplicatePath;
    }

    /// <summary> Save the opened scenes </summary>
    private static void Save()
    {
        onTimerSaving?.Invoke();

        IEnumerable <Scene> scenes = GetAllScenes();

        foreach (Scene scene in scenes)
        {
            var destination = s_saveMode == 0
                ? scene.path
                : $"{s_duplicatePath}/{scene.name} ({DateTime.Today:MM.dd.yy})({DateTime.Now:HH.mm.ss}).unity";

            EditorSceneManager.SaveScene(scene, destination, s_saveMode == 1);
        }

        onTimerSaved?.Invoke();
    }

    /// <summary> Start the timer </summary>
    private static void StartTimer()
    {
        if (!s_timerRunning)
#pragma warning disable CS4014
            Timer();
#pragma warning restore CS4014
    }

    /// <summary> Stop the timer </summary>
    private static void StopTimer()
    {
        if (s_timerRunning)
            s_breakTimer = true;
    }

    /// <summary> Timer method for the autoSave feature </summary>
    private static async Task Timer()
    {
        s_timerRunning = true;
        s_currentSecond = 0;
        onTimerStarted?.Invoke();

        while (await TryWaitOneSecond())
        {
            s_currentSecond++;
            onTimerTick?.Invoke(s_currentSecond);

            if (s_currentSecond < s_interval)
                continue;

            s_currentSecond = 0;
            Save();
        }

        s_timerRunning = false;
        onTimerStopped?.Invoke();
    }

    /// <summary> Try to start the timer when it was running before </summary>
    private static void TryStartRunning()
    {
        if (SaveValues.AutoSave.IsActive)
            StartTimer();
    }

    /// <summary> Try to wait one second break if the timer should be stopped </summary>
    private static async Task <bool> TryWaitOneSecond()
    {
        const int CycleCount = 10;

        for (var i = 0; i < CycleCount; i++)
        {
            if (s_breakTimer)
            {
                s_breakTimer = false;

                return false;
            }

            await Task.Delay(1000 / CycleCount);
        }

        return true;
    }

    #endregion
}

}
#endif

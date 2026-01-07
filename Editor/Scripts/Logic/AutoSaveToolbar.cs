#if UNITY_EDITOR
using MegaPint.Editor.Scripts.GUI.Utility;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

namespace MegaPint.Editor.Scripts.Logic
{

/// <summary> Handles the toolbar of the AutoSave package </summary>
[InitializeOnLoad]
public class AutoSaveToolbar
{
    private const string _CategoryPath = "MegaPint/AutoSave/";
    private const string _TogglePath = _CategoryPath + "Toggle";
    private const string _SliderPath = _CategoryPath + "Slider";

    private static int s_currentTick;

    static AutoSaveToolbar()
    {
        if (!SaveValues.AutoSave.ToolbarInitialized)
        {
            MainToolbarUtility.ForceShowElement(_CategoryPath, () =>
            {
                SaveValues.AutoSave.ToolbarInitialized = true;
            });
        }

        AutoSaveTimer.onTimerTick += tick =>
        {
            s_currentTick = tick;
            MainToolbar.Refresh(_SliderPath);
        };

        AutoSaveTimer.onTimerStarted += () => {MainToolbar.Refresh(_TogglePath);};
        AutoSaveTimer.onTimerStopped += () =>
        {
            s_currentTick = 0;
            MainToolbar.Refresh(_TogglePath);
            MainToolbar.Refresh(_SliderPath);
        };
        
        SaveValues.BasePackage.onUseIconsChanged += _ => {MainToolbar.Refresh(_TogglePath);};
        SaveValues.AutoSave.onIntervalChanged += _ => {MainToolbar.Refresh(_SliderPath);};
    }

    [MainToolbarElement(_TogglePath, defaultDockPosition = MainToolbarDockPosition.Right)]
    public static MainToolbarElement AutoSaveButton()
    {
        var icon = Resources.Load <Texture2D>(Constants.AutoSave.Images.ToolbarButton);
        const string text = "Auto Save";

        MainToolbarContent content = SaveValues.BasePackage.UseToolbarIcons ? new MainToolbarContent(icon) : new MainToolbarContent(text);
        content.tooltip = SaveValues.AutoSave.IsActive ? "Auto Save: Active" : "Auto Save: Inactive";

        var element = new MainToolbarToggle(
            content,
            SaveValues.AutoSave.IsActive,
            newValue =>
            {
                SaveValues.AutoSave.IsActive = newValue;
                MainToolbar.Refresh(_TogglePath);
            });

        return element;
    }

    [MainToolbarElement(_SliderPath, defaultDockPosition = MainToolbarDockPosition.Right)]
    public static MainToolbarElement AutoSaveSlider()
    {
        var content = new MainToolbarContent
        {
            tooltip = "Remaining time until next scene save",
        };

        var max = SaveValues.AutoSave.Interval;
        var remaining = Mathf.Clamp(max - s_currentTick, 0, max);

        var element = new MainToolbarSlider(
            content,
            remaining,
            0,
            max,
            null)
        {
            enabled = false,
        };

        return element;
    }
}

}
#endif

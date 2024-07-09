// TODO commenting

#if UNITY_EDITOR
using System.Text;
using MegaPint.Editor.Scripts.GUI;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace MegaPint.Editor.Scripts.Logic
{

[InitializeOnLoad]
internal static class AutoSaveToolbar
{
    private static ToolbarToggle s_toolbarToggle;
    
    static AutoSaveToolbar()
    {
        if (!SaveValues.AutoSave.DisplayToolbarToggle)
            return;
        
        ToolbarExtension.AddRightZoneAction(
            new ToolbarExtension.GUIAction
            {
                executionIndex = 0,
                action = root =>
                {
                    if (SaveValues.BasePackage.UseToolbarIcons)
                    {
                        s_toolbarToggle = ToolbarExtension.CreateToolbarToggle(
                            Constants.AutoSave.UserInterface.ToolbarButton,
                            OnToolbarCreation,
                            OnToolbarToggleChanged);
                    }
                    else
                        s_toolbarToggle = ToolbarExtension.CreateToolbarToggle("Auto Save", OnToolbarToggleChanged);

                    s_toolbarToggle.value = SaveValues.AutoSave.IsActive;
                    root.Add(s_toolbarToggle);

                    AutoSaveTimer.onTimerStarted += () =>
                    {
                        s_toolbarToggle.SetValueWithoutNotify(true);
                    };

                    AutoSaveTimer.onTimerStopped += () =>
                    {
                        s_toolbarToggle.SetValueWithoutNotify(false);
                    };
                    
                    AutoSaveTimer.onTimerTick += tick => {s_toolbarToggle.tooltip = GetTooltip(tick);};

                    s_toolbarToggle.tooltip = GetTooltip(0);
                }
            });
    }

    #region Private Methods

    private static string GetTooltip(int tick)
    {
        var isActive = SaveValues.AutoSave.IsActive;

        var tooltip = new StringBuilder();
        tooltip.Append($"State: {(isActive ? "active" : "inactive")}");

        if (isActive)
            tooltip.Append($"\nAutoSave in {SaveValues.AutoSave.Interval - tick} seconds");

        return tooltip.ToString();
    }

    private static void OnToolbarCreation(VisualElement element)
    {
    }

    private static void OnToolbarToggleChanged(bool newValue)
    {
        SaveValues.AutoSave.IsActive = newValue;
        
        s_toolbarToggle.tooltip = GetTooltip(0);
    }

    #endregion
}

}
#endif

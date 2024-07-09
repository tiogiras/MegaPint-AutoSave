#if UNITY_EDITOR
using System;
using MegaPint.Editor.Scripts.Settings;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using GUIUtility = MegaPint.Editor.Scripts.GUI.Utility.GUIUtility;

namespace MegaPint.Editor.Scripts
{

/// <summary> Partial class used to display the right pane in the BaseWindow </summary>
internal static partial class DisplayContent
{
    #region Private Methods

    // Called by reflection
    // ReSharper disable once UnusedMember.Local
    private static void AutoSave(DisplayContentReferences refs)
    {
        InitializeDisplayContent(
            refs,
            new TabSettings {info = true, settings = true},
            new TabActions
            {
                info = root =>
                {
                    GUIUtility.ActivateLinks(
                        root,
                        link =>
                        {
                            switch (link.linkID)
                            {
                                case "autoSavePath":
                                    EditorApplication.ExecuteMenuItem(
                                        link.linkText);

                                    break;
                                    
                                case "autoSave":
                                    ContextMenu.AutoSave.OpenAutoSave();

                                    break;
                            }
                        });
                },
                settings = AutoSaveSettingsTab
            });
    }

    /// <summary> Change the path the saved scenes are saved in </summary>
    /// <param name="pathLabel"> Label the path is displayed in </param>
    /// <param name="btnSave"> Button to save the new path </param>
    private static void AutoSavePathChange(TextElement pathLabel, VisualElement btnSave)
    {
        var oldValue = SaveValues.AutoSave.DuplicatePath;

        var path = EditorUtility.OpenFolderPanel("Set folder for duplicates", "Assets/", "");

        if (string.IsNullOrEmpty(path))
            return;

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
            path = path.Insert(0, "Assets");

            SaveValues.AutoSave.DuplicatePath = path;

            AutoSavePathVisuals(pathLabel);
        }

        if (!path.Equals(oldValue))
            btnSave.style.display = DisplayStyle.Flex;
    }

    /// <summary> Display the autoSave path </summary>
    /// <param name="path"> Path to display </param>
    private static void AutoSavePathVisuals(TextElement path)
    {
        var duplicatePathValue = SaveValues.AutoSave.DuplicatePath;

        var cleanPath = duplicatePathValue[(duplicatePathValue.IndexOf("/", StringComparison.Ordinal) + 1)..];

        if (cleanPath.Length <= 20)
            path.text = duplicatePathValue;
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

            path.text = text;
        }

        path.tooltip = duplicatePathValue;
    }

    /// <summary> Handle the logic for the autoSave settings tab </summary>
    /// <param name="root"> Root element of the tab </param>
    private static void AutoSaveSettingsTab(VisualElement root)
    {
        var interval = root.Q <IntegerField>("Interval");
        var saveMode = root.Q <DropdownField>("SaveMode");
        var warning = root.Q <Toggle>("Warning");
        var displayToolbarToggle = root.Q <Toggle>("DisplayToolbarToggle");

        var duplicatePath = root.Q <GroupBox>("DuplicatePath");
        var btnChange = root.Q <Button>("BTN_Change");
        var btnSave = root.Q <Button>("BTN_Save");
        var path = root.Q <Label>("Path");

        interval.value = SaveValues.AutoSave.Interval;
        saveMode.index = SaveValues.AutoSave.SaveMode;
        warning.value = SaveValues.AutoSave.Warning;
        displayToolbarToggle.value = SaveValues.AutoSave.DisplayToolbarToggle;

        duplicatePath.style.display = saveMode.index == 1 ? DisplayStyle.Flex : DisplayStyle.None;
        btnSave.style.display = DisplayStyle.None;

        AutoSavePathVisuals(path);

        interval.RegisterValueChangedCallback(
            evt =>
            {
                if (evt.newValue < 1)
                {
                    interval.value = 1;
                    return;
                }

                if (evt.newValue != SaveValues.AutoSave.Interval)
                    btnSave.style.display = DisplayStyle.Flex;
            });

        warning.RegisterValueChangedCallback(
            evt =>
            {
                if (evt.newValue != SaveValues.AutoSave.Warning)
                    btnSave.style.display = DisplayStyle.Flex;
            });

        saveMode.RegisterValueChangedCallback(
            _ =>
            {
                duplicatePath.style.display = saveMode.index == 1 ? DisplayStyle.Flex : DisplayStyle.None;

                if (saveMode.index != SaveValues.AutoSave.SaveMode)
                    btnSave.style.display = DisplayStyle.Flex;
            });

        displayToolbarToggle.RegisterValueChangedCallback(
            evt => {SaveValues.AutoSave.DisplayToolbarToggle = evt.newValue;});

        btnChange.clicked += () => {AutoSavePathChange(path, btnSave);};

        btnSave.clicked += () =>
        {
            SaveValues.AutoSave.Interval = interval.value;
            SaveValues.AutoSave.SaveMode = saveMode.index;
            SaveValues.AutoSave.Warning = warning.value;
            SaveValues.AutoSave.DuplicatePath = path.tooltip;

            MegaPintSettings.Save();

            SaveValues.AutoSave.onSettingsChanged?.Invoke();

            btnSave.style.display = DisplayStyle.None;
        };
    }

    #endregion
}

}
#endif

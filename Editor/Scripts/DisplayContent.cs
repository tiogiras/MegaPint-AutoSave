#if UNITY_EDITOR
using System;
using Editor.Scripts.Settings;
using Editor.Scripts.Windows;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using GUIUtility = Editor.Scripts.GUI.GUIUtility;

namespace Editor.Scripts
{

internal static partial class DisplayContent
{
    // Called by reflection
    // ReSharper disable once UnusedMember.Local
    private static void AutoSave(DisplayContentReferences refs)
    {
        InitializeDisplayContent(
            refs,
            new TabSettings
            {
                info = true,
                settings = true
            },
            new TabActions
            {
                info = root =>
                {
                    GUIUtility.ActivateLinks(root,
                                             link =>
                                             {
                                                 switch (link.linkID)
                                                 {
                                                     case "autosave":
                                                         EditorApplication.ExecuteMenuItem(
                                                             link.linkText);
                                                         break;
                                                 }
                                             });
                },
                settings = AutoSaveSettingsTab
            });
    }
    
    private static void AutoSavePathChange(Label pathLabel, VisualElement btnSave)
    {
        var oldValue = MegaPintAutoSaveData.DuplicatePathValue;

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
            path = path.Insert(0, "Assets");

            MegaPintAutoSaveData.DuplicatePathValue = path;

            AutoSavePathVisuals(pathLabel);
        }

        if (!path.Equals(oldValue))
            btnSave.style.display = DisplayStyle.Flex;
    }
    
    private static void AutoSavePathVisuals(Label path)
    {
        var duplicatePathValue = MegaPintAutoSaveData.DuplicatePathValue;

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

    private static void AutoSaveSettingsTab(VisualElement root)
    {
        #region Collect References

        var interval = root.Q <IntegerField>("Interval");
        var saveMode = root.Q <DropdownField>("SaveMode");
        var warning = root.Q <Toggle>("Warning");

        var duplicatePath = root.Q <GroupBox>("DuplicatePath");
        var btnChange = root.Q <Button>("BTN_Change");
        var btnSave = root.Q <Button>("BTN_Save");
        var path = root.Q <Label>("Path");

        #endregion

        #region Set initial Values

        interval.value = MegaPintAutoSaveData.IntervalValue;
        saveMode.index = MegaPintAutoSaveData.SaveModeValue;
        warning.value = MegaPintAutoSaveData.WarningValue;

        #endregion

        #region Set initial Visuals

        duplicatePath.style.display = saveMode.index == 1 ? DisplayStyle.Flex : DisplayStyle.None;
        btnSave.style.display = DisplayStyle.None;

        AutoSavePathVisuals(path);

        #endregion

        #region Register Callbacks

        interval.RegisterValueChangedCallback(
            evt =>
            {
                if (evt.newValue != MegaPintAutoSaveData.IntervalValue)
                    btnSave.style.display = DisplayStyle.Flex;
            });

        warning.RegisterValueChangedCallback(
            evt =>
            {
                if (evt.newValue != MegaPintAutoSaveData.WarningValue)
                    btnSave.style.display = DisplayStyle.Flex;
            });

        saveMode.RegisterValueChangedCallback(
            _ =>
            {
                duplicatePath.style.display = saveMode.index == 1 ? DisplayStyle.Flex : DisplayStyle.None;

                if (saveMode.index != MegaPintAutoSaveData.SaveModeValue)
                    btnSave.style.display = DisplayStyle.Flex;
            });

        btnChange.clicked += () => {AutoSavePathChange(path, btnSave);};

        btnSave.clicked += () =>
        {
            MegaPintAutoSaveData.IntervalValue = interval.value;
            MegaPintAutoSaveData.SaveModeValue = saveMode.index;
            MegaPintAutoSaveData.WarningValue = warning.value;
            MegaPintAutoSaveData.DuplicatePathValue = path.tooltip;

            MegaPintSettings.Save();

            MegaPintAutoSaveData.onSettingsChanged?.Invoke();

            btnSave.style.display = DisplayStyle.None;
        };

        #endregion
    }
}

}
#endif

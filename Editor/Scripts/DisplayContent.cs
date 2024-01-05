#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Scripts
{

internal static partial class DisplayContent
{
    private const string BasePathAutoSave = "AutoSave/User Interface/Display Content Tabs/";
    private const string AutoSaveSceneEntryPath = "AutoSave/User Interface/MegaPintAutoSaveSceneListEntry";

    #region Private Methods

    // Called by reflection
    private static void AutoSave(VisualElement root)
    {
        var tabs = root.Q <GroupBox>("Tabs");
        var tabContentParent = root.Q <GroupBox>("TabContent");

        RegisterTabCallbacks(tabs, tabContentParent, 4);

        SetTabContentLocations(BasePathAutoSave + "Tab0", BasePathAutoSave + "Tab1", BasePathAutoSave + "Tab2", BasePathAutoSave + "Tab3");

        s_onSelectedTabChanged += OnTabChangedAutoSave;
        s_onSelectedPackageChanged += UnsubscribeAutoSave;

        SwitchTab(tabContentParent, 0);
    }

    private static void AutoSavePathChange(Label pathLabel, string targetGuid = "")
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
            path = path.Insert(0, "Assets");
            
            if (string.IsNullOrEmpty(targetGuid))
                MegaPintAutoSaveData.DuplicatePathValue = path;
            else 
                MegaPintAutoSaveData.SetSceneOverwrites(targetGuid, new MegaPintAutoSaveData.Overwrites {duplicatePath = path}, false);
            
            AutoSavePathVisuals(pathLabel);
        }
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

    private static void AutoSaveTab1(VisualElement root)
    {
        var interval = root.Q <IntegerField>("Interval");
        var saveMode = root.Q <DropdownField>("SaveMode");
        var warning = root.Q <Toggle>("Warning");

        var duplicatePath = root.Q <GroupBox>("DuplicatePath");
        var btnChange = root.Q <Button>("BTN_Change");
        var btnSave = root.Q <Button>("BTN_Save");
        var path = root.Q <Label>("Path");

        interval.value = MegaPintAutoSaveData.IntervalValue;
        saveMode.index = MegaPintAutoSaveData.SaveModeValue;
        warning.value = MegaPintAutoSaveData.WarningValue;

        duplicatePath.style.display = saveMode.index == 1 ? DisplayStyle.Flex : DisplayStyle.None;
        btnSave.style.display = DisplayStyle.None;

        AutoSavePathVisuals(path);

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

        btnChange.clicked += () => {AutoSavePathChange(path);};

        btnSave.clicked += () =>
        {
            MegaPintAutoSaveData.IntervalValue = interval.value;
            MegaPintAutoSaveData.SaveModeValue = saveMode.index;
            MegaPintAutoSaveData.WarningValue = warning.value;
            MegaPintAutoSaveData.DuplicatePathValue = path.tooltip;

            btnSave.style.display = DisplayStyle.None;
        };
    }

    private static void OnTabChangedAutoSave(int tab, VisualElement root)
    {
        switch (tab)
        {
            case 1:
                AutoSaveTab1(root);
                break;
            
            case 2:
                AutoSaveTab2(root);
                break;
        }
    }

    private static void AutoSaveTab2(VisualElement root)
    {
        var sceneList = root.Q <ListView>("Scenes");

        sceneList.makeItem = () => new VisualElement();

        var labelTemplate = Resources.Load <VisualTreeAsset>(AutoSaveSceneEntryPath);
        
        sceneList.bindItem = (element, i) =>
        {
            element.Clear();

            var guid = MegaPintAutoSaveData.SceneGuids[i];
            
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var sceneAsset = AssetDatabase.LoadAssetAtPath <SceneAsset>(path);
            var sceneName = sceneAsset.name;

            TemplateContainer entry = labelTemplate.Instantiate();
            var label = entry.Q <Label>();
            label.text = sceneName;
            label.tooltip = sceneName;

            entry.Q <Button>().clickable = new Clickable(
                () =>
                {
                    if (!EditorUtility.DisplayDialog(
                            "Remove Scene",
                            $"Do you want to remove all settings corresponding to the following scene? \n {sceneName}",
                            "Yes",
                            "No"))
                        return;

                    MegaPintAutoSaveData.RemoveSceneOverwrites(guid);
                    
                    sceneList.itemsSource = MegaPintAutoSaveData.SceneGuids;
                    sceneList.RefreshItems();
                });

            element.Add(entry);
        };

        sceneList.unbindItem = (element, i) =>
        {
            element.Q <Button>().clickable = null;
        };

        sceneList.itemsSource = MegaPintAutoSaveData.SceneGuids;
        sceneList.RefreshItems();
        sceneList.ClearSelection();
        
        root.Q <Button>("BTN_AddScene").clicked += () =>
        {
            var controlID = GUIUtility.GetControlID(FocusType.Passive);
            EditorGUIUtility.ShowObjectPicker <SceneAsset>(null, false, "", controlID);

            onRightPaneGUI += AutoSaveWaitForObjectPicker;
        };

        var sceneSettings = root.Q <GroupBox>("SceneSettings");
        sceneSettings.style.display = DisplayStyle.None;

        var sceneName = sceneSettings.Q <Label>("SceneName");

        var interval = sceneSettings.Q <IntegerField>("Interval");
        var intervalOverwrite = sceneSettings.Q <VisualElement>("Interval_Overwrite");
        
        var saveMode = sceneSettings.Q <DropdownField>("SaveMode");
        var saveModeOverwrite = sceneSettings.Q <VisualElement>("SaveMode_Overwrite");

        var duplicatePath = sceneSettings.Q <GroupBox>("DuplicatePath");
        var path = duplicatePath.Q <Label>("Path");
        var duplicatePathOverwrite = sceneSettings.Q <VisualElement>("DuplicatePath_Overwrite");
        var btnChange = duplicatePath.Q <Button>("BTN_Change");
        
        var warning = sceneSettings.Q <Toggle>("Warning");
        var warningOverwrite = sceneSettings.Q <VisualElement>("Warning_Overwrite");

        var btnSave = sceneSettings.Q <Button>("BTN_Save");
        
        btnSave.style.display = DisplayStyle.None;

        interval.RegisterValueChangedCallback(
            evt =>
            {
                var guid = MegaPintAutoSaveData.SceneGuids[sceneList.selectedIndex];
                
                if (evt.newValue != MegaPintAutoSaveData.GetSceneInterval(guid))
                {
                    btnSave.style.display = DisplayStyle.Flex;
                    intervalOverwrite.style.display = DisplayStyle.Flex;
                }
                else
                    intervalOverwrite.style.display = DisplayStyle.None;
            });
        
        warning.RegisterValueChangedCallback(
            evt =>
            {
                var guid = MegaPintAutoSaveData.SceneGuids[sceneList.selectedIndex];
                
                if (evt.newValue != MegaPintAutoSaveData.GetSceneWarning(guid))
                {
                    btnSave.style.display = DisplayStyle.Flex;
                    warningOverwrite.style.display = DisplayStyle.Flex;
                }
                else
                    warningOverwrite.style.display = DisplayStyle.None;
            });

        duplicatePath.style.display = saveMode.index == 1 ? DisplayStyle.Flex : DisplayStyle.None;
        
        saveMode.RegisterValueChangedCallback(
            _ =>
            {
                duplicatePath.style.display = saveMode.index == 1 ? DisplayStyle.Flex : DisplayStyle.None;

                var guid = MegaPintAutoSaveData.SceneGuids[sceneList.selectedIndex];
                
                if (saveMode.index != MegaPintAutoSaveData.GetSceneSaveMode(guid))
                {
                    btnSave.style.display = DisplayStyle.Flex;
                    saveModeOverwrite.style.display = DisplayStyle.Flex;
                }
                else
                    saveModeOverwrite.style.display = DisplayStyle.None;
            });

        btnChange.clicked += () =>
        {
            AutoSavePathChange(path, MegaPintAutoSaveData.SceneGuids[sceneList.selectedIndex]);
            // TODO visuals
        };

        btnSave.clicked += () =>
        {
            MegaPintAutoSaveData.SetSceneOverwrites(
                MegaPintAutoSaveData.SceneGuids[sceneList.selectedIndex],
                new MegaPintAutoSaveData.Overwrites
                {
                    interval = interval.value,
                    saveMode = saveMode.index,
                    warning = warning.value,
                    duplicatePath = path.tooltip
                });

            btnSave.style.display = DisplayStyle.None;
        };
        
        sceneList.selectionChanged += _ =>
        {
            if (sceneList.selectedItem == null)
                return;
            
            var index = sceneList.selectedIndex;
            var guid = MegaPintAutoSaveData.SceneGuids[index];

            var sceneAsset = AssetDatabase.LoadAssetAtPath <SceneAsset>(AssetDatabase.GUIDToAssetPath(guid));

            sceneName.text = sceneAsset.name;

            MegaPintAutoSaveData.GetSceneSettings(
                guid,
                out var intervalValue,
                out var saveModeValue,
                out var duplicatePathValue,
                out var warningValue);

            MegaPintAutoSaveData.GetSceneOverwrites(
                guid, 
                "", 
                out var intervalOv, 
                out var saveModeOv, 
                out var duplicatePathOv, 
                out var warningOv);

            interval.value = intervalValue;
            intervalOverwrite.style.display = intervalOv ? DisplayStyle.Flex : DisplayStyle.None;
            
            saveMode.index = saveModeValue;
            saveModeOverwrite.style.display = saveModeOv ? DisplayStyle.Flex : DisplayStyle.None;
            
            path.text = duplicatePathValue;
            duplicatePathOverwrite.style.display = duplicatePathOv ? DisplayStyle.Flex : DisplayStyle.None;
            
            warning.value = warningValue;
            warningOverwrite.style.display = warningOv ? DisplayStyle.Flex : DisplayStyle.None;
            
            duplicatePath.style.display = saveModeValue == 1 ? DisplayStyle.Flex : DisplayStyle.None;
            
            sceneSettings.style.display = DisplayStyle.Flex;
        };
    }

    private static void AutoSaveWaitForObjectPicker(VisualElement rightPane)
    {
        if (Event.current.commandName != "ObjectSelectorClosed")
            return;

        onRightPaneGUI -= AutoSaveWaitForObjectPicker;

        var scene = (SceneAsset)EditorGUIUtility.GetObjectPickerObject();

        GUID guid = AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(scene));
        
        MegaPintAutoSaveData.SetSceneOverwrites(guid.ToString(), null);
        
        var sceneList = rightPane.Q <ListView>("Scenes");
        
        sceneList.itemsSource = MegaPintAutoSaveData.SceneGuids;
        sceneList.RefreshItems();
    }

    private static void UnsubscribeAutoSave()
    {
        s_onSelectedTabChanged -= OnTabChangedAutoSave;
        s_onSelectedPackageChanged -= UnsubscribeAutoSave;
    }

    #endregion
}

}
#endif

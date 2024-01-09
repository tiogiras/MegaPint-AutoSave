#if UNITY_EDITOR
using System;
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

    private static void AutoSaveSceneSelected(
        int index,
        Label sceneName,
        IntegerField interval,
        VisualElement intervalOverwrite,
        DropdownField saveMode,
        VisualElement saveModeOverwrite,
        Label path,
        VisualElement duplicatePathOverwrite,
        GroupBox duplicatePath,
        Toggle warning,
        VisualElement warningOverwrite)
    {
        var guid = MegaPintAutoSaveData.SceneGuids[index];

        var sceneAsset = AssetDatabase.LoadAssetAtPath <SceneAsset>(AssetDatabase.GUIDToAssetPath(guid));
        
        if (sceneAsset == null)
            return;

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

        Debug.Log($"Overwrites: {intervalOv}, {saveModeOv}, {duplicatePathOv}, {warningOv}");
        
        interval.value = intervalValue;
        intervalOverwrite.style.display = intervalOv ? DisplayStyle.Flex : DisplayStyle.None;

        Debug.Log(intervalOverwrite.style.display);

        saveMode.index = saveModeValue;
        saveModeOverwrite.style.display = saveModeOv ? DisplayStyle.Flex : DisplayStyle.None;

        path.text = duplicatePathValue;
        duplicatePathOverwrite.style.display = duplicatePathOv ? DisplayStyle.Flex : DisplayStyle.None;

        warning.value = warningValue;
        warningOverwrite.style.display = warningOv ? DisplayStyle.Flex : DisplayStyle.None;

        duplicatePath.style.display = saveModeValue == 1 ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private static void AutoSaveTab1(VisualElement root)
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

        btnChange.clicked += () => {AutoSavePathChange(path);};

        btnSave.clicked += () =>
        {
            MegaPintAutoSaveData.IntervalValue = interval.value;
            MegaPintAutoSaveData.SaveModeValue = saveMode.index;
            MegaPintAutoSaveData.WarningValue = warning.value;
            MegaPintAutoSaveData.DuplicatePathValue = path.tooltip;

            btnSave.style.display = DisplayStyle.None;
        };

        #endregion
    }

    private static void AutoSaveTab2(VisualElement root)
    {
        #region Collect References

        var sceneList = root.Q <ListView>("Scenes");

        var sceneSettings = root.Q <GroupBox>("SceneSettings");

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

        #endregion

        var labelTemplate = Resources.Load <VisualTreeAsset>(AutoSaveSceneEntryPath);

        #region Set initial Visuals

        sceneSettings.style.display = DisplayStyle.None;
        btnSave.style.display = DisplayStyle.None;

        duplicatePath.style.display = saveMode.index == 1 ? DisplayStyle.Flex : DisplayStyle.None;

        #endregion

        #region Register Callbacks

        #region Left Pane

        sceneList.makeItem = () => new VisualElement();

        sceneList.bindItem = (element, i) =>
        {
            element.Clear();

            var guid = MegaPintAutoSaveData.SceneGuids[i];

            var pathFromGuid = AssetDatabase.GUIDToAssetPath(guid);
            var sceneAsset = AssetDatabase.LoadAssetAtPath <SceneAsset>(pathFromGuid);
            
            if (sceneAsset == null)
                return;
            
            var selectedSceneName = sceneAsset.name;

            TemplateContainer entry = labelTemplate.Instantiate();
            var label = entry.Q <Label>();
            label.text = selectedSceneName;
            label.tooltip = selectedSceneName;

            entry.Q <Button>().clickable = new Clickable(
                () =>
                {
                    if (!EditorUtility.DisplayDialog(
                            "Remove Scene",
                            $"Do you want to remove all settings corresponding to the following scene? \n {selectedSceneName}",
                            "Yes",
                            "No"))
                        return;

                    MegaPintAutoSaveData.RemoveSceneOverwrites(guid);

                    sceneList.itemsSource = MegaPintAutoSaveData.SceneGuids;
                    sceneList.RefreshItems();
                });

            element.Add(entry);
        };

        sceneList.unbindItem = (element, _) => {element.Q <Button>().clickable = null;};

        root.Q <Button>("BTN_AddScene").clicked += () =>
        {
            var controlID = GUIUtility.GetControlID(FocusType.Passive);
            EditorGUIUtility.ShowObjectPicker <SceneAsset>(null, false, "", controlID);

            onRightPaneGUI += AutoSaveWaitForObjectPicker;
        };

        sceneList.selectionChanged += _ =>
        {
            if (sceneList.selectedItem == null)
                return;

            AutoSaveSceneSelected(
                sceneList.selectedIndex,
                sceneName,
                interval,
                intervalOverwrite,
                saveMode,
                saveModeOverwrite,
                path,
                duplicatePathOverwrite,
                duplicatePath,
                warning,
                warningOverwrite);

            sceneSettings.style.display = DisplayStyle.Flex;
        };

        #endregion

        #region Right Pane

        interval.RegisterValueChangedCallback(
            evt =>
            {
                if (evt.newValue != MegaPintAutoSaveData.GetSceneInterval(""/*TODO guid leer mit guid von preset ersetzen*/))
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
                if (evt.newValue != MegaPintAutoSaveData.GetSceneWarning(""/*TODO guid leer mit guid von preset ersetzen*/))
                {
                    btnSave.style.display = DisplayStyle.Flex;
                    warningOverwrite.style.display = DisplayStyle.Flex;
                }
                else
                    warningOverwrite.style.display = DisplayStyle.None;
            });

        saveMode.RegisterValueChangedCallback(
            _ =>
            {
                duplicatePath.style.display = saveMode.index == 1 ? DisplayStyle.Flex : DisplayStyle.None;

                if (saveMode.index != MegaPintAutoSaveData.GetSceneSaveMode(""/*TODO guid leer mit guid von preset ersetzen*/))
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
                    interval = interval.value, saveMode = saveMode.index, warning = warning.value, duplicatePath = path.tooltip
                });

            btnSave.style.display = DisplayStyle.None;
        };

        #endregion

        #endregion

        sceneList.itemsSource = MegaPintAutoSaveData.SceneGuids;
        sceneList.RefreshItems();
        sceneList.ClearSelection();
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

    private static void UnsubscribeAutoSave()
    {
        s_onSelectedTabChanged -= OnTabChangedAutoSave;
        s_onSelectedPackageChanged -= UnsubscribeAutoSave;
    }

    #endregion
}

}
#endif

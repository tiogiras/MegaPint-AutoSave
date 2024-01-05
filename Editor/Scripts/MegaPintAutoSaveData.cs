#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Editor.Scripts.Settings;

namespace Editor.Scripts
{

public static class MegaPintAutoSaveData
{
    private struct SettingsValue <T>
    {
        public string key;
        public T defaultValue;
    }

    private const string SettingsName = "MegaPint.AutoSave";

    // TODO

    // sceneSettings                        => struct dict
    //      duplicatePath with scene as key    => string dict
    //      warning with scene as key           => bool dict
    //      interval with scene as key          => float dict
    //      saveMode with scene as key          => int dict

    // sceneOverwrites                      => struct dict
    //      hasOverwrite with scene as key      => bool dict

    // Stores all scene guids that have overwrites like own settings or presets and so on
    // Overwrite value is stored in the bool dictionary if a scene has custom settings to be read can be found with hasKey(scene guid)
    // All custom settings can then be read with the scene guid as key from the sceneSettings struct dict

    // duplicatePath_default                => string dict
    // warning_default                      => bool dict
    // interval_default                     => int dict
    // saveMode_default                     => int dict

    private static readonly SettingsValue <string> s_duplicatePath = new() {key = "duplicatePath", defaultValue = "Assets"};

    private static readonly SettingsValue <bool> s_warning = new() {key = "warning", defaultValue = true};

    private static readonly SettingsValue <int> s_interval = new() {key = "interval", defaultValue = 30};

    private static readonly SettingsValue <int> s_saveMode = new() {key = "saveMode", defaultValue = 0};


    public class Overwrites
    {
        public string duplicatePath;
        public int interval = -1;
        public int saveMode = -1;

        public bool hasWarningOverwrite;
        public bool warning;
    }
    
    public static void SetSceneOverwrites(string guid, Overwrites overwrites, bool removeNullOverwrites = true)
    {
        _Settings.SetStructValue(s_sceneGuids.key, guid, "");
        
        if (overwrites == null)
            return;
        
        MegaPintSettingsStruct structValue = _Settings.GetStruct(s_sceneOverwrites.key, s_sceneOverwrites.defaultValue);
        
        if (!string.IsNullOrEmpty(overwrites.duplicatePath))
            structValue.SetValue(guid, overwrites.duplicatePath);
        else if (removeNullOverwrites)
            structValue.RemoveStringEntry(guid);

        if (overwrites.interval > 0)
            structValue.SetValue(guid, (float)overwrites.interval);
        else if (removeNullOverwrites)
            structValue.RemoveFloatEntry(guid);
        
        if (overwrites.saveMode > 0)
            structValue.SetValue(guid, overwrites.saveMode);
        else if (removeNullOverwrites)
            structValue.RemoveIntEntry(guid);
        
        if (overwrites.hasWarningOverwrite)
            structValue.SetValue(guid, overwrites.warning);
        else if (removeNullOverwrites)
            structValue.RemoveBoolEntry(guid);
    }

    public static void RemoveSceneOverwrites(string guid)
    {
        _Settings.GetStruct(s_sceneGuids.key, s_sceneGuids.defaultValue).RemoveStringEntry(guid);
        
        MegaPintSettingsStruct structValue = _Settings.GetStruct(s_sceneOverwrites.key, s_sceneOverwrites.defaultValue);
        structValue.RemoveStringEntry(guid);
        structValue.RemoveFloatEntry(guid);
        structValue.RemoveIntEntry(guid);
        structValue.RemoveBoolEntry(guid);
    }

    public static int GetSceneInterval(string guid)
    {
        MegaPintSettingsStruct structValue = _Settings.GetStruct(s_sceneOverwrites.key, s_sceneOverwrites.defaultValue);
        
        return structValue.floatValues.HasKey(guid)
            ? (int)structValue.GetValue(guid, 0f)
            : IntervalValue;
    }
    
    public static int GetSceneSaveMode(string guid)
    {
        MegaPintSettingsStruct structValue = _Settings.GetStruct(s_sceneOverwrites.key, s_sceneOverwrites.defaultValue);
        
        return structValue.intValues.HasKey(guid)
            ? structValue.GetValue(guid, 0)
            : SaveModeValue;
    }
    
    public static string GetSceneDuplicatePath(string guid)
    {
        MegaPintSettingsStruct structValue = _Settings.GetStruct(s_sceneOverwrites.key, s_sceneOverwrites.defaultValue);
        
        return structValue.stringValues.HasKey(guid)
            ? structValue.GetValue(guid, "")
            : DuplicatePathValue;
    }
    
    public static bool GetSceneWarning(string guid)
    {
        MegaPintSettingsStruct structValue = _Settings.GetStruct(s_sceneOverwrites.key, s_sceneOverwrites.defaultValue);
        
        return structValue.boolValues.HasKey(guid)
            ? structValue.GetValue(guid, true)
            : WarningValue;
    }

    public static void GetSceneSettings(string guid, out int interval, out int saveMode, out string duplicatePath, out bool warning)
    {
        MegaPintSettingsStruct structValue = _Settings.GetStruct(s_sceneOverwrites.key, s_sceneOverwrites.defaultValue);

        interval = structValue.floatValues.HasKey(guid) ? (int)structValue.GetValue(guid, 0f) : IntervalValue;

        saveMode = structValue.intValues.HasKey(guid) ? structValue.GetValue(guid, 0) : SaveModeValue;

        duplicatePath = structValue.stringValues.HasKey(guid) ? structValue.GetValue(guid, "") : DuplicatePathValue;

        warning = structValue.boolValues.HasKey(guid) ? structValue.GetValue(guid, true) : WarningValue;
    }

    public static void GetSceneOverwrites(
        string guid,
        string parentGuid,
        out bool intervalOverwrite,
        out bool saveModeOverwrite,
        out bool duplicatePathOverwrite,
        out bool warningOverwrite)
    {
        GetSceneSettings(guid, out var interval, out var saveMode, out var duplicatePath, out var warning);
        GetSceneSettings(parentGuid, out var parentInterval, out var parentSaveMode, out var parentDuplicatePath, out var parentWarning);

        intervalOverwrite = interval != parentInterval;
        saveModeOverwrite = saveMode != parentSaveMode;
        duplicatePathOverwrite = !duplicatePath.Equals(parentDuplicatePath);
        warningOverwrite = warning != parentWarning;
    }

    public static List <string> SceneGuids => _Settings.GetStruct(s_sceneGuids.key, s_sceneGuids.defaultValue).stringValues.Keys;

    private static SettingsValue <MegaPintSettingsStruct> s_sceneOverwrites = new()
    {
        key = "sceneOverwrites", defaultValue = new MegaPintSettingsStruct()
    };
    
    private static SettingsValue <MegaPintSettingsStruct> s_sceneGuids = new()
    {
        key = "sceneGuids", defaultValue = new MegaPintSettingsStruct()
    };

    public static int IntervalValue
    {
        get => _Settings.GetValue(s_interval.key, s_interval.defaultValue);
        set => _Settings.SetValue(s_interval.key, value);
    }

    public static int SaveModeValue
    {
        get => _Settings.GetValue(s_saveMode.key, s_saveMode.defaultValue);
        set => _Settings.SetValue(s_saveMode.key, value);
    }

    public static string DuplicatePathValue
    {
        get => _Settings.GetValue(s_duplicatePath.key, s_duplicatePath.defaultValue);
        set => _Settings.SetValue(s_duplicatePath.key, value);
    }

    public static bool WarningValue
    {
        get => _Settings.GetValue(s_warning.key, s_warning.defaultValue);
        set => _Settings.SetValue(s_warning.key, value);
    }

    private static MegaPintSettingsBase _Settings => MegaPintSettings.instance.GetSetting(SettingsName);
}

}
#endif

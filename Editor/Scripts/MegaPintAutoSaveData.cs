#if UNITY_EDITOR
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

    private static SettingsValue <MegaPintSettingsStruct> s_sceneSettings = new()
    {
        key = "sceneSettings", defaultValue = new MegaPintSettingsStruct()
    };

    private static SettingsValue <MegaPintSettingsStruct> s_sceneOverwrites = new()
    {
        key = "sceneOverwrites", defaultValue = new MegaPintSettingsStruct()
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

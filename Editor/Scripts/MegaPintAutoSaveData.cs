#if UNITY_EDITOR
using Editor.Scripts.Settings;
using Unity.Plastic.Newtonsoft.Json.Serialization;

namespace Editor.Scripts
{

public static class MegaPintAutoSaveData
{
    public static Action onSettingsChanged;

    private struct SettingsValue <T>
    {
        public string key;
        public T defaultValue;
    }

    private const string SettingsName = "MegaPint.AutoSave";

    private static readonly SettingsValue <string> s_duplicatePath = new() {key = "duplicatePath", defaultValue = "Assets"};

    private static readonly SettingsValue <bool> s_warning = new() {key = "warning", defaultValue = true};

    private static readonly SettingsValue <int> s_interval = new() {key = "interval", defaultValue = 30};

    private static readonly SettingsValue <int> s_saveMode = new() {key = "saveMode", defaultValue = 0};

    public static int IntervalValue
    {
        get => _Settings.GetValue(s_interval.key, s_interval.defaultValue);
        set => _Settings.SetValue(s_interval.key, value, true);
    }

    public static int SaveModeValue
    {
        get => _Settings.GetValue(s_saveMode.key, s_saveMode.defaultValue);
        set => _Settings.SetValue(s_saveMode.key, value, true);
    }

    public static string DuplicatePathValue
    {
        get => _Settings.GetValue(s_duplicatePath.key, s_duplicatePath.defaultValue);
        set => _Settings.SetValue(s_duplicatePath.key, value, true);
    }

    public static bool WarningValue
    {
        get => _Settings.GetValue(s_warning.key, s_warning.defaultValue);
        set => _Settings.SetValue(s_warning.key, value, true);
    }

    private static MegaPintSettingsBase _Settings => MegaPintSettings.instance.GetSetting(SettingsName);
}

}
#endif

#if UNITY_EDITOR
namespace com.tiogiras.megapint_autosave.Editor.Scripts
{
    public static class MegaPintAutoSaveData
    {
        public struct SettingsValue<T>
        {
            public string Key;
            public T DefaultValue;
        }

        public static string SettingsName => "MegaPint.AutoSave";
        
        public static SettingsValue<int> Interval = new()
        {
            Key = "interval",
            DefaultValue = 30
        };
        
        public static SettingsValue<int> SaveMode = new()
        {
            Key = "saveMode",
            DefaultValue = 0
        };
        
        public static SettingsValue<bool> Warning = new()
        {
            Key = "warning",
            DefaultValue = true
        };
        
        public static SettingsValue<string> DuplicatePath = new()
        {
            Key = "duplicatePath",
            DefaultValue = "Assets/MegaPint/SceneBackUps"
        };
    }
}
#endif
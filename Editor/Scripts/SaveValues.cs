#if UNITY_EDITOR
using System;
using MegaPint.Editor.Scripts.Settings;

namespace MegaPint.Editor.Scripts
{

/// <summary> Partial class storing saveData values (AutoSave) </summary>
internal static partial class SaveValues
{
    public static class AutoSave
    {
        public static Action onSettingsChanged;

        private static CacheValue <string> s_duplicatePath = new() {defaultValue = "Assets"};
        private static CacheValue <bool> s_warning = new() {defaultValue = true};
        private static CacheValue <int> s_interval = new() {defaultValue = 30};
        private static CacheValue <int> s_saveMode = new() {defaultValue = 0};

        private static SettingsBase s_settings;

        public static string DuplicatePath
        {
            get => ValueProperty.Get("duplicatePath", ref s_duplicatePath, _Settings);
            set => ValueProperty.Set("duplicatePath", value, ref s_duplicatePath, _Settings);
        }

        public static bool Warning
        {
            get => ValueProperty.Get("warning", ref s_warning, _Settings);
            set => ValueProperty.Set("warning", value, ref s_warning, _Settings);
        }

        public static int Interval
        {
            get => ValueProperty.Get("interval", ref s_interval, _Settings);
            set => ValueProperty.Set("interval", value, ref s_interval, _Settings);
        }

        public static int SaveMode
        {
            get => ValueProperty.Get("saveMode", ref s_saveMode, _Settings);
            set => ValueProperty.Set("saveMode", value, ref s_saveMode, _Settings);
        }

        private static SettingsBase _Settings
        {
            get
            {
                if (MegaPintSettings.Exists())
                    return s_generalSettings ??= MegaPintSettings.instance.GetSetting("AutoSave");

                return null;
            }
        }
    }
}

}
#endif

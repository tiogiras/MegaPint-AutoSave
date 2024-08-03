#if UNITY_EDITOR
using System;
using MegaPint.Editor.Scripts.Settings;
using UnityEditor;
using UnityEngine;

namespace MegaPint.Editor.Scripts
{

/// <summary> Partial class storing saveData values (AutoSave) </summary>
internal static partial class SaveValues
{
    public static class AutoSave
    {
        public static Action onSettingsChanged;
        public static Action <bool> onIsActiveChanged;
        public static Action <bool> onDisplayToolbarToggleChanged;

        public static Action <string> onDuplicatePathChanged;
        public static Action <bool> onWarningChanged;
        public static Action <int> onIntervalChanged;
        public static Action <int> onSaveModeChanged;

        private static CacheValue <string> s_duplicatePath = new() {defaultValue = "Assets"};
        private static CacheValue <bool> s_warning = new() {defaultValue = true};
        private static CacheValue <int> s_interval = new() {defaultValue = 30};
        private static CacheValue <int> s_saveMode = new() {defaultValue = 0};
        private static CacheValue <bool> s_displayToolbarToggle = new() {defaultValue = true};

        private static CacheValue <bool> s_isActive = new() {defaultValue = false};

        private static CacheValue <bool> s_applyPSAutoSaveWindow = new() {defaultValue = true};

        private static SettingsBase s_settings;

        public static string DuplicatePath
        {
            get => ValueProperty.Get("duplicatePath", ref s_duplicatePath, _Settings);
            set
            {
                var prevValue = DuplicatePath;
                ValueProperty.Set("duplicatePath", value, ref s_duplicatePath, _Settings);
                
                if (prevValue != value)
                    onDuplicatePathChanged?.Invoke(value);
            }
        }

        public static bool Warning
        {
            get => ValueProperty.Get("warning", ref s_warning, _Settings);
            set
            {
                var prevValue = Warning;
                ValueProperty.Set("warning", value, ref s_warning, _Settings);
                
                if (prevValue != value)
                    onWarningChanged?.Invoke(value);
            }
        }

        public static int Interval
        {
            get => ValueProperty.Get("interval", ref s_interval, _Settings);
            set
            {
                var prevValue = Interval;
                ValueProperty.Set("interval", value, ref s_interval, _Settings);
                
                if (prevValue != value)
                    onIntervalChanged?.Invoke(value);
            }
        }

        public static int SaveMode
        {
            get => ValueProperty.Get("saveMode", ref s_saveMode, _Settings);
            set
            {
                var prevValue = SaveMode;
                ValueProperty.Set("saveMode", value, ref s_saveMode, _Settings);
                
                if (prevValue != value)
                    onSaveModeChanged?.Invoke(value);
            }
        }

        public static bool ApplyPSAutoSaveWindow
        {
            get => ValueProperty.Get("ApplyPS_AutoSaveWindow", ref s_applyPSAutoSaveWindow, _Settings);
            set => ValueProperty.Set("ApplyPS_AutoSaveWindow", value, ref s_applyPSAutoSaveWindow, _Settings);
        }

        public static bool DisplayToolbarToggle
        {
            get => ValueProperty.Get("DisplayToolbarToggle", ref s_displayToolbarToggle, _Settings);
            set
            {
                ValueProperty.Set("DisplayToolbarToggle", value, ref s_displayToolbarToggle, _Settings);
                onDisplayToolbarToggleChanged?.Invoke(value);
            }
        }

        public static bool IsActive
        {
            get => ValueProperty.Get("IsActive", ref s_isActive, _Settings);
            set
            {
                ValueProperty.Set("IsActive", value, ref s_isActive, _Settings);
                onIsActiveChanged?.Invoke(value);

                if (!Warning || value)
                    return;

                EditorApplication.Beep();
                Debug.LogWarning("AutoSave feature was stopped!");
            }
        }

        private static SettingsBase _Settings
        {
            get
            {
                if (MegaPintMainSettings.Exists())
                    return s_settings ??= MegaPintMainSettings.instance.GetSetting("AutoSave");

                return null;
            }
        }
    }
}

}
#endif

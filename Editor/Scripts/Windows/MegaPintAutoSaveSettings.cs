#if UNITY_EDITOR
using System;
using Editor.Scripts.Settings;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Scripts.Windows
{
    public class MegaPintAutoSaveSettings : MegaPintEditorWindowBase
    {
        /// <summary> Loaded reference of the uxml </summary>
        private VisualTreeAsset _baseWindow;

        #region References
        
        /// <summary> Reference to the int field displaying the current interval </summary>
        private IntegerField _interval;
        
        /// <summary> Reference to the dropdown displaying the current saveMode </summary>
        private DropdownField _saveMode;
        
        /// <summary> Reference to the toggle displaying the warning settings </summary>
        private Toggle _warning;

        /// <summary> Reference to the GroupBox containing the path settings </summary>
        private GroupBox _duplicatePath;
        
        /// <summary> Reference to the label displaying the duplicatePath </summary>
        private Label _path;
        
        /// <summary> Reference to the change button </summary>
        private Button _change;

        #endregion

        /// <summary> Current value of the interval from the settings </summary>
        private int _intervalValue;
        
        /// <summary> Current value of the saveMode from the settings </summary>
        private int _saveModeValue;
        
        /// <summary> Current value of the warning from the settings </summary>
        private bool _warningValue;
        
        /// <summary> Current value of the duplicatePath from the settings </summary>
        private string _pathValue;

        #region Overrides

        protected override string BasePath() => "MegaPintAutoSaveSettings";
        public override MegaPintEditorWindowBase ShowWindow()
        {
            titleContent.text = "AutoSave - Settings";
            return this;
        }

        protected override void CreateGUI()
        {
            base.CreateGUI();

            var root = rootVisualElement;

            VisualElement content = _baseWindow.Instantiate();

            _interval = content.Q<IntegerField>("Interval");
            _interval.value = _intervalValue;
            
            _duplicatePath = content.Q<GroupBox>("DuplicatePath");
            _path = content.Q<Label>("Path");
            
            _saveMode = content.Q<DropdownField>("SaveMode");
            _saveMode.RegisterValueChangedCallback(UpdateDuplicatePathGUI);
            _saveMode.index = _saveModeValue;
            
            UpdateDuplicatePathGUI(null);

            _change = content.Q<Button>("BTN_Change");
            _change.clicked += OnPathChange;
            
            _warning = content.Q<Toggle>("Warning");
            _warning.value = _warningValue;

            root.Add(content);
        }

        protected override void OnDestroy()
        {
            var settings = MegaPintSettings.Get();
            var autoSaveSettings = settings.GetSetting(MegaPintAutoSaveData.SettingsName);
            
            autoSaveSettings.SetValue(MegaPintAutoSaveData.Interval.Key, _interval.value);
            autoSaveSettings.SetValue(MegaPintAutoSaveData.SaveMode.Key, _saveMode.index);
            autoSaveSettings.SetValue(MegaPintAutoSaveData.Warning.Key, _warning.value);
            autoSaveSettings.SetValue(MegaPintAutoSaveData.DuplicatePath.Key, _path.text);
            
            _saveMode.UnregisterValueChangedCallback(UpdateDuplicatePathGUI);
            _change.clicked -= OnPathChange;
            
            base.OnDestroy();
        }

        protected override bool LoadResources()
        {
            _baseWindow = Resources.Load<VisualTreeAsset>(BasePath());
            return _baseWindow != null;
        }

        protected override void LoadSettings()
        {
            var settings = MegaPintSettings.Get().GetSetting("MegaPint.AutoSave");
            _intervalValue = settings.GetValue(MegaPintAutoSaveData.Interval.Key, MegaPintAutoSaveData.Interval.DefaultValue);
            _saveModeValue = settings.GetValue(MegaPintAutoSaveData.SaveMode.Key, MegaPintAutoSaveData.SaveMode.DefaultValue);
            _warningValue = settings.GetValue(MegaPintAutoSaveData.Warning.Key, MegaPintAutoSaveData.Warning.DefaultValue);
            _pathValue = settings.GetValue(MegaPintAutoSaveData.DuplicatePath.Key, MegaPintAutoSaveData.DuplicatePath.DefaultValue);
        }

        #endregion

        private void UpdateDuplicatePathGUI(ChangeEvent<string> evt)
        {
            _duplicatePath.style.display = _saveMode.index == 1 ? DisplayStyle.Flex : DisplayStyle.None;
            
            var y = _saveMode.index == 1 ? 125 : 100;
            minSize = new Vector2(300, y);
            maxSize = new Vector2(300, y);

            var cleanPath = _pathValue[(_pathValue.IndexOf("/", StringComparison.Ordinal) + 1)..];
            if (cleanPath.Length <= 20)
                _path.text = _pathValue;
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

                _path.text = text;
            }

            _path.tooltip = _pathValue;
        }
        
        private void OnPathChange()
        {
            var path = EditorUtility.OpenFolderPanel("Set folder for duplicates", "Assets/", "");
            
            if (!path.StartsWith(Application.dataPath))
            {
                EditorUtility.DisplayDialog("Folder not valid",
                    "The specified folder must be inside the unity Assets folder", "OK");
            }
            else
            {
                path = path.Replace(Application.dataPath, "");
                _pathValue = path.Insert(0, "Assets");

                EditorUtility.DisplayDialog("New folder",
                    $"All duplicates will now be saved to:\n{_pathValue}", "OK");

                UpdateDuplicatePathGUI(null);
            }
        }
    }
}
#endif
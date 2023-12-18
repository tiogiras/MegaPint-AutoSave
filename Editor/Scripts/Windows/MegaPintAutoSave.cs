#if UNITY_EDITOR
using System;
using System.Threading.Tasks;
using Editor.Scripts.Settings;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Editor.Scripts.Windows
{
    public class MegaPintAutoSave : MegaPintEditorWindowBase
    {
        #region Visual References

        /// <summary> Reference to the progressbar displaying _currentSecond </summary>
        private ProgressBar _nextSaveProgress;
        
        /// <summary> Reference to the playMode group </summary>
        private GroupBox _playMode;
        
        /// <summary> Reference to the editMode group </summary>
        private GroupBox _editMode;
        
        /// <summary> Reference to the label displaying the time of the next save </summary>
        private Label _nextSave;
        
        /// <summary> Reference to the label displaying the time of the last save </summary>
        private Label _lastSave;
        
        /// <summary> Reference to the label displaying the current interval </summary>
        private Label _interval;

        private Button _btnSettings;

        #endregion

        #region Private

        /// <summary> Loaded reference of the uxml </summary>
        private VisualTreeAsset _baseWindow;

        /// <summary> Current progress towards the next save </summary>
        private int _currentSecond;

        /// <summary> Get the current interval from the settings </summary>
        private int _intervalValue;
        
        /// <summary> Get the current saveMode from the settings </summary>
        private int _saveModeValue;
        
        /// <summary> Get the current duplicatePath from the settings </summary>
        private string _duplicatePathValue;

        private bool _breakTimer;
        
        #endregion
        
        #region Override Methods

        protected override string BasePath() => "User Interface/MegaPintAutoSave";
        
        public override MegaPintEditorWindowBase ShowWindow()
        {
            minSize = new Vector2(300, 65);
            maxSize = new Vector2(300, 65);

            titleContent.text = "AutoSave";

            return this;
        }

        protected override void CreateGUI()
        {
            base.CreateGUI();

            var root = rootVisualElement;

            VisualElement content = _baseWindow.Instantiate();

            #region References

            _nextSave = content.Q<Label>("NextSave");
            _nextSaveProgress = content.Q<ProgressBar>("NextSaveProgress");

            _playMode = content.Q<GroupBox>("PlayMode");
            _editMode = content.Q<GroupBox>("EditMode");
            
            _lastSave = content.Q<Label>("LastSave");
            _interval = content.Q<Label>("Interval");

            _btnSettings = content.Q<Button>("Settings");
            
            #endregion
            
            RegisterCallbacks();

            UpdateStaticGUI();

            if (EditorApplication.isPlaying)
            {
                _playMode.style.display = DisplayStyle.Flex;
                _editMode.style.display = DisplayStyle.None;
            }
            else
            {
                _playMode.style.display = DisplayStyle.None;
                _editMode.style.display = DisplayStyle.Flex;
                var _ = Timer();
            }
            
            root.Add(content);
        }

        protected override bool LoadResources()
        {
            _baseWindow = Resources.Load<VisualTreeAsset>(BasePath());
            return _baseWindow != null;
        }

        protected override bool LoadSettings()
        {
            if (!base.LoadSettings())
                return false;
            
            _intervalValue = MegaPintSettings.instance.GetSetting(MegaPintAutoSaveData.SettingsName)
                .GetValue(MegaPintAutoSaveData.Interval.Key, MegaPintAutoSaveData.Interval.DefaultValue);
            
            _saveModeValue = MegaPintSettings.instance.GetSetting(MegaPintAutoSaveData.SettingsName)
                .GetValue(MegaPintAutoSaveData.SaveMode.Key, MegaPintAutoSaveData.SaveMode.DefaultValue);

            _duplicatePathValue = MegaPintSettings.instance.GetSetting(MegaPintAutoSaveData.SettingsName)
                .GetValue(MegaPintAutoSaveData.DuplicatePath.Key, MegaPintAutoSaveData.DuplicatePath.DefaultValue);

            return true;
        }

        protected override void RegisterCallbacks()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChange;
            _btnSettings.clicked += OnOpenAutoSaveSettings;
        }

        protected override void UnRegisterCallbacks()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChange;
            _btnSettings.clicked -= OnOpenAutoSaveSettings;
        }

        #endregion

        #region Callback Methods

        private void OnPlayModeChange(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredEditMode:
                    _breakTimer = false;
                    var _ = Timer();

                    _playMode.style.display = DisplayStyle.None;
                    _editMode.style.display = DisplayStyle.Flex;
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    _breakTimer = true;

                    _playMode.style.display = DisplayStyle.Flex;
                    _editMode.style.display = DisplayStyle.None;
                    break;
            }
        }
        
        private void OnOpenAutoSaveSettings()
        {
            var window = ContextMenu.TryOpen<MegaPintAutoSaveSettings>(true, "AutoSave Settings");
            if (window == null) 
                return;
            
            window.onClose += OnSettingsWindowClosed;
        }
            
        private void OnSettingsWindowClosed(MegaPintEditorWindowBase window)
        {
            LoadSettings();
            _currentSecond = 0;
            
            _nextSaveProgress.highValue = _intervalValue;
            _interval.text = $"{_intervalValue} Seconds";
            
            UpdateGUI();
            UpdateStaticGUI();
            
            window.onClose -= OnSettingsWindowClosed;
        }

        #endregion

        #region Internal Methods

        private async Task Timer()
        {
            while (!_breakTimer && this != null)
            {
                _currentSecond++;
                UpdateGUI();   
                
                await Task.Delay(1000);
            }
            
            if (this != null)
                return;
            
            if (MegaPintSettings.instance.GetSetting("MegaPint.AutoSave")
                .GetValue(MegaPintAutoSaveData.Warning.Key, MegaPintAutoSaveData.Warning.DefaultValue))
                EditorApplication.Beep();
        }

        private void UpdateGUI()
        {
            var interval = _intervalValue;
            
            if (_currentSecond >= interval)
            {
                Save();
                _currentSecond = 0;
                _lastSave.text = DateTime.Now.ToString("HH:mm:ss");
                _nextSave.text = DateTime.Now.AddSeconds(interval).ToString("HH:mm:ss");
            }

            _nextSaveProgress.highValue = _intervalValue;
            _nextSaveProgress.value = _currentSecond;
        }

        private void UpdateStaticGUI()
        {
            _nextSave.text = DateTime.Now.AddSeconds(_intervalValue).ToString("HH:mm:ss");
            _nextSaveProgress.highValue = _intervalValue;
            _interval.text = $"{_intervalValue} Seconds";
        }

        private void Save()
        {
            var scene = SceneManager.GetActiveScene();
            var destination = _saveModeValue == 0 ? 
                scene.path : 
                $"{_duplicatePathValue}/{scene.name} ({DateTime.Today:MM.dd.yy})({DateTime.Now:HH.mm.ss}).unity";
            
            EditorSceneManager.SaveScene(scene, destination, _saveModeValue == 1);
        }
        
        #endregion
    }
}
#endif
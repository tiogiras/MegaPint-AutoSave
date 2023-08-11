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
        /// <summary> Loaded reference of the uxml </summary>
        private VisualTreeAsset _baseWindow;

        #region References

        /// <summary> Reference to the progressbar displaying _currentSecond </summary>
        private ProgressBar _nextSaveProgress;
        
        /// <summary> Reference to the label displaying the time of the next save </summary>
        private Label _nextSave;
        
        /// <summary> Reference to the label displaying the time of the last save </summary>
        private Label _lastSave;
        
        /// <summary> Reference to the label displaying the current interval </summary>
        private Label _interval;

        #endregion

        /// <summary> Current progress towards the next save </summary>
        private int _currentSecond;

        /// <summary> Get the current interval from the settings </summary>
        private int _intervalValue;
        
        /// <summary> Get the current saveMode from the settings </summary>
        private int _saveModeValue;
        
        /// <summary> Get the current duplicatePath from the settings </summary>
        private string _duplicatePathValue;

        #region Overrides

        protected override string BasePath() => "MegaPintAutoSave";
        
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

            _nextSave = content.Q<Label>("NextSave");
            _nextSaveProgress = content.Q<ProgressBar>("NextSaveProgress");

            _lastSave = content.Q<Label>("LastSave");
            _interval = content.Q<Label>("Interval");
            _lastSave.text = "";
            UpdateStaticGUI();

            content.Q<Button>("Settings").clicked += OpenAutoSaveSettings;
            root.Add(content);

            Timer();
        }

        protected override bool LoadResources()
        {
            _baseWindow = Resources.Load<VisualTreeAsset>(BasePath());
            return _baseWindow != null;
        }

        protected override void LoadSettings()
        {
            _intervalValue = MegaPintSettings.Get().GetSetting(MegaPintAutoSaveData.SettingsName)
                .GetValue(MegaPintAutoSaveData.Interval.Key, MegaPintAutoSaveData.Interval.DefaultValue);
            
            _saveModeValue = MegaPintSettings.Get().GetSetting(MegaPintAutoSaveData.SettingsName)
                .GetValue(MegaPintAutoSaveData.SaveMode.Key, MegaPintAutoSaveData.SaveMode.DefaultValue);

            _duplicatePathValue = MegaPintSettings.Get().GetSetting(MegaPintAutoSaveData.SettingsName)
                .GetValue(MegaPintAutoSaveData.DuplicatePath.Key, MegaPintAutoSaveData.DuplicatePath.DefaultValue);
        }

        #endregion

        private void OpenAutoSaveSettings()
        {
            var window = GetWindow<MegaPintAutoSaveSettings>(true).ShowWindow();
            if (window == null) 
                return;
            
            window.OnClose += SettingsWindowClosed;
        }

        private void SettingsWindowClosed(MegaPintEditorWindowBase window)
        {
            LoadSettings();
            _currentSecond = 0;
            
            _nextSaveProgress.highValue = _intervalValue;
            _interval.text = $"{_intervalValue} Seconds";
            
            UpdateGUI();
            UpdateStaticGUI();
            
            window.OnClose -= SettingsWindowClosed;
        }
        
        private async void Timer()
        {
            while (true)
            {
                if (this == null)
                {
                    if (MegaPintSettings.Get().GetSetting("MegaPint.AutoSave")
                        .GetValue(MegaPintAutoSaveData.Warning.Key, MegaPintAutoSaveData.Warning.DefaultValue))
                        EditorApplication.Beep();
                    
                    return;
                }

                _currentSecond++;
                UpdateGUI();

                await Task.Delay(1000);
            }
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
    }
}
#endif
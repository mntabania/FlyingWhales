using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Assertions;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Settings {
    public class SettingsManager : MonoBehaviour {

        public static SettingsManager Instance;
        private string _settingFileLocation;
        
        public GameObject settingsGO;
        
        [Header("Graphics Settings UI")]
        public TMP_Dropdown resolutionsDropdown;
        public TMP_Dropdown graphicsDropdown;
        public Toggle fullscreenToggle;

        [Header("Gameplay Settings UI")] 
        [SerializeField] private Toggle skipTutorialsToggle;
        [SerializeField] private Toggle edgePanningToggle;
        
        [Header("Audio Settings UI")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        
        private List<Resolution> resolutions;
        private Settings _settings;

        #region getters
        public Settings settings => _settings;
        #endregion

        #region Monobehaviours
        private void Awake() {
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(gameObject);
#if UNITY_EDITOR
                EditorApplication.quitting += OnEditorQuit;
#endif
            } else {
                Destroy(gameObject);
            }
            _settingFileLocation = $"{Application.persistentDataPath}/Settings.ini";
            Initialize();
        }
        private void OnApplicationQuit() {
            SaveSettingsFile();
        }
        private void OnEditorQuit() {
            SaveSettingsFile();
        }
        #endregion

        #region Initialization
        private void Initialize() {
            LoadSettings();
            ConstructGraphicsQuality();
            ConstructResolutions();
        }
        #endregion

        #region UI
        private void ConstructResolutions() {
            resolutionsDropdown.ClearOptions();
            resolutions = new List<Resolution>();
            for (int i = 0; i < Screen.resolutions.Length; i++) {
                if (!HasResolution(Screen.resolutions[i])) {
                    resolutions.Add(Screen.resolutions[i]);
                }
            }
            List<string> options = new List<string>();
            for (int i = 0; i < resolutions.Count; i++) {
                options.Add($"{resolutions[i].width}x{resolutions[i].height}");
            }
            resolutionsDropdown.AddOptions(options);
        }
        private bool HasResolution(Resolution resolution) {
            for (int i = 0; i < resolutions.Count; i++) {
                if (resolutions[i].width == resolution.width && resolutions[i].height == resolution.height) {
                    return true;
                }
            }
            return false;
        }
        private void ConstructGraphicsQuality() {
            graphicsDropdown.ClearOptions();
            graphicsDropdown.AddOptions(QualitySettings.names.ToList());
        }
        public void OpenSettings() {
            UpdateUI();
            settingsGO.SetActive(true);
        }
        public void CloseSettings() {
            settingsGO.SetActive(false);
        }
        private void UpdateUI() {
            skipTutorialsToggle.SetIsOnWithoutNotify(settings.skipTutorials);
            edgePanningToggle.isOn = settings.useEdgePanning;

            resolutionsDropdown.value =
                UtilityScripts.GameUtilities.GetOptionIndex(resolutionsDropdown, settings.resolution);
            graphicsDropdown.value = settings.graphicsQuality;
            fullscreenToggle.isOn = settings.fullscreen;

            masterVolumeSlider.value = settings.masterVolume;
            musicVolumeSlider.value = settings.musicVolume;
        }
        public void OnToggleEdgePanning(bool isOn) {
            _settings.useEdgePanning = isOn;
            Messenger.Broadcast(Signals.EDGE_PANNING_TOGGLED, isOn);
        }
        public void OnToggleSkipTutorials(bool isOn) {
            _settings.skipTutorials = isOn;
            Messenger.Broadcast(Signals.ON_SKIP_TUTORIALS_CHANGED, isOn, true);
        }
        public void ManualToggleSkipTutorials(bool isOn, bool deSpawnExisting) {
            _settings.skipTutorials = isOn;
            Messenger.Broadcast(Signals.ON_SKIP_TUTORIALS_CHANGED, isOn, deSpawnExisting);
        }
        #endregion
        
        #region Loading
        private void LoadSettings() {
            if (UtilityScripts.Utilities.DoesFileExist(_settingFileLocation)) {
                StreamReader sr = new StreamReader(_settingFileLocation);
                string fileContents = sr.ReadToEnd();
                sr.Close();
                _settings = JsonUtility.FromJson<Settings>(fileContents);
            } else {
                 _settings = new Settings {
                     fullscreen = true,
                     graphicsQuality = 2,
                     resolution = $"{Screen.currentResolution.width.ToString()}x{Screen.currentResolution.height.ToString()}",
                     skipTutorials = false,
                     useEdgePanning = false,
                     musicVolume = 0f,
                     masterVolume = 0f,
                 };
                 Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, settings.fullscreen);
                 QualitySettings.SetQualityLevel(settings.graphicsQuality);
            }
        }
        #endregion

        #region Applying
        public void ApplySettings() {
            SetGraphicsQuality();
            SetResolution();
            SetFullscreen();

            _settings.skipTutorials = skipTutorialsToggle.isOn;
            _settings.useEdgePanning = edgePanningToggle.isOn;
            
            //resolution
            Screen.fullScreen = settings.fullscreen;
            string[] dimensionsStr = settings.resolution.Split('x');
            Assert.IsTrue(dimensionsStr.Length == 2, $"Resolution in graphics settings is invalid {settings.resolution}");
            int width = Int32.Parse(dimensionsStr[0]);
            int height = Int32.Parse(dimensionsStr[1]);
            Screen.SetResolution(width, height, settings.fullscreen);
            
            //quality
            QualitySettings.SetQualityLevel(settings.graphicsQuality);
            
            //save file
            SaveSettingsFile();
        }
        private void SetGraphicsQuality() {
            _settings.graphicsQuality = graphicsDropdown.value;
        }
        private void SetResolution() {
            _settings.resolution = resolutionsDropdown.options[resolutionsDropdown.value].text;
        }
        private void SetFullscreen() {
            _settings.fullscreen = fullscreenToggle.isOn;
        }
        private void SaveSettingsFile() {
            string jsonString = JsonUtility.ToJson(settings, true);
            System.IO.StreamWriter writer = new System.IO.StreamWriter(_settingFileLocation, false);
            writer.WriteLine(jsonString);
            writer.Close();
        }
        #endregion

        #region Audio
        public void OnMusicVolumeChanged(float volume) {
            _settings.musicVolume = volume;
            Messenger.Broadcast(Signals.MUSIC_VOLUME_CHANGED, volume);
        }
        public void OnMasterVolumeChanged(float volume) {
            _settings.masterVolume = volume;
            Messenger.Broadcast(Signals.MASTER_VOLUME_CHANGED, volume);
        }
        #endregion
    }
}
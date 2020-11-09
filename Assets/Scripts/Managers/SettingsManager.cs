using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
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
        public int targetFrameRate;
        
        [Header("Gameplay Settings UI")] 
        [SerializeField] private Toggle edgePanningToggle;
        [SerializeField] private Toggle skipTutorialsToggle;
        [SerializeField] private Toggle confineCursorToggle;
        [SerializeField] private Toggle vsyncToggle;
        [SerializeField] private Toggle showVideosToggle;
        [SerializeField] private GameObject miscParentGO;
        
        [Header("Audio Settings UI")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;

        private List<Resolution> resolutions;
        private Settings _settings;

        public bool hasShownEarlyAccessAnnouncement { get; private set; }

        #region getters
        public Settings settings => _settings;
        public bool doNotShowVideos => true; // settings.doNotShowVideos;
        #endregion

        #region Monobehaviours
        private void Awake() {
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Application.targetFrameRate = targetFrameRate;
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
            //set min-max volume levels.
            masterVolumeSlider.minValue = AudioManager.Minimum_Volume_Level;
            masterVolumeSlider.maxValue = AudioManager.Maximum_Volume_Level;
            musicVolumeSlider.minValue = AudioManager.Minimum_Volume_Level;
            musicVolumeSlider.maxValue = AudioManager.Maximum_Volume_Level;
            
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
        public bool IsShowing() {
            return settingsGO.activeSelf;
        }
        private void UpdateUI() {
            edgePanningToggle.isOn = settings.useEdgePanning;
            confineCursorToggle.SetIsOnWithoutNotify(settings.confineCursor);
            skipTutorialsToggle.SetIsOnWithoutNotify(settings.skipTutorials);
            miscParentGO.SetActive(SceneManager.GetActiveScene().name == "MainMenu");

            resolutionsDropdown.value =
                UtilityScripts.GameUtilities.GetOptionIndex(resolutionsDropdown, settings.resolution);
            graphicsDropdown.value = settings.graphicsQuality;
            fullscreenToggle.isOn = settings.fullscreen;

            masterVolumeSlider.value = settings.masterVolume;
            musicVolumeSlider.value = settings.musicVolume;

            vsyncToggle.isOn = settings.isVsyncOn;
            showVideosToggle.isOn = !settings.doNotShowVideos;
        }
        public void OnToggleEdgePanning(bool isOn) {
            _settings.useEdgePanning = isOn;
            Messenger.Broadcast(SettingsSignals.EDGE_PANNING_TOGGLED, isOn);
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
                     useEdgePanning = false,
                     confineCursor = false,
                     musicVolume = AudioManager.Maximum_Volume_Level,
                     masterVolume = AudioManager.Maximum_Volume_Level,
                     isVsyncOn = false,
                     doNotShowVideos = true,
                     skipEarlyAccessAnnouncement = false,
                 };
                 Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, settings.fullscreen);
                 QualitySettings.SetQualityLevel(settings.graphicsQuality);
            }
            SetVsync(_settings.isVsyncOn);
            SetConfineCursor(_settings.confineCursor);
        }
        #endregion

        #region Applying
        public void ApplySettings() {
            SetGraphicsQuality();
            SetResolution();
            SetFullscreen();

            _settings.useEdgePanning = edgePanningToggle.isOn;
            _settings.skipTutorials = skipTutorialsToggle.isOn;
            _settings.isVsyncOn = vsyncToggle.isOn;
            _settings.doNotShowVideos = !showVideosToggle.isOn;
            _settings.confineCursor = confineCursorToggle.isOn;
            ApplyConfineCursorSetting();
            
            //resolution
            Screen.fullScreen = settings.fullscreen;
            string[] dimensionsStr = settings.resolution.Split('x');
            Assert.IsTrue(dimensionsStr.Length == 2, $"Resolution in graphics settings is invalid {settings.resolution}");
            int width = Int32.Parse(dimensionsStr[0]);
            int height = Int32.Parse(dimensionsStr[1]);
            Screen.SetResolution(width, height, settings.fullscreen);
            
            //quality
            QualitySettings.SetQualityLevel(settings.graphicsQuality);

            //VSync
            SetVsync(_settings.isVsyncOn);
            
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
            Messenger.Broadcast(SettingsSignals.MUSIC_VOLUME_CHANGED, volume);
        }
        public void OnMasterVolumeChanged(float volume) {
            _settings.masterVolume = volume;
            Messenger.Broadcast(SettingsSignals.MASTER_VOLUME_CHANGED, volume);
        }
        #endregion

        #region Tutorials
        public void OnToggleSkipTutorials(bool state) {
            _settings.skipTutorials = state;
            Messenger.Broadcast(SettingsSignals.ON_SKIP_TUTORIALS_CHANGED, state);
        }
        #endregion

        #region Vsync
        //public void OnToggleVsync(bool state) {
        //    _settings.isVsyncOn = state;
        //    SetVsync(_settings.isVsyncOn);
        //}
        public void SetVsync(bool state) {
            if (state) {
                // Turn on v-sync
                QualitySettings.vSyncCount = 1;
            } else {
                // Turn on v-sync
                QualitySettings.vSyncCount = 0;
            }
        }
        #endregion

        #region Cursor
        public void OnToggleConfineCursor(bool state) {
            SetConfineCursor(state);
        }
        private void SetConfineCursor(bool state) {
            _settings.confineCursor = state;
            ApplyConfineCursorSetting();
        }
        private void ApplyConfineCursorSetting() {
            if (_settings.confineCursor) {
                Cursor.lockState = CursorLockMode.Confined;
            } else {
                Cursor.lockState = CursorLockMode.None;
            }
        }
        #endregion

        #region Early Access Announcement
        public void OnToggleSkipEarlyAccessAnnouncement(bool state) {
            _settings.skipEarlyAccessAnnouncement = state;
        }
        public void SetHasShownEarlyAccessAnnouncement(bool state) {
            hasShownEarlyAccessAnnouncement = state;
        }
        #endregion
    }
}
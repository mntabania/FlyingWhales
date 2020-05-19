using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour {

    public TMP_Dropdown resolutionsDropdown;
    public TMP_Dropdown graphicsDropdown;
    public Toggle fullscreenToggle;
    public GameObject settingsGO;

    private List<Resolution> resolutions;

    private void Start() {
        ConstructGraphicsQuality();
        ConstructResolutions();
    }

    public void OpenSettings() {
        SetCurrentGraphicsQualityToDropdown();
        SetCurrentResolutionToDropdown();
        SetCurrentFullscreenState();
        settingsGO.SetActive(true);
    }

    public void CloseSettings() {
        settingsGO.SetActive(false);
    }

    public void ApplySettings() {
        SetGraphicsQuality();
        SetResolution();
        SetFullscreen();
    }
    public void CheckResolution(int index) {
        if((resolutionsDropdown.options.Count - 1) == index) {
            fullscreenToggle.isOn = true;
        }
    }
    public void OnToggleFullscreen(bool state) {
        // if(resolutionsDropdown.value == (resolutionsDropdown.options.Count - 1)) {
        //     fullscreenToggle.isOn = true;
        // }
    }

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
            options.Add($"{resolutions[i].width} x {resolutions[i].height}");
        }
        resolutionsDropdown.AddOptions(options);
    }
    private void ConstructGraphicsQuality() {
        graphicsDropdown.ClearOptions();
        graphicsDropdown.AddOptions(QualitySettings.names.ToList());
    }

    #region Current Settings
    private void SetCurrentResolutionToDropdown() {
        for (int i = 0; i < resolutions.Count; i++) {
            if(resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height) {
                resolutionsDropdown.value = i;
                resolutionsDropdown.RefreshShownValue();
                break;
            }
        }
    }
    private void SetCurrentGraphicsQualityToDropdown() {
        graphicsDropdown.value = QualitySettings.GetQualityLevel();
        graphicsDropdown.RefreshShownValue();
    }
    private void SetCurrentFullscreenState() {
        fullscreenToggle.isOn = Screen.fullScreen;
    }
    private bool HasResolution(int width, int height) {
        for (int i = 0; i < resolutions.Count; i++) {
            if(resolutions[i].width == width && resolutions[i].height == height) {
                return true;
            }
        }
        return false;
    }
    private bool HasResolution(Resolution resolution) {
        for (int i = 0; i < resolutions.Count; i++) {
            if (resolutions[i].width == resolution.width && resolutions[i].height == resolution.height) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Applying Settings
    private void SetGraphicsQuality() {
        if(QualitySettings.GetQualityLevel() != graphicsDropdown.value) {
            QualitySettings.SetQualityLevel(graphicsDropdown.value);
        }
    }
    private void SetResolution() {
        Resolution newResolution = resolutions[resolutionsDropdown.value];
        if (newResolution.width == Screen.currentResolution.width && newResolution.height == Screen.currentResolution.height) {
            return;
        }
        Screen.SetResolution(newResolution.width, newResolution.height, Screen.fullScreen);
    }
    private void SetFullscreen() {
        if(Screen.fullScreen != fullscreenToggle.isOn) {
            Screen.fullScreen = fullscreenToggle.isOn;
        }
    }
    #endregion
}

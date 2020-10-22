using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Managers;
using Settings;
using TMPro;
using Ruinarch.Custom_UI;

public class MainMenuUI : MonoBehaviour {

    public static MainMenuUI Instance = null;

    [SerializeField] private EasyTween buttonsTween;
    [SerializeField] private EasyTween titleTween;

    [SerializeField] private EasyTween glowTween;
    [SerializeField] private EasyTween glow2Tween;

    [SerializeField] private Image bg;

    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;

    [Header("Steam")]
    [SerializeField] private TextMeshProUGUI steamName;
    
    [Header("Version")]
    [SerializeField] private TextMeshProUGUI version;
    
    [Header("Yes/No Confirmation")]
    public YesNoConfirmation yesNoConfirmation;
    
    [Header("Load Game")]
    [SerializeField] private Button loadGameButton;
    [SerializeField] private LoadWindow loadWindow;

    [Header("Early Access Announcement")]
    [SerializeField] private GameObject earlyAccessAnnouncementGO;
    [SerializeField] private GameObject roadmapGO;
    [SerializeField] private RuinarchToggle skipEarlyAccessAnnouncementToggle;

    private void Awake() {
        Instance = this;
    }
    private void Start() {
        if (!SaveManager.Instance.savePlayerManager.hasSavedDataPlayer) {
            SaveManager.Instance.savePlayerManager.CreateNewSaveDataPlayer();
        }
        newGameButton.interactable = true;
        steamName.text = $"Logged in as: <b>{SteamworksManager.Instance.GetSteamName()}</b>";
        version.text = $"Version: {Application.version}";
        //Set current save data to null everytime this is loaded, this is so that the previous save file is not loaded if new game was clicked
        SaveManager.Instance.saveCurrentProgressManager.SetCurrentSaveDataPath(string.Empty); 
        UpdateButtonStates();
        Messenger.AddListener<string>(Signals.SAVE_FILE_DELETED, OnSaveFileDeleted);
    }
    public void ShowMenuButtons() {
        titleTween.OnValueChangedAnimation(true);
        glowTween.OnValueChangedAnimation(true);
        buttonsTween.OnValueChangedAnimation(true);
    }
    private void HideMenuButtons() {
        buttonsTween.OnValueChangedAnimation(false);
    }
    public void ShowEarlyAccessAnnouncement() {
        if (!SettingsManager.Instance.hasShownEarlyAccessAnnouncement) {
            SettingsManager.Instance.SetHasShownEarlyAccessAnnouncement(true);
            if (SettingsManager.Instance.settings.skipEarlyAccessAnnouncement) {
                earlyAccessAnnouncementGO.SetActive(false);
                roadmapGO.SetActive(true);
            } else {
                skipEarlyAccessAnnouncementToggle.isOn = SettingsManager.Instance.settings.skipEarlyAccessAnnouncement;
                earlyAccessAnnouncementGO.SetActive(true);
                roadmapGO.SetActive(false);
            }
        }
    }
    public void OnClickOkEarlyAccessAnnouncement() {
        earlyAccessAnnouncementGO.SetActive(false);
        roadmapGO.SetActive(true);
    }
    public void ExitGame() {
        Application.Quit();
    }
    public void OnClickPlayGame() {
        WorldSettings.Instance.Open(); 
    }
    public void OnClickContinue() {
        //Load latest save
        string latestFile = SaveManager.Instance.saveCurrentProgressManager.GetLatestSaveFile();
        if (!string.IsNullOrEmpty(latestFile)) {
            if (SaveUtilities.IsSaveFileValid(latestFile)) {
                SaveManager.Instance.saveCurrentProgressManager.SetCurrentSaveDataPath(latestFile);
                MainMenuManager.Instance.StartGame();    
            } else {
                yesNoConfirmation.ShowYesNoConfirmation("Incompatible Save", "The latest save file is no longer compatible with the current game version. Do you want to delete it?", () => OnConfirmDelete(latestFile), showCover: true, layer:50);
            }
        } else {
            //in case no latest file was found, doubt that this will happen.
            OnClickPlayGame();
        }
    }
    public void OnClickSettings() {
        SettingsManager.Instance.OpenSettings();
    }
    public void OnClickDiscord() {
        Application.OpenURL("http://discord.ruinarch.com/");
    }
    private void UpdateButtonStates() {
        bool hasSaves = SaveManager.Instance.saveCurrentProgressManager.HasAnySaveFiles();
        continueButton.interactable = hasSaves;
        loadGameButton.interactable = hasSaves;
    }
    private void OnConfirmDelete(string path) {
        File.Delete(path);
        Messenger.Broadcast(Signals.SAVE_FILE_DELETED, path);
    }

    #region Load Game
    private void OnSaveFileDeleted(string saveFileDeleted) {
        UpdateButtonStates();
        if (!SaveManager.Instance.saveCurrentProgressManager.HasAnySaveFiles()) {
            //automatically close load window when all saves have been deleted.
            loadWindow.Close();
        }
    }
    public void OnClickLoadGame() {
        loadWindow.Open();
    }
    #endregion
}

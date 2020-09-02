using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Managers;
using Settings;
using TMPro;

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
    [SerializeField] private ScrollRect loadGameScrollRect;
    [SerializeField] private GameObject saveItemPrefab;
    [SerializeField] private GameObject loadGameWindow;
    private string[] saveFiles;
    
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
        saveFiles = System.IO.Directory.GetFiles(UtilityScripts.Utilities.gameSavePath, "*.sav");
        SaveManager.Instance.saveCurrentProgressManager.SetCurrentSaveDataPath(string.Empty); //Set current save data to null everytime this is loaded, this is so that the previous save file is not loaded if new game was clicked
        UpdateButtonStates();
    }
    public void ShowMenuButtons() {
        titleTween.OnValueChangedAnimation(true);
        glowTween.OnValueChangedAnimation(true);
        buttonsTween.OnValueChangedAnimation(true);
    }
    private void HideMenuButtons() {
        buttonsTween.OnValueChangedAnimation(false);
    }
    public void ExitGame() {
        Application.Quit();
    }
    public void OnClickPlayGame() {
        WorldSettings.Instance.Open(); 
    }
    public void OnClickContinue() {
        //Load latest save
        string latestFile = string.Empty;
        for (int i = 0; i < saveFiles.Length; i++) {
            string saveFile = saveFiles[i];
            if (string.IsNullOrEmpty(latestFile)) {
                latestFile = saveFile;
            } else {
                //compare times
                DateTime writeTimeOfCurrentSave = System.IO.File.GetLastWriteTime(saveFile);
                DateTime writeTimeOfLatestSave = System.IO.File.GetLastWriteTime(latestFile);
                if (writeTimeOfCurrentSave > writeTimeOfLatestSave) {
                    latestFile = saveFile;
                }
            }
        }

        if (!string.IsNullOrEmpty(latestFile)) {
            SaveManager.Instance.saveCurrentProgressManager.SetCurrentSaveDataPath(latestFile);
            MainMenuManager.Instance.StartGame();
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
        bool hasSaves = saveFiles != null && saveFiles.Length > 0;
        continueButton.interactable = hasSaves;
        loadGameButton.interactable = hasSaves;
    }

    #region Load Game
    public void RedetermineSaveFiles() {
        saveFiles = System.IO.Directory.GetFiles(UtilityScripts.Utilities.gameSavePath, "*.sav");
        if (saveFiles.Length > 0) {
            LoadSavedGameItems();    
        } else {
            OnClickCloseLoadGame();
        }
        
        UpdateButtonStates();
    }
    public void OnClickLoadGame() {
        LoadSavedGameItems();
        loadGameWindow.gameObject.SetActive(true);
        // SaveManager.Instance.useSaveData = true;
        // newGameButton.interactable = false;
        // // loadGameButton.interactable = false;
        // AudioManager.Instance.TransitionToLoading();
        // MainMenuManager.Instance.LoadMainGameScene();
    }
    public void OnClickCloseLoadGame() {
        loadGameWindow.gameObject.SetActive(false);
    }
    private void LoadSavedGameItems() {
        UtilityScripts.Utilities.DestroyChildren(loadGameScrollRect.content);
        for (int i = 0; i < saveFiles.Length; i++) {
            string saveFile = saveFiles[i];
            GameObject saveItemGO = GameObject.Instantiate(saveItemPrefab, loadGameScrollRect.content);
            SaveItem saveItem = saveItemGO.GetComponent<SaveItem>();
            saveItem.SetSaveFile(saveFile);
        }
    }
    #endregion
}

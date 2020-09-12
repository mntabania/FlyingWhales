using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : PopupMenuBase {

    [SerializeField] private GameObject saveLoadingGO;
    [SerializeField] private TextMeshProUGUI saveLbl;
    [SerializeField] private Button saveBtn;
    [SerializeField] private LoadWindow loadWindow;
    
    public override void Open() {
        UIManager.Instance.Pause();
        UIManager.Instance.SetSpeedTogglesState(false);
        base.Open();
        UpdateSaveBtnState();
    }
    public override void Close() {
        UIManager.Instance.ResumeLastProgressionSpeed();
        base.Close();
    }
    public void OpenSettings() {
        SettingsManager.Instance.OpenSettings();
    }
    public void SaveGame() {
        if (SaveManager.Instance.saveCurrentProgressManager.isSaving) {
            //prevent saving if player is already saving
            return;
        }
        SaveManager.Instance.savePlayerManager.SavePlayerData();
        SaveCurrentProgress();
    }
    public void QuickSave() {
        if (SaveManager.Instance.saveCurrentProgressManager.isSaving) {
            //prevent saving if player is already saving
            return;
        }
        if (!SaveManager.Instance.saveCurrentProgressManager.CanSaveCurrentProgress()) {
            return;
        }
        UIManager.Instance.Pause();
        UIManager.Instance.SetSpeedTogglesState(false);
        SaveManager.Instance.savePlayerManager.SavePlayerData();
        SaveCurrentProgress(saveCallback: UIManager.Instance.ResumeLastProgressionSpeed);
    }
    public void OnClickLoadGame() {
        loadWindow.Open();
    }
    public void ExitGame() {
        Application.Quit();
    }
    public void AbandonWorld() {
        UIManager.Instance.ShowYesNoConfirmation("Abandon World", "Are you sure you want to abandon this world?", Abandon, layer: 50, showCover: true);
    }
    public void ReportABug() {
        UIManager.Instance.ShowYesNoConfirmation("Open Browser", "To report a bug, the game needs to open a Web browser, do you want to proceed?",
            () => Application.OpenURL("https://forms.gle/gcoa8oHxywFLegNx7"), layer: 50, showCover: true);
    }
    public void SubmitFeedback() {
        UIManager.Instance.ShowYesNoConfirmation("Open Browser", "To submit feedback, the game needs to open a Web browser, do you want to proceed?",
            () => Application.OpenURL("https://forms.gle/6QYHiSmU8ySVGSXp7"), layer: 50, showCover: true);
    }
    private void Abandon() {
        DOTween.Clear(true);
        SaveManager.Instance.savePlayerManager.SavePlayerData();
        Messenger.Cleanup();
        LevelLoaderManager.Instance.UpdateLoadingInfo(string.Empty);
        LevelLoaderManager.Instance.LoadLevel("MainMenu", true);
    }
    /// <summary>
    /// Load the scene again after setting a file to load at
    /// <see cref="LoadWindow.OnLoadFileChosen"/>.
    /// </summary>
    public void LoadSave() {
        DOTween.Clear(true);
        Messenger.Cleanup();
        LevelLoaderManager.Instance.SetLoadingState(true);
        AudioManager.Instance.TransitionToLoading();
        LevelLoaderManager.Instance.UpdateLoadingInfo("Initializing data...");
        LevelLoaderManager.Instance.UpdateLoadingBar(0.1f, 3f);
        LevelLoaderManager.Instance.LoadLevel("Game");
    }
    public bool IsLoadWindowShowing() {
        return loadWindow.isShowing;
    }
    public void CloseLoadWindow() {
        loadWindow.Close();
    }

    #region Saving
    public void ShowSaveLoading() {
        saveLoadingGO.SetActive(true);
    }
    public void HideSaveLoading() {
        saveLoadingGO.SetActive(false);
    }
    public void UpdateSaveMessage(string message) {
        saveLbl.text = message;
    }
    private void UpdateSaveBtnState() {
        saveBtn.interactable = SaveManager.Instance.saveCurrentProgressManager.CanSaveCurrentProgress();
    }
    private void SaveCurrentProgress(string fileName = "", System.Action saveCallback = null) {
        if (SaveManager.Instance.saveCurrentProgressManager.CanSaveCurrentProgress()) {
            SaveManager.Instance.saveCurrentProgressManager.DoManualSave(fileName, saveCallback);
        } else {
            PlayerUI.Instance.ShowGeneralConfirmation("Save Progress", "Cannot save while seizing.");
        }
    }
    #endregion
}

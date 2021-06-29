using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class OptionsMenu : PopupMenuBase {

    [SerializeField] private GameObject saveLoadingGO;
    [SerializeField] private TextMeshProUGUI saveLbl;
    [SerializeField] private Button saveBtn;

    [SerializeField] private GameObject inputSaveNameGO;
    [SerializeField] private InputField saveNameInput;

    private Action m_saveAction;

    #region Listeners
    public void SubscribeListeners() {
        Messenger.AddListener<KeyCode>(ControlsSignals.KEY_DOWN, OnKeyDown);
    }
    private void OnKeyDown(KeyCode p_keyPressed) {
        if (p_keyPressed == KeyCode.F5) {
            QuickSave();
        }
    }
    #endregion
    
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
        m_saveAction = ActualSavegame;
        ShowInputFileName();
    }

    void ActualSavegame() {
        if (SaveManager.Instance.saveCurrentProgressManager.isSaving || SaveManager.Instance.saveCurrentProgressManager.isWritingToDisk) {
            //prevent saving if player is already saving
            return;
        }
        SaveManager.Instance.savePlayerManager.SavePlayerData();
        SaveCurrentProgress(saveNameInput.text);
    }

    public void SaveAndExit() {
        m_saveAction = ActualSaveAndExit;
        ShowInputFileName();
    }

    void ActualSaveAndExit() {
        if (SaveManager.Instance.saveCurrentProgressManager.isSaving || SaveManager.Instance.saveCurrentProgressManager.isWritingToDisk) {
            //prevent saving if player is already saving
            return;
        }
        SaveManager.Instance.savePlayerManager.SavePlayerData();
        SaveCurrentProgress(saveNameInput.text);
        saveLoadingGO.SetActive(true);
        StartCoroutine(WaitForLoadToFinishThenExit());
    }

    IEnumerator WaitForLoadToFinishThenExit() { 
        while(SaveManager.Instance.saveCurrentProgressManager.isWritingToDisk || SaveManager.Instance.saveCurrentProgressManager.isSaving) {
            if (!saveLoadingGO.activeSelf) {
                saveLoadingGO.SetActive(true);
            }
            yield return 0;
		}
        saveLoadingGO.SetActive(false);
        Application.Quit();
    }

    public void QuickSave() {
        if (SaveManager.Instance.saveCurrentProgressManager.isSaving || SaveManager.Instance.saveCurrentProgressManager.isWritingToDisk) {
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
        UIManager.Instance.OpenLoadWindow();
    }
    public void ExitGame() {
        UIManager.Instance.ShowYesNoConfirmation("Exit Game", "Are you sure you want to exit?", Application.Quit, layer: 50, showCover: true);
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

    void ShowInputFileName() {
        inputSaveNameGO.gameObject.SetActive(true);
    }

    public void HideInputFileName() {
        inputSaveNameGO.gameObject.SetActive(false);
    }
    public void SaveNameFromInputName() {
        HideInputFileName();
        m_saveAction?.Invoke();
    }
    private void Abandon() {
        DOTween.Clear(true);
        SaveManager.Instance.savePlayerManager.SavePlayerData();
        LevelLoaderManager.Instance.UpdateLoadingInfo(string.Empty);
        LevelLoaderManager.Instance.LoadLevel("MainMenu", true);
    }
    /// <summary>
    /// Load the scene again after setting a file to load at
    /// <see cref="LoadWindow.OnLoadFileChosen"/>.
    /// </summary>
    public void LoadSave() {
        DOTween.Clear(true);
        LevelLoaderManager.Instance.SetLoadingState(true);
        AudioManager.Instance.TransitionToLoading();
        LevelLoaderManager.Instance.UpdateLoadingInfo("Initializing Data...");
        LevelLoaderManager.Instance.UpdateLoadingBar(0.1f, 3f);
        LevelLoaderManager.Instance.LoadLevel("Game");
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
    public void UpdateSaveBtnState() {
        saveBtn.interactable = SaveManager.Instance.saveCurrentProgressManager.CanSaveCurrentProgress() && !SaveManager.Instance.saveCurrentProgressManager.isSaving && !SaveManager.Instance.saveCurrentProgressManager.isWritingToDisk;
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

using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Settings;
using UnityEngine;

public class OptionsMenu : PopupMenuBase {
    
    public override void Open() {
        UIManager.Instance.Pause();
        UIManager.Instance.SetSpeedTogglesState(false);
        base.Open();
    }
    public override void Close() {
        UIManager.Instance.ResumeLastProgressionSpeed();
        base.Close();
    }
    public void OpenSettings() {
        SettingsManager.Instance.OpenSettings();
    }
    public void SaveGame() {
        SaveManager.Instance.Save();
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
    private void Abandon() {
        DOTween.Clear(true);
        SaveManager.Instance.Save();
        LevelLoaderManager.Instance.UpdateLoadingInfo(string.Empty);
        LevelLoaderManager.Instance.LoadLevel("MainMenu", true);
        Messenger.Cleanup();
    }
}

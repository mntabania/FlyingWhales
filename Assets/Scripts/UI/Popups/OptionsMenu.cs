using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
    
    public void ToggleMute(bool state) {
        AudioManager.Instance.SetMute(state);
    }
    public void ToggleEdgePanning(bool state) {
        CameraMove.Instance.AllowEdgePanning(state);
        InnerMapCameraMove.Instance.AllowEdgePanning(state);
    }
    public void SaveGame() {
        SaveManager.Instance.SaveCurrentStateOfWorld();
    }
    public void ExitGame() {
        Application.Quit();
    }
    public void AbandonWorld() {
        UIManager.Instance.ShowYesNoConfirmation("Abandon World", "Are you sure you want to abandon this world?", Abandon, layer: 50);
    }
    private void Abandon() {
        DOTween.Clear(true);
        SaveManager.Instance.SaveCurrentStateOfWorld();
        LevelLoaderManager.Instance.LoadLevel("MainMenu");
    }
}

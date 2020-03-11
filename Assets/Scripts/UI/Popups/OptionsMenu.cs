﻿using System.Collections;
using System.Collections.Generic;
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
}

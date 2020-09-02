using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class SaveItem : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI saveNameLbl;
    [SerializeField] private TextMeshProUGUI timeStampLbl;

    private string path;
    
    public void SetSaveFile(string path) {
        this.path = path;
        DateTime lastWriteTime = System.IO.File.GetLastWriteTime(path);
        timeStampLbl.text = lastWriteTime.ToString("yyyy-MM-dd HH:mm");
        saveNameLbl.text = Path.GetFileNameWithoutExtension(path);
    }

    public void OnClickItem() {
        MainMenuUI.Instance.yesNoConfirmation.ShowYesNoConfirmation("Load Game", $"Are you sure you want to load {saveNameLbl.text}?", OnConfirmLoad, showCover: true);
    }

    private void OnConfirmLoad() {
        SaveManager.Instance.saveCurrentProgressManager.SetCurrentSaveDataPath(path);
        MainMenuManager.Instance.StartGame();
    }
}

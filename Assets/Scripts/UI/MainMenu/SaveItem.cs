using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;

public class SaveItem : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI saveNameLbl;
    [SerializeField] private TextMeshProUGUI timeStampLbl;

    private string path;
    private string json;
    public void SetSaveFile(string path) {
        this.path = path;
        DateTime lastWriteTime = System.IO.File.GetLastWriteTime(path);
        timeStampLbl.text = lastWriteTime.ToString("yyyy-MM-dd HH:mm");
        saveNameLbl.text = Path.GetFileNameWithoutExtension(path);
        json = File.ReadAllText(path);
    }

    public void OnClickItem() {
        string saveFileVersion = SaveUtilities.GetGameVersionOfSaveFile(json);
        if (saveFileVersion != Application.version) {
            //TODO: Make a system for incompatible saves?
            //no longer compatible
            if (MainMenuUI.Instance != null) {
                MainMenuUI.Instance.yesNoConfirmation.ShowYesNoConfirmation("Incompatible Save", "This save file is no longer compatible with the current game version. Do you want to delete it?", OnConfirmDelete, showCover: true, layer:50);    
            } else if (UIManager.Instance != null) {
                UIManager.Instance.yesNoConfirmation.ShowYesNoConfirmation("Incompatible Save", "This save file is no longer compatible with the current game version. Do you want to delete it?", OnConfirmDelete, showCover: true, layer:50);
            }
        } else {
            //TODO: Convert yesNoShow to Signal perhaps?
            if (MainMenuUI.Instance != null) {
                MainMenuUI.Instance.yesNoConfirmation.ShowYesNoConfirmation("Load Game", $"Are you sure you want to load {saveNameLbl.text}?", OnConfirmLoad, showCover: true, layer:50);    
            } else if (UIManager.Instance != null) {
                UIManager.Instance.yesNoConfirmation.ShowYesNoConfirmation("Load Game", $"Are you sure you want to load {saveNameLbl.text}?", OnConfirmLoad, showCover: true, layer:50);
            }    
        }
    }
    
    
    private void OnConfirmLoad() {
        Messenger.Broadcast(Signals.LOAD_SAVE_FILE, path);
    }
    public void OnClickDelete() {
        if (MainMenuUI.Instance != null) {
            MainMenuUI.Instance.yesNoConfirmation.ShowYesNoConfirmation("Delete Save", $"Are you sure you want to delete {saveNameLbl.text}?", OnConfirmDelete, showCover: true, layer:50);    
        } else if (UIManager.Instance != null) {
            UIManager.Instance.yesNoConfirmation.ShowYesNoConfirmation("Delete Save", $"Are you sure you want to delete {saveNameLbl.text}?", OnConfirmDelete, showCover: true, layer:50);
        }
    }
    private void OnConfirmDelete() {
        File.Delete(path);
        GameObject.Destroy(this.gameObject);
        Messenger.Broadcast(Signals.SAVE_FILE_DELETED, path);
    }
}

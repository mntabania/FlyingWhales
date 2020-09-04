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
        //TODO: Convert yesNoShow to Signal perhaps?
        if (MainMenuUI.Instance != null) {
            MainMenuUI.Instance.yesNoConfirmation.ShowYesNoConfirmation("Load Game", $"Are you sure you want to load {saveNameLbl.text}?", OnConfirmLoad, showCover: true, layer:50);    
        } else if (UIManager.Instance != null) {
            UIManager.Instance.yesNoConfirmation.ShowYesNoConfirmation("Load Game", $"Are you sure you want to load {saveNameLbl.text}?", OnConfirmLoad, showCover: true, layer:50);
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

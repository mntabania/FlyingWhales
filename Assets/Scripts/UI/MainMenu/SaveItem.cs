using System;
using System.IO;
using System.IO.Compression;
using TMPro;
using UnityEngine;

public class SaveItem : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI saveNameLbl;
    [SerializeField] private TextMeshProUGUI timeStampLbl;

    private string path;
    private string json;
    public void SetSaveFile(string path) {
        this.path = path;
        DateTime lastWriteTime = File.GetLastWriteTime(path);
        timeStampLbl.text = lastWriteTime.ToString("yyyy-MM-dd HH:mm");
        string fileName = Path.GetFileNameWithoutExtension(path);
        string formattedFileName = fileName.Substring(0, fileName.IndexOf('(')).Replace(" ", "").Replace('_', ' ').Replace('-', ':');
        saveNameLbl.text = formattedFileName.Replace(' ', '-');
    }

    public void OnClickItem() {
        if (string.IsNullOrEmpty(json)) {
            using (ZipArchive zip = ZipFile.Open(path, ZipArchiveMode.Read)) {
                foreach (ZipArchiveEntry entry in zip.Entries) {
                    if (entry.Name == "mainSave.sav") {
                        using (StreamReader reader = new StreamReader(entry.Open())) {
                            json = reader.ReadToEnd();
                        }
                        break;
                    }
                }
            }
        }
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
        Destroy(gameObject);
        Messenger.Broadcast(Signals.SAVE_FILE_DELETED, path);
    }
    private void OnDestroy() {
        path = string.Empty;
        json = string.Empty;
    }
}

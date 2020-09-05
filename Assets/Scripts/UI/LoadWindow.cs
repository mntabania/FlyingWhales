using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadWindow : PopupMenuBase  {

    [Header("Load Game")]
    [SerializeField] private ScrollRect loadGameScrollRect;
    [SerializeField] private GameObject saveItemPrefab;

    public override void Open() {
        LoadSavedGameItems();
        Messenger.AddListener<string>(Signals.LOAD_SAVE_FILE, OnLoadFileChosen);
        Messenger.AddListener<string>(Signals.SAVE_FILE_DELETED, OnSaveFileDeleted);
        base.Open();
        
    }
    public override void Close() {
        base.Close();
        Messenger.RemoveListener<string>(Signals.LOAD_SAVE_FILE, OnLoadFileChosen);
        Messenger.RemoveListener<string>(Signals.SAVE_FILE_DELETED, OnSaveFileDeleted);
    }
    private void OnSaveFileDeleted(string deleted) {
        LoadSavedGameItems();
    }
    private void OnLoadFileChosen(string path) {
        SaveManager.Instance.saveCurrentProgressManager.SetCurrentSaveDataPath(path);
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.name == "MainMenu") {
            MainMenuManager.Instance.StartGame();    
        } else if (activeScene.name == "Game") {
            UIManager.Instance.optionsMenu.LoadSave();
        }
    }
    
    private void LoadSavedGameItems() {
        UtilityScripts.Utilities.DestroyChildren(loadGameScrollRect.content);
        string[] saveFiles = System.IO.Directory.GetFiles(UtilityScripts.Utilities.gameSavePath, "*.sav");
        saveFiles = saveFiles.OrderBy(System.IO.File.GetLastWriteTime).ToArray();
        for (int i = 0; i < saveFiles.Length; i++) {
            string saveFile = saveFiles[i];
            GameObject saveItemGO = GameObject.Instantiate(saveItemPrefab, loadGameScrollRect.content);
            SaveItem saveItem = saveItemGO.GetComponent<SaveItem>();
            saveItem.SetSaveFile(saveFile);
            (saveItem.transform as RectTransform)?.SetAsFirstSibling();
        }
    }
}

using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadWindow : PopupMenuBase  {

    [Header("Load Game")]
    [SerializeField] private ScrollRect loadGameScrollRect;
    [SerializeField] private GameObject saveItemPrefab;
    [SerializeField] private GameObject fetchSavesCover;

    private bool isFetchingSaves;
    
    public override void Open() {
        if (UIManager.Instance != null) {
            if (!UIManager.Instance.optionsMenu.isShowing) {
                UIManager.Instance.Pause();
                UIManager.Instance.SetSpeedTogglesState(false);    
            }
        }
        // LoadSavedGameItems();
        Messenger.AddListener<string>(UISignals.LOAD_SAVE_FILE, OnLoadFileChosen);
        // Messenger.AddListener<string>(Signals.SAVE_FILE_DELETED, OnSaveFileDeleted);
        isFetchingSaves = false;
        fetchSavesCover.gameObject.SetActive(false);
        base.Open();
        // StartCoroutine(LoadSaveGamesCoroutine());
        LoadSavedGameItems();
    }
    public override void Close() {
        if (isFetchingSaves) {
            //do not allow close while saves are being loaded.
            return;
        }
        if (UIManager.Instance != null) {
            if (!UIManager.Instance.optionsMenu.isShowing) {
                UIManager.Instance.ResumeLastProgressionSpeed();
            }
        }
        base.Close();
        isFetchingSaves = false;
        fetchSavesCover.gameObject.SetActive(false);
        Messenger.RemoveListener<string>(UISignals.LOAD_SAVE_FILE, OnLoadFileChosen);
        // Messenger.RemoveListener<string>(Signals.SAVE_FILE_DELETED, OnSaveFileDeleted);
    }
    private void OnSaveFileDeleted(string deleted) {
        LoadSavedGameItems();
        // StartCoroutine(LoadSaveGamesCoroutine());
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

    private IEnumerator LoadSaveGamesCoroutine() {
        isFetchingSaves = true;
        fetchSavesCover.gameObject.SetActive(true);
        UtilityScripts.Utilities.DestroyChildren(loadGameScrollRect.content);
        string[] saveFiles = System.IO.Directory.GetFiles(UtilityScripts.Utilities.gameSavePath, "*.zip");
        saveFiles = saveFiles.OrderBy(System.IO.File.GetLastWriteTime).ToArray();
        for (int i = 0; i < saveFiles.Length; i++) {
            string saveFile = saveFiles[i];
            GameObject saveItemGO = GameObject.Instantiate(saveItemPrefab, loadGameScrollRect.content);
            SaveItem saveItem = saveItemGO.GetComponent<SaveItem>();
            saveItem.SetSaveFile(saveFile);
            (saveItem.transform as RectTransform)?.SetAsFirstSibling();
            yield return null;
        }
        fetchSavesCover.gameObject.SetActive(false);
        isFetchingSaves = false;
    }
    
    private void LoadSavedGameItems() {
        UtilityScripts.Utilities.DestroyChildren(loadGameScrollRect.content);
        string[] saveFiles = System.IO.Directory.GetFiles(UtilityScripts.Utilities.gameSavePath, "*.zip");
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

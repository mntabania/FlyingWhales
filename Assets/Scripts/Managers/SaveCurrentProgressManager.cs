using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using UnityEngine;
using BayatGames.SaveGameFree;
using Newtonsoft.Json;
using Tutorial;
using Debug = UnityEngine.Debug;

public class SaveCurrentProgressManager : MonoBehaviour {
    public const string savedCurrentProgressFileName = "SAVED_CURRENT_PROGRESS";
    public SaveDataCurrentProgress currentSaveDataProgress { get; private set; }

    public bool isSaving { get; private set; }
    public string currentSaveDataPath { get; private set; }
    
    #region getters
    public bool hasSavedDataCurrentProgress => currentSaveDataProgress != null;
    #endregion

    #region Saving
    public void AddToSaveHub<T>(T data) where T : ISavable {
        currentSaveDataProgress.AddToSaveHub(data);
    }
    public bool CanSaveCurrentProgress() {
        if (PlayerManager.Instance.player != null && PlayerManager.Instance.player.seizeComponent.hasSeizedPOI) {
            return false;
        }
        if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial && !TutorialManager.Instance.hasCompletedImportantTutorials) {
            return false;
        }
        if (!GameManager.Instance.gameHasStarted) {
            return false;
        }
        return true;
    }
    public void DoManualSave(string fileName = "") {
        StartCoroutine(SaveCoroutine(fileName));
        // isSaving = true;
        // Stopwatch loadingWatch = new Stopwatch();
        // loadingWatch.Start();
        // currentSaveDataProgress = new SaveDataCurrentProgress();
        // currentSaveDataProgress.Initialize();
        // //date
        // currentSaveDataProgress.SaveDate();
        // currentSaveDataProgress.SaveWorldSettings();
        // currentSaveDataProgress.SavePlayer();
        // currentSaveDataProgress.SaveFactions();
        // currentSaveDataProgress.SaveCharacters();
        // currentSaveDataProgress.SaveJobs();
        //
        // //save world map
        // WorldMapSave worldMapSave = new WorldMapSave();
        // worldMapSave.SaveWorld(
        //     WorldConfigManager.Instance.mapGenerationData.chosenWorldMapTemplate,
        //     DatabaseManager.Instance.hexTileDatabase,
        //     DatabaseManager.Instance.regionDatabase,
        //     DatabaseManager.Instance.settlementDatabase,
        //     DatabaseManager.Instance.structureDatabase
        // );
        // currentSaveDataProgress.worldMapSave = worldMapSave;
        // currentSaveDataProgress.SaveTileObjects(DatabaseManager.Instance.tileObjectDatabase.allTileObjectsList);
        // currentSaveDataProgress.familyTreeDatabase = DatabaseManager.Instance.familyTreeDatabase;
        //
        //
        // if (string.IsNullOrEmpty(fileName)) {
        //     // fileName = savedCurrentProgressFileName;
        //     string timeStampStr = $"{currentSaveDataProgress.timeStamp.ToString("yyyy-MM-dd_HHmm")}";
        //     fileName = $"{timeStampStr}_{worldMapSave.worldType.ToString()}_Day{currentSaveDataProgress.day.ToString()}";
        // }
        //
        // string path = $"{UtilityScripts.Utilities.gameSavePath}{fileName}.sav";
        //
        // SaveGame.Save(path, currentSaveDataProgress);
        // //SaveData(path, currentSaveDataProgress);
        //
        // Debug.Log($"Saved new game at {path}");
        // loadingWatch.Stop();
        // Debug.Log($"\nTotal saving time is {loadingWatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds");
        // loadingWatch = null;
        // isSaving = false;
    }
    private IEnumerator SaveCoroutine(string fileName) {
        isSaving = true;
        UIManager.Instance.optionsMenu.ShowSaveLoading();
        UIManager.Instance.optionsMenu.UpdateSaveMessage("Saving current progress");
        Stopwatch loadingWatch = new Stopwatch();
        loadingWatch.Start();
        currentSaveDataProgress = new SaveDataCurrentProgress();
        currentSaveDataProgress.Initialize();
        currentSaveDataProgress.SaveDate();
        currentSaveDataProgress.SaveWorldSettings();
        currentSaveDataProgress.SavePlayer();
        yield return null;
        yield return StartCoroutine(currentSaveDataProgress.SaveFactionsCoroutine());
        yield return StartCoroutine(currentSaveDataProgress.SaveCharactersCoroutine());
        yield return StartCoroutine(currentSaveDataProgress.SaveJobsCoroutine());

        //save world map
        WorldMapSave worldMapSave = new WorldMapSave();
        yield return StartCoroutine(worldMapSave.SaveWorldCoroutine(WorldConfigManager.Instance.mapGenerationData.chosenWorldMapTemplate, DatabaseManager.Instance.hexTileDatabase,
            DatabaseManager.Instance.regionDatabase, DatabaseManager.Instance.settlementDatabase, DatabaseManager.Instance.structureDatabase));
        currentSaveDataProgress.worldMapSave = worldMapSave;
        yield return StartCoroutine(currentSaveDataProgress.SaveTileObjectsCoroutine());
        currentSaveDataProgress.familyTreeDatabase = DatabaseManager.Instance.familyTreeDatabase;

        UIManager.Instance.optionsMenu.UpdateSaveMessage("Finalizing...");
        yield return new WaitForSeconds(0.5f);

        if (string.IsNullOrEmpty(fileName)) {
            // fileName = savedCurrentProgressFileName;
            string timeStampStr = $"{currentSaveDataProgress.timeStamp.ToString("yyyy-MM-dd_HHmm")}";
            fileName = $"{timeStampStr}_{worldMapSave.worldType.ToString()}_Day{currentSaveDataProgress.day.ToString()}";
        }

        string path = $"{UtilityScripts.Utilities.gameSavePath}{fileName}.sav";
        filePath = path;
        var thread = new Thread(SaveCurrentDataToFile);
        thread.Start();

        while (thread.IsAlive) {
            yield return null;
        }
        thread = null;
        

        Debug.Log($"Saved new game at {path}");
        loadingWatch.Stop();
        Debug.Log($"\nTotal saving time is {loadingWatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds");
        loadingWatch = null;
        yield return null;
        UIManager.Instance.optionsMenu.HideSaveLoading();
        isSaving = false;
    }
    private string filePath;
    private void SaveCurrentDataToFile() {
        SaveGame.Save(filePath, currentSaveDataProgress);
    }
    #endregion

    #region Loading
    public void SetCurrentSaveDataPath(string path) {
        currentSaveDataPath = path;
    }
    public IEnumerator LoadSaveDataCurrentProgressBasedOnSetPath() {
        var thread = new Thread(() => LoadDataFromPath(currentSaveDataPath));
        thread.Start();
        while (thread.IsAlive) {
            yield return null;
        }
        thread = null;
    }
    private void LoadDataFromPath(string path) {
        currentSaveDataProgress = GetSaveFileData(currentSaveDataPath);
    }
    public SaveDataCurrentProgress LoadSaveDataCurrentProgress(string path) {
        return GetSaveFileData(path);
    }
    private SaveDataCurrentProgress GetSaveFileData(string path) {
        return SaveGame.Load<SaveDataCurrentProgress>(path);
        //return LoadData<SaveDataCurrentProgress>(path);
    }
    #endregion

    #region JSON Net
    public void SaveData<T>(string identifier, T obj) {
        if (string.IsNullOrEmpty(identifier)) {
            throw new System.ArgumentNullException("identifier");
        }
        string filePath = "";
        if (IsFilePath(identifier)) {
            filePath = identifier;
        } else {
            throw new System.Exception("identifier is not a file path!");
        }
        if (obj == null) {
            throw new System.Exception("Object to be saved is null!");
        }
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

        string json = JsonConvert.SerializeObject(obj);

        File.WriteAllText(filePath, json);

    }
    public T LoadData<T>(string identifier) {
        if (string.IsNullOrEmpty(identifier)) {
            throw new System.ArgumentNullException("identifier");
        }
        string filePath = "";
        if (IsFilePath(identifier)) {
            filePath = identifier;
        } else {
            throw new System.Exception("identifier is not a file path!");
        }
        string data = File.ReadAllText(filePath);

        T convertedObj = JsonConvert.DeserializeObject<T>(data);

        return convertedObj;
    }
    public bool IsFilePath(string str) {
        bool result = false;
        if (Path.IsPathRooted(str)) {
            try {
                Path.GetFullPath(str);
                result = true;
            } catch (System.Exception) {
                result = false;
            }
        }
        return result;
    }
    #endregion

}

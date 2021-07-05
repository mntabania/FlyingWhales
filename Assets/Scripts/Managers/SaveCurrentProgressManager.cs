using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Threading;
using UnityEngine;
using BayatGames.SaveGameFree;
using Managers;
using Tutorial;
using UtilityScripts;
using Debug = UnityEngine.Debug;

public class SaveCurrentProgressManager : MonoBehaviour {
    public const string savedCurrentProgressFileName = "SAVED_CURRENT_PROGRESS";
    public SaveDataCurrentProgress currentSaveDataProgress { get; private set; }

    public static readonly object THREAD_LOCKER = new object();

    private bool generalSaveFlag;
    private bool saveFileWriteFlag;
    private List<SaveTileObjectThreadQueueItem> objectThreadItems = new List<SaveTileObjectThreadQueueItem>();

    public bool isSaving { get; private set; }
    public bool isWritingToDisk { get; private set; }
    public string currentSaveDataPath { get; private set; }
    private string filePath;

    void LateUpdate() {
        if (isWritingToDisk) {
            if (saveFileWriteFlag) {
                //Save file is done writing to disk
                DoneSaveFileWriteToDisk();
            }
        }
    }

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
        if (UIManager.Instance != null) {
            if (UIManager.Instance.IsLoadWindowShowing()) {
                return false;    
            }
            if (UIManager.Instance.IsShowingEndScreen()) {
                return false;
            }
        }
        if (!GameManager.Instance.gameHasStarted) {
            return false;
        }
        return true;
    }
    public void DoManualSave(string fileName = "", Action saveCallback = null) {
        StartCoroutine(SaveThisGame(fileName, saveCallback));
    }
    private IEnumerator SaveCoroutine(string fileName, Action saveCallback = null) {
        isSaving = true;
        InnerMapCameraMove.Instance.DisableMovement();
        UIManager.Instance.optionsMenu.ShowSaveLoading();
        UIManager.Instance.optionsMenu.UpdateSaveMessage("Saving current progress");
        Stopwatch loadingWatch = new Stopwatch();
        loadingWatch.Start();
        currentSaveDataProgress = new SaveDataCurrentProgress();
        currentSaveDataProgress.Initialize();
        currentSaveDataProgress.SaveDate();
        currentSaveDataProgress.SaveWorldSettings();
        currentSaveDataProgress.SavePlayer();
        currentSaveDataProgress.SavePlagueDisease();
        currentSaveDataProgress.SaveWinConditionTracker();
        yield return null;
        yield return StartCoroutine(currentSaveDataProgress.SaveFactionsCoroutine());
        yield return StartCoroutine(currentSaveDataProgress.SaveCharactersCoroutine());
        yield return StartCoroutine(currentSaveDataProgress.SaveJobsCoroutine());
        yield return StartCoroutine(currentSaveDataProgress.SaveReactionQuestsCoroutine());

        //save world map
        WorldMapSave worldMapSave = new WorldMapSave();
        yield return StartCoroutine(worldMapSave.SaveWorldCoroutine(WorldConfigManager.Instance.mapGenerationData.chosenWorldMapTemplate, DatabaseManager.Instance.areaDatabase,
            DatabaseManager.Instance.regionDatabase, DatabaseManager.Instance.settlementDatabase, DatabaseManager.Instance.structureDatabase, WorldEventManager.Instance.activeEvents));
        currentSaveDataProgress.worldMapSave = worldMapSave;
        yield return StartCoroutine(currentSaveDataProgress.SaveTileObjectsCoroutine());
        currentSaveDataProgress.familyTreeDatabase = DatabaseManager.Instance.familyTreeDatabase;

        UIManager.Instance.optionsMenu.UpdateSaveMessage("Finalizing...");
        yield return new WaitForSeconds(0.5f);

        if (string.IsNullOrEmpty(fileName)) {
            //if no file name was provided
            string timeStampStr = $"{currentSaveDataProgress.timeStamp.ToString("yyyy-MM-dd_HHmmss")}";
            fileName = $"{worldMapSave.worldType.ToString()}_{currentSaveDataProgress.continuousDays.ToString()}_{GameManager.Instance.ConvertTickToTime(currentSaveDataProgress.tick, "-")}({timeStampStr})";
        }

        //write to file
        string savePath = $"{UtilityScripts.Utilities.tempZipPath}mainSave.sav";
        filePath = savePath;
        var thread = new Thread(SaveCurrentDataToFile);
        thread.Start();
        while (thread.IsAlive) {
            yield return null;
        }
#if DEBUG_LOG
        Debug.Log($"Saved new game at {savePath}");
#endif
        
        //Need to close connection to database so .db file can be zipped.
        DatabaseManager.Instance.mainSQLDatabase.SaveInMemoryDatabaseToFile($"{UtilityScripts.Utilities.tempZipPath}gameDB.db");
        yield return GameUtilities.waitFor2Seconds; //put buffer in between to ensure database has been fully backed-up.

        //zip files
        string zipPath = $"{UtilityScripts.Utilities.gameSavePath}/{fileName}.zip";
        ZipFile.CreateFromDirectory(UtilityScripts.Utilities.tempZipPath, zipPath);
        yield return GameUtilities.waitFor2Seconds; //put buffer in between to ensure zipping file has finished.
        
        //delete created save file in temp folder since its already been zipped.
        File.Delete(savePath);
        File.Delete($"{UtilityScripts.Utilities.tempZipPath}gameDB.db");
        
        loadingWatch.Stop();
#if DEBUG_LOG
        Debug.Log($"\nTotal saving time is {loadingWatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds");
#endif
        yield return null;
        
        UIManager.Instance.optionsMenu.HideSaveLoading();
        isSaving = false;
        InnerMapCameraMove.Instance.EnableMovement();
        saveCallback?.Invoke();
    }
    private string fileName;

    public string GetFileName() {
        //string timeStampStr = $"{System.DateTime.Now.ToString("yyyy-MM-dd_HHmmss")}";
        string newFileName = $"{WorldSettings.Instance.worldSettingsData.worldType.ToString()}-{GameManager.Instance.continuousDays.ToString()}_{GameManager.Instance.ConvertTickToTime(GameManager.Instance.currentTick, "-")}";
        return newFileName;
    }
    private IEnumerator SaveThisGame(string fileName, Action saveCallback = null) {
        isSaving = true;
        InnerMapCameraMove.Instance.DisableMovement();
        UIManager.Instance.optionsMenu.ShowSaveLoading();
        UIManager.Instance.optionsMenu.UpdateSaveMessage("Saving Progress...");
        Stopwatch loadingWatch = new Stopwatch();
        loadingWatch.Start();
        this.fileName = fileName;
        filePath = $"{UtilityScripts.Utilities.tempZipPath}mainSave.sav";

        objectThreadItems.Clear();
        saveFileWriteFlag = false;
        generalSaveFlag = false;

        currentSaveDataProgress = new SaveDataCurrentProgress();
        currentSaveDataProgress.Initialize();

        while (MultiThreadPool.Instance.IsThereStillFunctionsToBeResolved()) {
            yield return null;
        }
        //Copy sql database here so that the sql database will be copied before writing to disk in case the player unpauses the game when writing to disk, the database might already be altered
        //That is why copy it here before writing to disk
        DatabaseManager.Instance.mainSQLDatabase.SaveInMemoryDatabaseToFile($"{UtilityScripts.Utilities.tempZipPath}gameDB.db");
        yield return null;

        ThreadPool.QueueUserWorkItem(SaveGeneralMultithread);
        foreach (KeyValuePair<TILE_OBJECT_TYPE, List<TileObject>> item in DatabaseManager.Instance.tileObjectDatabase.allTileObjects) {
            //Note: No longer pools SaveTileObjectThreadQueueItem, since they are not processed every time
            //It would be unwise to keep objects that are only rarely used
            SaveTileObjectThreadQueueItem threadItem = new SaveTileObjectThreadQueueItem() { list = item.Value, isDone = false };
            objectThreadItems.Add(threadItem);
            if (item.Key == TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT) {
                ThreadPool.QueueUserWorkItem(SaveGenericTileObjectsMultithread, threadItem);
            } else {
                ThreadPool.QueueUserWorkItem(SaveTileObjectsMultithread, threadItem);
            }
        }
        SaveTileObjectThreadQueueItem destroyedTileObjectsThreadItem = new SaveTileObjectThreadQueueItem() { list = null, isDone = false };
        ThreadPool.QueueUserWorkItem(SaveDestroyedTileObjectsMultithread, destroyedTileObjectsThreadItem);
        while (IsThereStillAProcessingThread()) {
            yield return null;
        }

#if DEBUG_LOG
        Debug.Log($"\nFinal saving time is {loadingWatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds");
#endif

        UIManager.Instance.optionsMenu.HideSaveLoading();
        isSaving = false;
        InnerMapCameraMove.Instance.EnableMovement();
        saveCallback?.Invoke();

        SaveFileWriteToDisk();

        //        UIManager.Instance.optionsMenu.UpdateSaveMessage("Finalizing...");
        //        var fileThread = new Thread(SaveToFileMultithread);
        //        fileThread.Start();
        //        while (fileThread.IsAlive) {
        //            yield return null;
        //        }
        //        loadingWatch.Stop();
        //#if DEBUG_LOG
        //        Debug.Log($"\nFinal saving time is {loadingWatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds");
        //#endif
        //#if DEBUG_LOG
        //        Debug.Log($"Saved new game at {filePath}");
        //#endif

        //        //Need to close connection to database so .db file can be zipped.
        //        DatabaseManager.Instance.mainSQLDatabase.SaveInMemoryDatabaseToFile($"{UtilityScripts.Utilities.tempZipPath}gameDB.db");
        //        yield return null;

        //        //zip files
        //        string zipPath = $"{UtilityScripts.Utilities.gameSavePath}/{this.fileName}.zip";
        //        ZipFile.CreateFromDirectory(UtilityScripts.Utilities.tempZipPath, zipPath);
        //        yield return null;

        //        //delete created save file in temp folder since its already been zipped.
        //        File.Delete(filePath);
        //        File.Delete($"{UtilityScripts.Utilities.tempZipPath}gameDB.db");
        //        yield return null;

    }
    private IEnumerator DoneSaveFileWriteToDiskEnumerator() {
        //Need to close connection to database so .db file can be zipped.
        if (SaveManager.Instance.doNotContinueSaving) {
            if (File.Exists(filePath)) {
                File.Delete(filePath);
            }
            if (File.Exists($"{UtilityScripts.Utilities.tempZipPath}gameDB.db")) {
                File.Delete($"{UtilityScripts.Utilities.tempZipPath}gameDB.db");
            }
            SetIsWritingToDisk(false);
            yield return null;
        } else {
            //zip files
            string zipPath = $"{UtilityScripts.Utilities.gameSavePath}/{this.fileName}.zip";
            ZipFile.CreateFromDirectory(UtilityScripts.Utilities.tempZipPath, zipPath);
            yield return null;

            //delete created save file in temp folder since its already been zipped.
            File.Delete(filePath);
            File.Delete($"{UtilityScripts.Utilities.tempZipPath}gameDB.db");
            yield return null;

            SetIsWritingToDisk(false);
        }
        SaveManager.Instance.SetDoNotContinueSaving(false);
    }
    private void SaveFileWriteToDisk() {
        SetIsWritingToDisk(true);
        ThreadPool.QueueUserWorkItem(SaveToFileMultithread);
    }
    private void DoneSaveFileWriteToDisk() {
        saveFileWriteFlag = false;
        StartCoroutine(DoneSaveFileWriteToDiskEnumerator());
    }
    private void SetIsWritingToDisk(bool p_state) {
        isWritingToDisk = p_state;
        if (UIManager.Instance != null) {
            if (isWritingToDisk) {
                UIManager.Instance.ShowSaveWritingToDisk();
            } else {
                UIManager.Instance.HideSaveWritingToDisk();
            }
            UIManager.Instance.optionsMenu.UpdateSaveBtnState();
        }
    }
    private bool IsThereStillAProcessingThread() {
        if (!generalSaveFlag) {
            return true;
        }
        for (int i = 0; i < objectThreadItems.Count; i++) {
            if (!objectThreadItems[i].isDone) {
                return true;
            }
        }
        return false;
    }
    private void SaveGeneralMultithread(object state) {
        try {
            lock (THREAD_LOCKER) {
                currentSaveDataProgress.SaveDate();
                currentSaveDataProgress.SaveWorldSettings();
                currentSaveDataProgress.SavePlayer();
                currentSaveDataProgress.SavePlagueDisease();
                currentSaveDataProgress.SaveWinConditionTracker();

                currentSaveDataProgress.SaveFactions();
                currentSaveDataProgress.SaveCharacters();
                currentSaveDataProgress.SaveJobs();
                currentSaveDataProgress.SaveReactionQuests();

                currentSaveDataProgress.familyTreeDatabase = DatabaseManager.Instance.familyTreeDatabase;

                SaveWorldMultithread();
                generalSaveFlag = true;
            }
        } catch (Exception e) {
            Debug.LogError(e.StackTrace + "\n" + e.Message);
        }
    }
    private void SaveTileObjectsMultithread(object state) {
        try {
            SaveTileObjectThreadQueueItem threadItem = state as SaveTileObjectThreadQueueItem;
            List<TileObject> tileObjectList = threadItem.list;
            currentSaveDataProgress.SaveTileObjects(tileObjectList);
            threadItem.isDone = true;
        } catch (Exception e) {
            Debug.LogError(e.StackTrace + "\n" + e.Message);
        }
    }
    private void SaveGenericTileObjectsMultithread(object state) {
        try {
            SaveTileObjectThreadQueueItem threadItem = state as SaveTileObjectThreadQueueItem;
            List<TileObject> tileObjectList = threadItem.list;
            currentSaveDataProgress.SaveGenericTileObjects(tileObjectList);
            threadItem.isDone = true;
        } catch (Exception e) {
            Debug.LogError(e.StackTrace + "\n" + e.Message);
        }
    }
    private void SaveDestroyedTileObjectsMultithread(object state) {
        try {
            SaveTileObjectThreadQueueItem threadItem = state as SaveTileObjectThreadQueueItem;
            currentSaveDataProgress.SaveDestroyedTileObjects();
            threadItem.isDone = true;
        } catch (Exception e) {
            Debug.LogError(e.StackTrace + "\n" + e.Message);
        }
    }
    private void SaveWorldMultithread() {
        //save world map
        WorldMapSave worldMapSave = new WorldMapSave();
        worldMapSave.SaveWorld(WorldConfigManager.Instance.mapGenerationData.chosenWorldMapTemplate, DatabaseManager.Instance.areaDatabase,
            DatabaseManager.Instance.regionDatabase, DatabaseManager.Instance.settlementDatabase, DatabaseManager.Instance.structureDatabase, WorldEventManager.Instance.activeEvents);
        currentSaveDataProgress.worldMapSave = worldMapSave;
    }
    private void SaveToFileMultithread(object state) {
        if (string.IsNullOrEmpty(fileName)) {
            //if no file name was provided
            string timeStampStr = $"{currentSaveDataProgress.timeStamp.ToString("yyyy-MM-dd_HHmmss")}";
            fileName = $"{currentSaveDataProgress.worldMapSave.worldType}_{currentSaveDataProgress.continuousDays}_{GameManager.Instance.ConvertTickToTime(currentSaveDataProgress.tick, "-")}({timeStampStr})";
        }

        //write to file
        SaveCurrentDataToFile();
        saveFileWriteFlag = true;
    }

    private void SaveCurrentDataToFile() {
        SaveGame.Save(filePath, currentSaveDataProgress);
    }
#endregion

#region Loading
    public void SetCurrentSaveDataPath(string path) {
        currentSaveDataPath = path;
    }
    public IEnumerator LoadSaveDataCurrentProgressBasedOnSetPath() {
        //extract files from currentSaveDataPath zip to temp folder
        ZipFile.ExtractToDirectory(currentSaveDataPath, UtilityScripts.Utilities.tempPath);
        string savePath = $"{UtilityScripts.Utilities.tempPath}mainSave.sav";
        var thread = new Thread(() => LoadDataFromPath(savePath));
        thread.Start();
        while (thread.IsAlive) {
            yield return null;
        }
    }
    private void LoadDataFromPath(string path) {
        currentSaveDataProgress = GetSaveFileData(path);
    }
    private SaveDataCurrentProgress GetSaveFileData(string path) {
        return SaveGame.Load<SaveDataCurrentProgress>(path);
    }
    public bool HasAnySaveFiles() {
        string[] saveFiles = Directory.GetFiles(UtilityScripts.Utilities.gameSavePath, "*.zip");
        return saveFiles.Length > 0;
    }
    public string GetLatestSaveFile() {
        string[] saveFiles = Directory.GetFiles(UtilityScripts.Utilities.gameSavePath, "*.zip");
        string latestFile = string.Empty;
        for (int i = 0; i < saveFiles.Length; i++) {
            string saveFile = saveFiles[i];
            if (string.IsNullOrEmpty(latestFile)) {
                latestFile = saveFile;
            } else {
                //compare times
                DateTime writeTimeOfCurrentSave = File.GetLastWriteTime(saveFile);
                DateTime writeTimeOfLatestSave = File.GetLastWriteTime(latestFile);
                if (writeTimeOfCurrentSave > writeTimeOfLatestSave) {
                    latestFile = saveFile;
                }
            }
        }
        return latestFile;
    }
    public void CleanUpLoadedData() {
        currentSaveDataProgress?.CleanUp();
        currentSaveDataProgress = null;
    }
#endregion

    // #region JSON Net
    // public void SaveData<T>(string identifier, T obj) {
    //     if (string.IsNullOrEmpty(identifier)) {
    //         throw new System.ArgumentNullException("identifier");
    //     }
    //     string filePath = "";
    //     if (IsFilePath(identifier)) {
    //         filePath = identifier;
    //     } else {
    //         throw new System.Exception("identifier is not a file path!");
    //     }
    //     if (obj == null) {
    //         throw new System.Exception("Object to be saved is null!");
    //     }
    //     Directory.CreateDirectory(Path.GetDirectoryName(filePath));
    //
    //     var stream = new MemoryStream();
    //     // MsgPack.Serialize(obj, stream);
    //
    //     File.WriteAllBytes(filePath, stream.ToArray());
    //
    // }
    // public T LoadData<T>(string identifier) {
    //     if (string.IsNullOrEmpty(identifier)) {
    //         throw new System.ArgumentNullException("identifier");
    //     }
    //     string filePath = "";
    //     if (IsFilePath(identifier)) {
    //         filePath = identifier;
    //     } else {
    //         throw new System.Exception("identifier is not a file path!");
    //     }
    //     MemoryStream data = new MemoryStream(File.ReadAllBytes(filePath));
    //
    //     //Stream stream;
    //     // T convertedObj = MsgPack.Deserialize<T>(data);
    //
    //     return convertedObj;
    // }
    // public bool IsFilePath(string str) {
    //     bool result = false;
    //     if (Path.IsPathRooted(str)) {
    //         try {
    //             Path.GetFullPath(str);
    //             result = true;
    //         } catch (System.Exception) {
    //             result = false;
    //         }
    //     }
    //     return result;
    // }
    // #endregion

}

public class SaveTileObjectThreadQueueItem {
    public List<TileObject> list;
    public bool isDone;

    public void Reset() {
        list = null;
        isDone = false;
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using UnityEngine;
using BayatGames.SaveGameFree;
using Newtonsoft.Json;
using Tutorial;
using Debug = UnityEngine.Debug;

public class SaveCurrentProgressManager : MonoBehaviour {
    public const string savedCurrentProgressFileName = "SAVED_CURRENT_PROGRESS";
    public SaveDataCurrentProgress currentSaveDataProgress { get; private set; }

    public string currentSaveDataPath { get; private set; }
    
    #region getters
    public bool hasSavedDataCurrentProgress => currentSaveDataProgress != null;
    #endregion

    #region Saving
    public void AddToSaveHub<T>(T data) where T : ISavable {
        currentSaveDataProgress.AddToSaveHub(data);
    }
    public bool CanSaveCurrentProgress() {
        return !PlayerManager.Instance.player.seizeComponent.hasSeizedPOI && (WorldSettings.Instance.worldSettingsData.worldType != WorldSettingsData.World_Type.Tutorial || TutorialManager.Instance.hasCompletedImportantTutorials);
    }
    public void SaveCurrentProgress() {
        currentSaveDataProgress = new SaveDataCurrentProgress();
        currentSaveDataProgress.Initialize();
        currentSaveDataProgress.SaveDate();
        currentSaveDataProgress.SavePlayer();
        currentSaveDataProgress.SaveFactions();
        currentSaveDataProgress.SaveCharacters();
        currentSaveDataProgress.SaveJobs();

        //save world map
        WorldMapSave worldMapSave = new WorldMapSave();
        worldMapSave.SaveWorld(
            WorldConfigManager.Instance.mapGenerationData.chosenWorldMapTemplate,
            DatabaseManager.Instance.hexTileDatabase, 
            DatabaseManager.Instance.regionDatabase, 
            DatabaseManager.Instance.settlementDatabase,
            DatabaseManager.Instance.structureDatabase
        );
        currentSaveDataProgress.worldMapSave = worldMapSave;
        currentSaveDataProgress.SaveTileObjects(DatabaseManager.Instance.tileObjectDatabase.allTileObjectsList);
        currentSaveDataProgress.familyTreeDatabase = DatabaseManager.Instance.familyTreeDatabase;
        SaveGame.Save($"{UtilityScripts.Utilities.gameSavePath}{savedCurrentProgressFileName}.sav", currentSaveDataProgress);
    }
    public void DoManualSave(string fileName = "") {
        Stopwatch loadingWatch = new Stopwatch();
        loadingWatch.Start();
        currentSaveDataProgress = new SaveDataCurrentProgress();
        currentSaveDataProgress.Initialize();
        //date
        currentSaveDataProgress.SaveDate();
        currentSaveDataProgress.SavePlayer();
        currentSaveDataProgress.SaveFactions();
        currentSaveDataProgress.SaveCharacters();
        currentSaveDataProgress.SaveJobs();
        
        //save world map
        WorldMapSave worldMapSave = new WorldMapSave();
        worldMapSave.SaveWorld(
            WorldConfigManager.Instance.mapGenerationData.chosenWorldMapTemplate, 
            DatabaseManager.Instance.hexTileDatabase,
            DatabaseManager.Instance.regionDatabase,
            DatabaseManager.Instance.settlementDatabase,
            DatabaseManager.Instance.structureDatabase
        );
        currentSaveDataProgress.worldMapSave = worldMapSave;
        currentSaveDataProgress.SaveTileObjects(DatabaseManager.Instance.tileObjectDatabase.allTileObjectsList);
        currentSaveDataProgress.familyTreeDatabase = DatabaseManager.Instance.familyTreeDatabase;

        
        if (string.IsNullOrEmpty(fileName)) {
            // fileName = savedCurrentProgressFileName;
            string timeStampStr = $"{currentSaveDataProgress.timeStamp.ToString("yyyy-MM-dd_HHmm")}";
            fileName = $"{timeStampStr}_{worldMapSave.worldType.ToString()}_Day{currentSaveDataProgress.day.ToString()}";
        }
        
        string path = $"{UtilityScripts.Utilities.gameSavePath}{fileName}.sav";
        
        // Directory.CreateDirectory(Path.GetDirectoryName(UtilityScripts.Utilities.gameSavePath));
        // string json = JsonConvert.SerializeObject(currentSaveDataProgress, Formatting.Indented);
        // File.WriteAllText(path, json);
        SaveGame.Save(path, currentSaveDataProgress);
        
        Debug.Log($"Saved new game at {path}");
        loadingWatch.Stop();
        Debug.Log($"\nTotal saving time is {loadingWatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds");
        loadingWatch = null;
    }
    #endregion

    #region Loading
    public void SetCurrentSaveDataPath(string path) {
        currentSaveDataPath = path;
    }
    public void LoadSaveDataCurrentProgressBasedOnSetPath() {
        currentSaveDataProgress = GetSaveFileData(currentSaveDataPath);
    }
    public SaveDataCurrentProgress LoadSaveDataCurrentProgress(string path) {
        return GetSaveFileData(path);
    }
    private SaveDataCurrentProgress GetSaveFileData(string path) {
        return SaveGame.Load<SaveDataCurrentProgress>(path);
    }
    #endregion

}

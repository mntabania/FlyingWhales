using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BayatGames.SaveGameFree;

public class SaveCurrentProgressManager : MonoBehaviour {
    public const string savedCurrentProgressFileName = "SAVED_CURRENT_PROGRESS";
    public SaveDataCurrentProgress currentSaveDataProgress { get; private set; }

    #region getters
    public bool hasSavedDataCurrentProgress => currentSaveDataProgress != null;
    #endregion

    #region Saving
    public void AddToSaveHub<T>(T data) where T : ISavable {
        currentSaveDataProgress.AddToSaveHub(data);
    }
    public bool CanSaveCurrentProgress() {
        return !PlayerManager.Instance.player.seizeComponent.hasSeizedPOI;
    }
    public void SaveCurrentProgress() {
        currentSaveDataProgress = new SaveDataCurrentProgress();
        currentSaveDataProgress.Initialize();
        currentSaveDataProgress.SaveDate();
        currentSaveDataProgress.SavePlayer();
        currentSaveDataProgress.SaveFactions();
        
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

        //        Save save = new Save((int)GridMap.Instance.width, (int)GridMap.Instance.height, GridMap.Instance._borderThickness);
        //        save.SaveHextiles(GridMap.Instance.normalHexTiles);
        //        // save.SaveOuterHextiles(GridMap.Instance.outerGridList);
        //        save.SaveRegions(GridMap.Instance.allRegions);
        //        // save.SavePlayerArea(PlayerManager.Instance.player.playerSettlement);
        //        save.SaveNonPlayerAreas();
        //        save.SaveFactions(FactionManager.Instance.allFactions);
        //        save.SaveCharacters(CharacterManager.Instance.allCharacters);
        //        save.SavePlayer(PlayerManager.Instance.player);
        //        save.SaveTileObjects(InnerMapManager.Instance.allTileObjects);
        //        // save.SaveSpecialObjects(TokenManager.Instance.specialObjects);
        ////        save.SaveAreaMaps(InnerMapManager.Instance.innerMaps); TODO: Saving for new generic inner map
        //        save.SaveCurrentDate();
        //        save.SaveNotifications();

        //SaveGame.Encode = true;
        SaveGame.Save($"{UtilityScripts.Utilities.gameSavePath}{savedCurrentProgressFileName}.sav", currentSaveDataProgress);
    }
    public void DoManualSave(string fileName = "") {
        if (string.IsNullOrEmpty(fileName)) {
            fileName = savedCurrentProgressFileName;
        }
        currentSaveDataProgress = new SaveDataCurrentProgress();
        currentSaveDataProgress.Initialize();
        //date
        currentSaveDataProgress.SaveDate();
        currentSaveDataProgress.SavePlayer();
        currentSaveDataProgress.SaveFactions();

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

        string path = $"{UtilityScripts.Utilities.gameSavePath}{fileName}.sav";
        SaveGame.Save(path, currentSaveDataProgress);
        
        Debug.Log($"Saved new game at {path}");
    }
    #endregion

    #region Loading
    public void LoadSaveDataCurrentProgress() {
        currentSaveDataProgress = GetSaveFileData($"{UtilityScripts.Utilities.gameSavePath}{savedCurrentProgressFileName}.sav");
    }
    private SaveDataCurrentProgress GetSaveFileData(string path) {
        return SaveGame.Load<SaveDataCurrentProgress>(path);
    }
    #endregion

}

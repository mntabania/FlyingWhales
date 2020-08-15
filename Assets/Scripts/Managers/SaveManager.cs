using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BayatGames.SaveGameFree;
using Inner_Maps;
using Locations.Tile_Features;
using Scenario_Maps;
using Traits;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SaveManager : MonoBehaviour {
    public static SaveManager Instance;
    private const string savedPlayerDataFileName = "SAVED_PLAYER_DATA";
    private const string savedCurrentProgressFileName = "SAVED_CURRENT_PROGRESS";
    public bool useSaveData;

    public SaveDataPlayer currentSaveDataPlayer { get; private set; }
    public SaveDataCurrentProgress currentSaveDataProgress { get; private set; }

    [Header("For Testing")] 
    [SerializeField] private bool alwaysResetSpecialPopupsOnStartup;
    [SerializeField] private bool alwaysResetBonusTutorialsOnStartup;

    public bool hasSavedDataCurrentProgress => currentSaveDataProgress != null;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
#if UNITY_EDITOR
            EditorApplication.quitting += OnEditorQuit;
#endif
        } else {
            Destroy(this.gameObject);
        }
    }
    private void OnApplicationQuit() {
        SavePlayerData();
    }
    private void OnEditorQuit() {
        SavePlayerData();
    }

    #region General
    public void SetCurrentSaveDataPlayer(SaveDataPlayer save) {
        currentSaveDataPlayer = save;
    }
    public static SaveDataTrait ConvertTraitToSaveDataTrait(Trait trait) {
        if (trait.isNotSavable) {
            return null;
        }
        SaveDataTrait saveDataTrait = null;
        System.Type type = System.Type.GetType($"SaveData{trait.name}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
        if (type != null) {
            saveDataTrait = System.Activator.CreateInstance(type) as SaveDataTrait;
        } else {
            saveDataTrait = new SaveDataTrait();
        }
        return saveDataTrait;
    }
    #endregion

    #region Saving
    public bool CanSaveCurrentProgress() {
        return !PlayerManager.Instance.player.seizeComponent.hasSeizedPOI;
    }
    public void SaveCurrentProgress() {
        SaveDataCurrentProgress saveData = new SaveDataCurrentProgress();
        saveData.SaveDate();
        saveData.SavePlayer();
        saveData.SaveFactions();
        
        //save world map
        WorldMapSave worldMapSave = new WorldMapSave();
        worldMapSave.SaveWorld(
            WorldConfigManager.Instance.mapGenerationData.chosenWorldMapTemplate, 
            GridMap.Instance.normalHexTiles
        );
        saveData.worldMapSave = worldMapSave;
        
        saveData.SaveSettlements(LandmarkManager.Instance.allSettlements);
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

        SaveGame.Encode = true;
        SaveGame.Save($"{UtilityScripts.Utilities.gameSavePath}{savedCurrentProgressFileName}.sav", saveData);
    }
    public void SavePlayerData() {
        SaveDataPlayer save = currentSaveDataPlayer;
        SaveGame.Save(UtilityScripts.Utilities.gameSavePath + savedPlayerDataFileName, save);
    }
    public void DoManualSave(string fileName = "") {
        SaveDataCurrentProgress completeSave = new SaveDataCurrentProgress();
        
        //date
        completeSave.SaveDate();
        
        //save world map
        WorldMapSave worldMapSave = new WorldMapSave();
        worldMapSave.SaveWorld(
            WorldConfigManager.Instance.mapGenerationData.chosenWorldMapTemplate, 
            GridMap.Instance.normalHexTiles
        );
        completeSave.worldMapSave = worldMapSave;
        
        completeSave.SaveSettlements(LandmarkManager.Instance.allSettlements);

        if (string.IsNullOrEmpty(fileName)) {
            fileName = savedCurrentProgressFileName;
        }
        string path = $"{UtilityScripts.Utilities.gameSavePath}{fileName}.sav";
        SaveGame.Save(path, completeSave);
        
        Debug.Log($"Saved new game at {path}");
    }
    public void SaveScenario(string fileName = "") {
        ScenarioMapData scenarioSave = new ScenarioMapData();

        //save world map
        WorldMapSave worldMapSave = new WorldMapSave();
        worldMapSave.SaveWorld(
            WorldConfigManager.Instance.mapGenerationData.chosenWorldMapTemplate, 
            GridMap.Instance.normalHexTiles
        );
        scenarioSave.worldMapSave = worldMapSave;

        if (string.IsNullOrEmpty(fileName)) {
            fileName = savedCurrentProgressFileName;
        }
        string path = $"{Application.streamingAssetsPath}/Scenario Maps/{fileName}.sce";
        SaveGame.Save(path, scenarioSave);
        
        Debug.Log($"Saved new scenario at {path}");
    }
    #endregion

    #region Loading
    public void LoadSaveDataPlayer() {
        //if(UtilityScripts.Utilities.DoesFileExist(UtilityScripts.Utilities.gameSavePath + saveFileName)) {
        //    SetCurrentSave(SaveGame.Load<Save>(UtilityScripts.Utilities.gameSavePath + saveFileName));
        //}
        if (WorldConfigManager.Instance.isTutorialWorld) {
            currentSaveDataPlayer = new SaveDataPlayer();
            currentSaveDataPlayer.InitializeInitialData();
        } else {
            if (UtilityScripts.Utilities.DoesFileExist(UtilityScripts.Utilities.gameSavePath + savedPlayerDataFileName)) {
                SetCurrentSaveDataPlayer(SaveGame.Load<SaveDataPlayer>(UtilityScripts.Utilities.gameSavePath + savedPlayerDataFileName));
            }
            if (currentSaveDataPlayer == null) {
                currentSaveDataPlayer = new SaveDataPlayer();
                currentSaveDataPlayer.InitializeInitialData();
            }
        }
        if (alwaysResetBonusTutorialsOnStartup) {
            currentSaveDataPlayer.ResetBonusTutorialProgress();
        }
        if (alwaysResetSpecialPopupsOnStartup) {
            currentSaveDataPlayer.ResetSpecialPopupsProgress();
        }
    }
    public void LoadSaveDataCurrentProgress() {
        currentSaveDataProgress = GetSaveFileData($"{UtilityScripts.Utilities.gameSavePath}/SAVED_CURRENT_PROGRESS.sav");
    }
    private SaveDataCurrentProgress GetSaveFileData(string path) {
        return SaveGame.Load<SaveDataCurrentProgress>(path);
    }
    #endregion

    #region Tile Features
    public static SaveDataTileFeature ConvertTileFeatureToSaveData(TileFeature tileFeature) {
        SaveDataTileFeature saveDataTrait = null;
        System.Type type = System.Type.GetType($"Locations.Tile_Features.SaveData{tileFeature.GetType().Name}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
        if (type != null) {
            saveDataTrait = System.Activator.CreateInstance(type) as SaveDataTileFeature;
        } else {
            saveDataTrait = new SaveDataTileFeature();
        }
        return saveDataTrait;
    }
    #endregion

    #region Scenario Maps
    public ScenarioMapData GetScenarioMapData(string path) {
        return SaveGame.Load<ScenarioMapData>(path);
    }
    #endregion
}

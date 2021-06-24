using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using BayatGames.SaveGameFree;
using Inner_Maps;
using Locations.Area_Features;
using Scenario_Maps;
using Traits;
using UtilityScripts;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SaveManager : MonoBehaviour {
    public static SaveManager Instance;
    public SavePlayerManager savePlayerManager;
    public SaveCurrentProgressManager saveCurrentProgressManager;

    public bool useSaveData { get; private set; }
    public bool doNotContinueSaving { get; private set; }

    [Header("For Testing")] 
    [SerializeField] private bool alwaysResetSpecialPopupsOnStartup;
    [SerializeField] private bool alwaysResetBonusTutorialsOnStartup;
    [SerializeField] private bool _unlockAllWorlds;
    
    #region Saving Batches
    public const int HexTile_Save_Batches = 200;
    public const int Region_Save_Batches = 200;
    public const int Job_Save_Batches = 200;
    public const int Character_Save_Batches = 200;
    public const int TileObject_Save_Batches = 200;
    public const int Settlement_Save_Batches = 200;
    public const int Structure_Save_Batches = 200;
    public const int Reaction_Quest_Save_Batches = 200;
    #endregion

    #region getters
    public SaveDataPlayer currentSaveDataPlayer => savePlayerManager.currentSaveDataPlayer;
    public SaveDataCurrentProgress currentSaveDataProgress => saveCurrentProgressManager.currentSaveDataProgress;
// #if UNITY_EDITOR || DEVELOPMENT_BUILD
    public bool unlockAllWorlds => _unlockAllWorlds;
// #else
//     public bool unlockAllWorlds => false;
// #endif
    #endregion

    #region Monobehaviours
    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
#if UNITY_EDITOR
            EditorApplication.quitting += OnEditorQuit;
#endif
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            //Should create folder of save path if no folder exists
            if (!Directory.Exists(UtilityScripts.Utilities.gameSavePath)) {
                Directory.CreateDirectory(UtilityScripts.Utilities.gameSavePath);
            }
        } else {
            Destroy(this.gameObject);
        }
    }
    private void OnApplicationQuit() {
        savePlayerManager.SavePlayerData();
    }
    private void OnEditorQuit() {
        savePlayerManager.SavePlayerData();
    }
    private void OnSceneUnloaded(Scene unloaded) {
        if (unloaded.name == "Game") {
            if (saveCurrentProgressManager.isSaving || saveCurrentProgressManager.isWritingToDisk) {
                SetDoNotContinueSaving(true);
            }
        }
    }
    public void SetDoNotContinueSaving(bool p_state) {
        doNotContinueSaving = p_state;
    }
    #endregion

    #region Initialization
    public void PrepareTempDirectory() {
        if (saveCurrentProgressManager.isSaving || saveCurrentProgressManager.isWritingToDisk) {
            return;
        }
        if (Directory.Exists(UtilityScripts.Utilities.tempPath)) {
            Directory.Delete(UtilityScripts.Utilities.tempPath, true);
        }
        Directory.CreateDirectory(UtilityScripts.Utilities.tempPath);
        Directory.CreateDirectory(UtilityScripts.Utilities.tempZipPath);
    }
    public void DeleteSaveFilesInTempDirectory() {
        string[] saveFiles = Directory.GetFiles(UtilityScripts.Utilities.tempPath, "*.sav");
        for (int i = 0; i < saveFiles.Length; i++) {
            string file = saveFiles[i];
            File.Delete(file);
        }
    }
    #endregion
    
    #region Saving
    public void SetUseSaveData(bool state) {
        useSaveData = state;
    }
    public void SaveScenario(string fileName = "") {
        ScenarioMapData scenarioSave = new ScenarioMapData();

        //save world map
        ScenarioWorldMapSave worldMapSave = new ScenarioWorldMapSave();
        worldMapSave.SaveWorld(
            WorldConfigManager.Instance.mapGenerationData.chosenWorldMapTemplate, 
            GridMap.Instance.allAreas,
            GridMap.Instance.mainRegion.innerMap.elevationPerlinSettings,
            GridMap.Instance.mainRegion.innerMap.warpWeight,
            GridMap.Instance.mainRegion.innerMap.temperatureSeed,
            GridMap.Instance.mainRegion.villageSpots
        );
        scenarioSave.worldMapSave = worldMapSave;

        scenarioSave.SaveVillageSettlements(LandmarkManager.Instance.allNonPlayerSettlements.Where(x => x.locationType == LOCATION_TYPE.VILLAGE).ToList());
        
        if (string.IsNullOrEmpty(fileName)) {
            fileName = SaveCurrentProgressManager.savedCurrentProgressFileName;
        }
        string path = $"{Application.streamingAssetsPath}/Scenario Maps/{fileName}.json";
        SaveGame.Save(path, scenarioSave);

#if DEBUG_LOG
        Debug.Log($"Saved new scenario at {path}");
#endif
    }
#endregion

#region Loading
    public void LoadSaveDataPlayer() {
        savePlayerManager.LoadSaveDataPlayer();
        if (alwaysResetBonusTutorialsOnStartup) {
            savePlayerManager.currentSaveDataPlayer?.ResetBonusTutorialProgress();
        }
        if (alwaysResetSpecialPopupsOnStartup) {
            savePlayerManager.currentSaveDataPlayer?.ResetSpecialPopupsProgress();
        }
    }
#endregion

#region Tile Features
    public static SaveDataAreaFeature ConvertAreaFeatureToSaveData(AreaFeature p_areaFeature) {
        SaveDataAreaFeature saveDataTrait = null;
        System.Type type = p_areaFeature.serializedData; //System.Type.GetType($"Locations.Tile_Features.SaveData{p_areaFeature.GetType().Name}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
        if (type != null) {
            saveDataTrait = System.Activator.CreateInstance(type) as SaveDataAreaFeature;
        } else {
            saveDataTrait = new SaveDataAreaFeature();
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

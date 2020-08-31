using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public bool useSaveData;
    public SavePlayerManager savePlayerManager;
    public SaveCurrentProgressManager saveCurrentProgressManager;


    [Header("For Testing")] 
    [SerializeField] private bool alwaysResetSpecialPopupsOnStartup;
    [SerializeField] private bool alwaysResetBonusTutorialsOnStartup;
    [SerializeField] private bool _unlockAllWorlds;


    #region getters
    public SaveDataPlayer currentSaveDataPlayer => savePlayerManager.currentSaveDataPlayer;
    public SaveDataCurrentProgress currentSaveDataProgress => saveCurrentProgressManager.currentSaveDataProgress;
    #endregion


#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public bool unlockAllWorlds => _unlockAllWorlds;
#else
    public bool unlockAllWorlds => false;
#endif
    
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
        savePlayerManager.SavePlayerData();
    }
    private void OnEditorQuit() {
        savePlayerManager.SavePlayerData();
    }

    #region Saving
    public void SaveScenario(string fileName = "") {
        ScenarioMapData scenarioSave = new ScenarioMapData();

        //save world map
        ScenarioWorldMapSave worldMapSave = new ScenarioWorldMapSave();
        worldMapSave.SaveWorld(
            WorldConfigManager.Instance.mapGenerationData.chosenWorldMapTemplate, 
            GridMap.Instance.normalHexTiles
        );
        scenarioSave.worldMapSave = worldMapSave;

        scenarioSave.SaveVillageSettlements(LandmarkManager.Instance.allNonPlayerSettlements.Where(x => x.locationType == LOCATION_TYPE.SETTLEMENT).ToList());
        
        if (string.IsNullOrEmpty(fileName)) {
            fileName = SaveCurrentProgressManager.savedCurrentProgressFileName;
        }
        string path = $"{Application.streamingAssetsPath}/Scenario Maps/{fileName}.sce";
        SaveGame.Save(path, scenarioSave);
        
        Debug.Log($"Saved new scenario at {path}");
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

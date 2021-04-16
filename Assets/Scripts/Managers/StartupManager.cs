using System;
using UnityEngine;
using System.Collections;
using Scenario_Maps;
using UnityEngine.Assertions;

public class StartupManager : MonoBehaviour {
	public MapGenerator mapGenerator;
    public Initializer initializer;

    void Start(){
        Messenger.AddListener(Signals.GAME_LOADED, OnGameLoaded);
        Messenger.AddListener(UISignals.START_GAME_AFTER_LOADOUT_SELECT, OnLoadoutSelected);
        StartCoroutine(PerformStartup());
    }

    private IEnumerator PerformStartup() {
        LevelLoaderManager.Instance.SetLoadingState(true);
        LevelLoaderManager.Instance.UpdateLoadingInfo("Initializing Data...");
        yield return StartCoroutine(initializer.InitializeDataBeforeWorldCreation());

        if (!string.IsNullOrEmpty(SaveManager.Instance.saveCurrentProgressManager.currentSaveDataPath)) {
            LevelLoaderManager.Instance.UpdateLoadingInfo("Reading Save File...");
            LevelLoaderManager.Instance.UpdateLoadingBar(0.4f, 8f);
            yield return StartCoroutine(SaveManager.Instance.saveCurrentProgressManager.LoadSaveDataCurrentProgressBasedOnSetPath());
            yield return StartCoroutine(mapGenerator.InitializeSavedWorld(SaveManager.Instance.saveCurrentProgressManager.currentSaveDataProgress));
            //clear out save file in temp folder
            SaveManager.Instance.DeleteSaveFilesInTempDirectory();
        } else {
            LevelLoaderManager.Instance.UpdateLoadingInfo("Initializing World...");
            if (WorldSettings.Instance.worldSettingsData.IsScenarioMap()) {
                ScenarioData scenarioData = WorldSettings.Instance.GetScenarioDataByWorldType(WorldSettings.Instance.worldSettingsData.worldType);
                ScenarioMapData scenarioMapData = null;
                if(scenarioData != null) {
                    scenarioMapData = UtilityScripts.Utilities.Deserialize<ScenarioMapData>(scenarioData.scenarioSettings.text);
                } else {
                    throw new Exception($"There is no scenario map data for {WorldSettings.Instance.worldSettingsData.worldType.ToString()}");
                }
                //switch (WorldSettings.Instance.worldSettingsData.worldType) {
                //    case WorldSettingsData.World_Type.Tutorial:
                //        scenarioMapData = SaveManager.Instance.GetScenarioMapData($"{Application.streamingAssetsPath}/Scenario Maps/Tutorial.sce");
                //        break;
                //    case WorldSettingsData.World_Type.Oona:
                //        scenarioMapData = SaveManager.Instance.GetScenarioMapData($"{Application.streamingAssetsPath}/Scenario Maps/Oona.sce");
                //        break;
                //    case WorldSettingsData.World_Type.Pangat_Loo:
                //        scenarioMapData = SaveManager.Instance.GetScenarioMapData($"{Application.streamingAssetsPath}/Scenario Maps/Pangat_Loo.sce");
                //        break;
                //    case WorldSettingsData.World_Type.Zenko:
                //        scenarioMapData = SaveManager.Instance.GetScenarioMapData($"{Application.streamingAssetsPath}/Scenario Maps/Zenko.sce");
                //        break;
                //    case WorldSettingsData.World_Type.Affatt:
                //        scenarioMapData = SaveManager.Instance.GetScenarioMapData($"{Application.streamingAssetsPath}/Scenario Maps/Affatt.sce");
                //        break;
                //    case WorldSettingsData.World_Type.Icalawa:
                //        scenarioMapData = SaveManager.Instance.GetScenarioMapData($"{Application.streamingAssetsPath}/Scenario Maps/Icalawa.sce");
                //        break;
                //    default:
                //        throw new Exception($"There is no scenario map data for {WorldSettings.Instance.worldSettingsData.worldType.ToString()}");
                //}
                
                if (scenarioMapData != null && !WorldConfigManager.Instance.useRandomGenerationForScenarioMaps) {
                    yield return StartCoroutine(mapGenerator.InitializeScenarioWorld(scenarioMapData));    
                } else {
                    Debug.Log("Generating random world...");
                    yield return StartCoroutine(mapGenerator.InitializeWorld());    
                }
            } else {
                Debug.Log("Generating random world...");
                yield return StartCoroutine(mapGenerator.InitializeWorld());
            }
        }
    }

    private void OnGameLoaded() {
        Messenger.RemoveListener(Signals.GAME_LOADED, OnGameLoaded);
        initializer.InitializeDataAfterWorldCreation();
    }
    private void OnLoadoutSelected() {
        Messenger.RemoveListener(UISignals.START_GAME_AFTER_LOADOUT_SELECT, OnLoadoutSelected);
        initializer.InitializeDataAfterLoadoutSelection();
    }
}

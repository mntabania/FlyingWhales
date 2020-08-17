using System;
using UnityEngine;
using System.Collections;
using Scenario_Maps;

public class StartupManager : MonoBehaviour {
	public MapGenerator mapGenerator;
    public Initializer initializer;

    void Awake() {
        Messenger.Cleanup();
    }
    void Start(){
        Messenger.AddListener(Signals.GAME_LOADED, OnGameLoaded);
        Messenger.AddListener(Signals.START_GAME_AFTER_LOADOUT_SELECT, OnLoadoutSelected);
        StartCoroutine(PerformStartup());
    }

    private IEnumerator PerformStartup() {
        LevelLoaderManager.Instance.SetLoadingState(true);
        LevelLoaderManager.Instance.UpdateLoadingInfo("Initializing Data...");
        yield return StartCoroutine(initializer.InitializeDataBeforeWorldCreation());

        LevelLoaderManager.Instance.UpdateLoadingInfo("Initializing World...");

        if (SaveManager.Instance.useSaveData) {
            SaveDataCurrentProgress saveData = SaveManager.Instance.currentSaveDataProgress;
            yield return StartCoroutine(mapGenerator.InitializeSavedWorld(saveData));
        } else {
            if (WorldSettings.Instance.worldSettingsData.IsScenarioMap()) {
                ScenarioMapData scenarioMapData = null;
                switch (WorldSettings.Instance.worldSettingsData.worldType) {
                    case WorldSettingsData.World_Type.Tutorial:
                        scenarioMapData = SaveManager.Instance.GetScenarioMapData($"{Application.streamingAssetsPath}/Scenario Maps/Tutorial.sce");
                        break;
                    case WorldSettingsData.World_Type.Oona:
                        scenarioMapData = SaveManager.Instance.GetScenarioMapData($"{Application.streamingAssetsPath}/Scenario Maps/Oona.sce");
                        break;
                    case WorldSettingsData.World_Type.Pangat_Loo:
                        scenarioMapData = SaveManager.Instance.GetScenarioMapData($"{Application.streamingAssetsPath}/Scenario Maps/Pangat_Loo.sce");
                        break;
                    case WorldSettingsData.World_Type.Zenko:
                        scenarioMapData = SaveManager.Instance.GetScenarioMapData($"{Application.streamingAssetsPath}/Scenario Maps/Zenko.sce");
                        break;
                    case WorldSettingsData.World_Type.Affatt:
                        scenarioMapData = SaveManager.Instance.GetScenarioMapData($"{Application.streamingAssetsPath}/Scenario Maps/Affatt.sce");
                        break;
                    case WorldSettingsData.World_Type.Icalawa:
                        scenarioMapData = SaveManager.Instance.GetScenarioMapData($"{Application.streamingAssetsPath}/Scenario Maps/Icalawa.sce");
                        break;
                    default:
                        throw new Exception($"There is no scenario map data for {WorldSettings.Instance.worldSettingsData.worldType.ToString()}");
                }

                if (scenarioMapData != null && !WorldConfigManager.Instance.useRandomGenerationForScenarioMaps) {
                    yield return StartCoroutine(mapGenerator.InitializeScenarioWorld(scenarioMapData));    
                } else {
                    Debug.Log("Generating random world...");
                    yield return StartCoroutine(mapGenerator.InitializeWorld());    
                }
            }
            else {
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
        Messenger.RemoveListener(Signals.START_GAME_AFTER_LOADOUT_SELECT, OnLoadoutSelected);
        initializer.InitializeDataAfterLoadoutSelection();
    }
}

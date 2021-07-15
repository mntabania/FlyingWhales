using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using Ruinarch;
using UnityEngine;
using Inner_Maps;
using Scenario_Maps;
using UtilityScripts;

public class SingletonDataGeneration : MapGenerationComponent {
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
        yield return null;
	}

	#region Scenario Maps
	public override IEnumerator LoadScenarioData(MapGenerationData data, ScenarioMapData scenarioMapData) {
		yield return MapGenerator.Instance.StartCoroutine(ExecuteRandomGeneration(data));
	}
	#endregion
	
	#region Saved World
	public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData) {
		yield return MapGenerator.Instance.StartCoroutine(LoadSaveDataPlagueDisease(data, saveData));
    }
    public override void LoadSavedData(object state) {
        try {
            LoadThreadQueueItem threadItem = state as LoadThreadQueueItem;
            MapGenerationData mapData = threadItem.mapData;
            SaveDataCurrentProgress saveData = threadItem.saveData;
            saveData.LoadPlagueDisease();
            threadItem.isDone = true;
        } catch (Exception e) {
            Debug.LogError(e.Message + "\n" + e.StackTrace);
        }
    }
    #endregion

    #region Plague Disease
    private IEnumerator LoadSaveDataPlagueDisease(MapGenerationData data, SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Data...");
        saveData.LoadPlagueDisease();
        yield return null;
    }
    #endregion
}

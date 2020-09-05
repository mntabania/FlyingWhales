using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using Ruinarch;
using UnityEngine;
using Inner_Maps;
using Scenario_Maps;
using UtilityScripts;

public class LoadCharactersCurrentAction : MapGenerationComponent {
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
        yield return null;
	}

	#region Scenario Maps
	public override IEnumerator LoadScenarioData(MapGenerationData data, ScenarioMapData scenarioMapData) {
        yield return null;
    }
    #endregion

    #region Saved World
    public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData) {
        yield return MapGenerator.Instance.StartCoroutine(LoadActions(data, saveData));
    }
    #endregion

    #region Player
    private IEnumerator LoadActions(MapGenerationData data, SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Preparing Character Actions...");
        saveData.LoadCharactersCurrentAction();
        yield return null;
    }
    #endregion
}

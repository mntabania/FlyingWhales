using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using Ruinarch;
using UnityEngine;
using Inner_Maps;
using Scenario_Maps;
using UtilityScripts;

public class LoadAwarenessGeneration : MapGenerationComponent {
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
        yield return null;
        //yield return MapGenerator.Instance.StartCoroutine(LoadAwareness(data, saveData));
    }
    #endregion

    #region Player
    private IEnumerator LoadAwareness(MapGenerationData data, SaveDataCurrentProgress saveData) {
        yield return null;
        //LevelLoaderManager.Instance.UpdateLoadingInfo("Updating Awareness...");
        //for (int i = 0; i < LocationAwarenessUtility.allLocationsToBeUpdated.Count; i++) {
        //    LocationAwarenessUtility.allLocationsToBeUpdated[i].UpdateAwareness();
        //    yield return null;
        //}
        //LocationAwarenessUtility.allLocationsToBeUpdated.Clear();
        /* removed by aaron aranas for awareness update
        for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
            Region region = GridMap.Instance.allRegions[i];
            region.UpdateAwareness();
            yield return null;
        }*/
    }
    #endregion
}

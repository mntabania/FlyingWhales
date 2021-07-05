using System.Collections;
using System.Collections.Generic;
using Scenario_Maps;
using UnityEngine;

public class SupportingFactionGeneration : MapGenerationComponent {
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
		LevelLoaderManager.Instance.UpdateLoadingInfo("Creating Factions...");
		FactionManager.Instance.CreateWildMonsterFaction();
		FactionManager.Instance.CreateVagrantFaction();
		FactionManager.Instance.CreateDisguisedFaction();
		yield return null;
	}

	#region Scenario Maps
	public override IEnumerator LoadScenarioData(MapGenerationData data, ScenarioMapData scenarioMapData) {
		yield return MapGenerator.Instance.StartCoroutine(ExecuteRandomGeneration(data));
	}
	#endregion
	
	#region Saved World
	public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData) {
		yield return MapGenerator.Instance.StartCoroutine(ExecuteRandomGeneration(data));
	}
	#endregion
}

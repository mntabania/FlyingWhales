using System.Collections;
using System.Collections.Generic;
using Scenario_Maps;
using UnityEngine;

public class ElevationGeneration : MapGenerationComponent {
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
		LevelLoaderManager.Instance.UpdateLoadingInfo("Generating Elevation Maps...");
		// Biomes.Instance.GenerateElevation(GridMap.Instance.allAreas, GridMap.Instance.width, GridMap.Instance.height);
		yield return null;
	}
}

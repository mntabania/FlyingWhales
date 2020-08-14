using System.Collections;
using System.Collections.Generic;
using Scenario_Maps;
using UnityEngine;

public class WorldMapElevationGeneration : MapGenerationComponent {
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
		LevelLoaderManager.Instance.UpdateLoadingInfo("Generating elevation maps...");
		EquatorGenerator.Instance.GenerateEquator(GridMap.Instance.width, GridMap.Instance.height, GridMap.Instance.normalHexTiles);
		yield return MapGenerator.Instance.StartCoroutine(Biomes.Instance.GenerateElevation(GridMap.Instance.normalHexTiles, GridMap.Instance.width, GridMap.Instance.height));
	}
}

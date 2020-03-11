using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;

public class RegionInnerMapGeneration : MapGenerationComponent {
	
	public override IEnumerator Execute(MapGenerationData data) {
		for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
			Region region = GridMap.Instance.allRegions[i];
			yield return MapGenerator.Instance.StartCoroutine(LandmarkManager.Instance.GenerateRegionMap(region, this));
		}
	}
}

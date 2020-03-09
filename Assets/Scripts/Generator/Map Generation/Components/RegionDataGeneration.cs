using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegionDataGeneration : MapGenerationComponent {
	public override IEnumerator Execute(MapGenerationData data) {
		// LandmarkManager.Instance.GenerateRegionFeatures();
		// yield return null;
		yield return null;
		// yield return MapGenerator.Instance.StartCoroutine(LandmarkManager.Instance.GenerateRegionInnerMaps());
		LandmarkManager.Instance.MakeAllRegionsAwareOfEachOther();
	}
}

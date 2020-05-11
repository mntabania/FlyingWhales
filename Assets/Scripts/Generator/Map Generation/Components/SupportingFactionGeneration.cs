using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupportingFactionGeneration : MapGenerationComponent {
	public override IEnumerator Execute(MapGenerationData data) {
		LevelLoaderManager.Instance.UpdateLoadingInfo("Creating factions...");
		FactionManager.Instance.CreateNeutralFaction();
		FactionManager.Instance.CreateFriendlyNeutralFaction();
		FactionManager.Instance.CreateDisguisedFaction();
		yield return null;
	}
}

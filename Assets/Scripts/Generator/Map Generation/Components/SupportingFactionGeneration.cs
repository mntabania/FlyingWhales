using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupportingFactionGeneration : MapGenerationComponent {
	public override IEnumerator Execute(MapGenerationData data) {
		LevelLoaderManager.Instance.UpdateLoadingInfo("Creating factions...");
		FactionManager.Instance.CreateWildMonsterFaction();
		FactionManager.Instance.CreateVagrantFaction();
		FactionManager.Instance.CreateDisguisedFaction();
		yield return null;
	}
}

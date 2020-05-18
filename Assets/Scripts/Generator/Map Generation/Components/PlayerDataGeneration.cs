using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using Ruinarch;
using UnityEngine;
using Inner_Maps;
using UtilityScripts;

public class PlayerDataGeneration : MapGenerationComponent {
	public override IEnumerator Execute(MapGenerationData data) {
        //PlayerManager.Instance.InitializePlayer(data.portal, data.portalStructure, InputManager.Instance.selectedArchetype);
        PlayerManager.Instance.InitializePlayer(data.portal, data.portalStructure, PLAYER_ARCHETYPE.Normal);
        yield return null;
		yield return MapGenerator.Instance.StartCoroutine(LoadArtifacts());
		//GenerateInitialMinions();
		yield return null;
	}

	//private void GenerateInitialMinions() {
 //       List<string> archetypeMinions = PlayerManager.Instance.player.archetype.minionClasses;
 //       for (int i = 0; i < archetypeMinions.Count; i++) {
 //           string className = archetypeMinions[i];
 //           Minion minion = PlayerManager.Instance.player.CreateNewMinion(className, RACE.DEMON, false);
 //           minion.SetCombatAbility(COMBAT_ABILITY.FLAMESTRIKE);
 //           PlayerManager.Instance.player.AddMinion(minion);
 //           PlayerManager.Instance.player.portalTile.region.RemoveCharacterFromLocation(minion.character);
 //       }
 //   }
	
	private IEnumerator LoadArtifacts() {
		List<ARTIFACT_TYPE> artifactChoices = WorldConfigManager.Instance.initialArtifactChoices;

		if (WorldConfigManager.Instance.isDemoWorld) {
			//if demo build, always spawn necronomicon at ancient ruins
			artifactChoices.Remove(ARTIFACT_TYPE.Necronomicon);
			Region randomRegion = CollectionUtilities.GetRandomElement(GridMap.Instance.allRegions);
			LocationStructure ancientRuin = randomRegion.GetRandomStructureOfType(STRUCTURE_TYPE.ANCIENT_RUIN);
			Artifact artifact = InnerMapManager.Instance.CreateNewArtifact(ARTIFACT_TYPE.Necronomicon);
			ancientRuin.AddPOI(artifact);
			
			//place berserk orb at monster lair
			artifactChoices.Remove(ARTIFACT_TYPE.Berserk_Orb);
			LocationStructure monsterLair = randomRegion.GetRandomStructureOfType(STRUCTURE_TYPE.MONSTER_LAIR);
			artifact = InnerMapManager.Instance.CreateNewArtifact(ARTIFACT_TYPE.Berserk_Orb);
			monsterLair.AddPOI(artifact);
		}
		else {
			//randomly generate 3 Artifacts
			for (int i = 0; i < 3; i++) {
				if (artifactChoices.Count == 0) { break; }
				Region randomRegion = CollectionUtilities.GetRandomElement(GridMap.Instance.allRegions);
				LocationStructure wilderness = randomRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
				ARTIFACT_TYPE randomArtifact = CollectionUtilities.GetRandomElement(artifactChoices);
				Artifact artifact = InnerMapManager.Instance.CreateNewArtifact(randomArtifact);
				wilderness.AddPOI(artifact);
				artifactChoices.Remove(randomArtifact);
			}
		}
		
		
		
		// List<ARTIFACT_TYPE> artifactChoices = UtilityScripts.CollectionUtilities.GetEnumValues<ARTIFACT_TYPE>().ToList();
		// artifactChoices.Remove(ARTIFACT_TYPE.None);
		// //place 2 within storage of the npcSettlement
		// List<NPCSettlement> settlementChoices = new List<NPCSettlement>(LandmarkManager.Instance.allNonPlayerSettlements.Where(
		// 	x => x.locationType == LOCATION_TYPE.ELVEN_SETTLEMENT || x.locationType == LOCATION_TYPE.HUMAN_SETTLEMENT));
		// for (int i = 0; i < 2; i++) {
		// 	if (settlementChoices.Count > 0) {
		// 		NPCSettlement chosenNpcSettlement = UtilityScripts.CollectionUtilities.GetRandomElement(settlementChoices);
		// 		ARTIFACT_TYPE artifactType = UtilityScripts.CollectionUtilities.GetRandomElement(artifactChoices);
		// 		Artifact artifact = PlayerManager.Instance.CreateNewArtifact(artifactType);
		// 		chosenNpcSettlement.mainStorage.AddPOI(artifact);
		// 		artifactChoices.Remove(artifactType);
		// 		settlementChoices.Remove(chosenNpcSettlement);
		// 	} else {
		// 		Debug.LogWarning($"Could no longer find a valid npcSettlement to place an artifact");
		// 		break;
		// 	}
		// }
		//
		// //place remaining in monster lairs/wilderness
		// List<BaseLandmark> monsterLairChoices = LandmarkManager.Instance.GetLandmarksOfType(LANDMARK_TYPE.MONSTER_LAIR);
		// List<Region> nonPlayerRegions = GridMap.Instance.allRegions
		// 	.Where(x => x.HasStructure(STRUCTURE_TYPE.THE_PORTAL) == false).ToList();
		// for (int i = 0; i < 3; i++) {
		// 	if (monsterLairChoices.Count > 0) {
		// 		BaseLandmark chosenLair = UtilityScripts.CollectionUtilities.GetRandomElement(monsterLairChoices);
		// 		LocationStructure structure = chosenLair.tileLocation.settlementOnTile.GetRandomStructureOfType(STRUCTURE_TYPE.MONSTER_LAIR);
		// 		ARTIFACT_TYPE artifactType = UtilityScripts.CollectionUtilities.GetRandomElement(artifactChoices);
		// 		Artifact artifact = PlayerManager.Instance.CreateNewArtifact(artifactType);
		// 		structure.AddPOI(artifact);
		// 		artifactChoices.Remove(artifactType);
		// 		monsterLairChoices.Remove(chosenLair);
		// 	} else { //no more monster lairs
		// 		Region chosenRegion = UtilityScripts.CollectionUtilities.GetRandomElement(nonPlayerRegions);
		// 		LocationStructure wilderness = chosenRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
		// 		ARTIFACT_TYPE artifactType = UtilityScripts.CollectionUtilities.GetRandomElement(artifactChoices);
		// 		Artifact artifact = PlayerManager.Instance.CreateNewArtifact(artifactType);
		// 		wilderness.AddPOI(artifact);
		// 		artifactChoices.Remove(artifactType);
		// 		Debug.Log($"No more monster lairs for {artifact}. It was placed at {artifact.gridTileLocation} at {chosenRegion.name} instead.");
		// 	}
		// }
        yield return null;
	}
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilityScripts;

public class WorldMapLandmarkGeneration : MapGenerationComponent {

	public override IEnumerator Execute(MapGenerationData data) {
		CreateMonsterLairs();
		yield return null;
		CreateAbandonedMines();
		yield return null;
		CreateTemples();
		yield return null;
		CreateMageTowers();
		yield return null;
	}

	private void CreateMonsterLairs() {
		int createdCount = 0;
		for (int i = 0; i < 3; i++) { //3
			if (Random.Range(0, 100) < 75) { //75
				List<HexTile> choices = GridMap.Instance.normalHexTiles
					.Where(x => x.elevationType == ELEVATION.PLAIN && x.featureComponent.features.Count == 0 && x.landmarkOnTile == null)
					.ToList();
				if (choices.Count > 0) {
					HexTile chosenTile = CollectionUtilities.GetRandomElement(choices);
					LandmarkManager.Instance.CreateNewLandmarkOnTile(chosenTile, LANDMARK_TYPE.MONSTER_LAIR, false);
					NPCSettlement npcSettlement =
						LandmarkManager.Instance.CreateNewSettlement(chosenTile.region, LOCATION_TYPE.DUNGEON, 0,
							chosenTile);
					createdCount++;
				} else {
					break;
				}
			}
		}
		Debug.Log($"Created {createdCount.ToString()} Monster Lairs");
	}
	private void CreateAbandonedMines() {
		int createdCount = 0;
		for (int i = 0; i < 2; i++) {
			if (Random.Range(0, 100) < 50) { //50
				List<HexTile> choices = GridMap.Instance.normalHexTiles
					.Where(x => x.elevationType == ELEVATION.PLAIN && x.featureComponent.features.Count == 0
					            && x.HasNeighbourWithElevation(ELEVATION.MOUNTAIN) && x.landmarkOnTile == null)
					.ToList();
				if (choices.Count > 0) {
					HexTile chosenTile = CollectionUtilities.GetRandomElement(choices);
					LandmarkManager.Instance.CreateNewLandmarkOnTile(chosenTile, LANDMARK_TYPE.MINE, false);
					NPCSettlement npcSettlement =
						LandmarkManager.Instance.CreateNewSettlement(chosenTile.region, LOCATION_TYPE.DUNGEON, 0,
							chosenTile);
					createdCount++;
				} else {
					break;
				}
			}
		}
		Debug.Log($"Created {createdCount.ToString()} Mines");
	}
	private void CreateTemples() {
		int createdCount = 0;
		for (int i = 0; i < 2; i++) {
			if (Random.Range(0, 100) < 35) { //35
				List<HexTile> choices = GridMap.Instance.normalHexTiles
					.Where(x => x.elevationType == ELEVATION.PLAIN && x.featureComponent.features.Count == 0 && x.landmarkOnTile == null)
					.ToList();
				if (choices.Count > 0) {
					HexTile chosenTile = CollectionUtilities.GetRandomElement(choices);
					LandmarkManager.Instance.CreateNewLandmarkOnTile(chosenTile, LANDMARK_TYPE.ANCIENT_RUIN, false);
					NPCSettlement npcSettlement =
						LandmarkManager.Instance.CreateNewSettlement(chosenTile.region, LOCATION_TYPE.DUNGEON, 0,
							chosenTile);
					createdCount++;
				} else {
					break;
				}
			}
		}
		Debug.Log($"Created {createdCount.ToString()} Temples");
	}
	private void CreateMageTowers() {
		int createdCount = 0;
		for (int i = 0; i < 2; i++) {
			if (Random.Range(0, 100) < 35) { //35
				List<HexTile> choices = GridMap.Instance.normalHexTiles
					.Where(x => x.elevationType == ELEVATION.PLAIN && x.featureComponent.features.Count == 0 && x.landmarkOnTile == null)
					.ToList();
				if (choices.Count > 0) {
					HexTile chosenTile = CollectionUtilities.GetRandomElement(choices);
					LandmarkManager.Instance.CreateNewLandmarkOnTile(chosenTile, LANDMARK_TYPE.MAGE_TOWER, false);
					NPCSettlement npcSettlement =
						LandmarkManager.Instance.CreateNewSettlement(chosenTile.region, LOCATION_TYPE.DUNGEON, 0,
							chosenTile);
					createdCount++;
				} else {
					break;
				}
			}
		}
		Debug.Log($"Created {createdCount.ToString()} Mage Towers");
	}
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilityScripts;

public class WorldMapLandmarkGeneration : MapGenerationComponent {

	public override IEnumerator Execute(MapGenerationData data) {
		CreateMonsterLairs(WorldConfigManager.Instance.isDemoWorld ? 1 : 3, WorldConfigManager.Instance.isDemoWorld ? 100 : 75);
		yield return null;
		CreateAbandonedMines(WorldConfigManager.Instance.isDemoWorld ? 0 : 2, WorldConfigManager.Instance.isDemoWorld ? 0 : 50);
		yield return null;
		CreateTemples(WorldConfigManager.Instance.isDemoWorld ? 1 : 2, WorldConfigManager.Instance.isDemoWorld ? 100 : 35);
		yield return null;
		CreateMageTowers(WorldConfigManager.Instance.isDemoWorld ? 0 : 2, WorldConfigManager.Instance.isDemoWorld ? 0 : 35);
		yield return null;
	}

	private void CreateMonsterLairs(int loopCount, int chance) {
		int createdCount = 0;
		for (int i = 0; i < loopCount; i++) { //3
			if (Random.Range(0, 100) < chance) { //75
				List<HexTile> choices = GridMap.Instance.normalHexTiles
					.Where(x => x.elevationType == ELEVATION.PLAIN && x.featureComponent.features.Count == 0 && x.landmarkOnTile == null)
					.ToList();
				if (choices.Count > 0) {
					HexTile chosenTile = CollectionUtilities.GetRandomElement(choices);
					LandmarkManager.Instance.CreateNewLandmarkOnTile(chosenTile, LANDMARK_TYPE.MONSTER_LAIR);
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
	private void CreateAbandonedMines(int loopCount, int chance) {
		int createdCount = 0;
		for (int i = 0; i < loopCount; i++) {
			if (Random.Range(0, 100) < chance) { //50
				List<HexTile> choices = GridMap.Instance.normalHexTiles
					.Where(x => x.elevationType == ELEVATION.PLAIN && x.featureComponent.features.Count == 0
					            && x.HasNeighbourWithElevation(ELEVATION.MOUNTAIN) && x.landmarkOnTile == null)
					.ToList();
				if (choices.Count > 0) {
					HexTile chosenTile = CollectionUtilities.GetRandomElement(choices);
					LandmarkManager.Instance.CreateNewLandmarkOnTile(chosenTile, LANDMARK_TYPE.ABANDONED_MINE);
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
	private void CreateTemples(int loopCount, int chance) {
		int createdCount = 0;
		for (int i = 0; i < loopCount; i++) {
			if (Random.Range(0, 100) < chance) { //35
				List<HexTile> choices = GridMap.Instance.normalHexTiles
					.Where(x => x.elevationType == ELEVATION.PLAIN && x.featureComponent.features.Count == 0 && x.landmarkOnTile == null)
					.ToList();
				if (choices.Count > 0) {
					HexTile chosenTile = CollectionUtilities.GetRandomElement(choices);
					LandmarkManager.Instance.CreateNewLandmarkOnTile(chosenTile, LANDMARK_TYPE.ANCIENT_RUIN);
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
	private void CreateMageTowers(int loopCount, int chance) {
		int createdCount = 0;
		for (int i = 0; i < loopCount; i++) {
			if (Random.Range(0, 100) < chance) { //35
				List<HexTile> choices = GridMap.Instance.normalHexTiles
					.Where(x => x.elevationType == ELEVATION.PLAIN && x.featureComponent.features.Count == 0 && x.landmarkOnTile == null)
					.ToList();
				if (choices.Count > 0) {
					HexTile chosenTile = CollectionUtilities.GetRandomElement(choices);
					LandmarkManager.Instance.CreateNewLandmarkOnTile(chosenTile, LANDMARK_TYPE.MAGE_TOWER);
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

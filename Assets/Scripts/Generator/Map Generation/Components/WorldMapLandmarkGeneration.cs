using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilityScripts;

public class WorldMapLandmarkGeneration : MapGenerationComponent {

	public override IEnumerator Execute(MapGenerationData data) {
		CreateMonsterLairs(GetLoopCount(LANDMARK_TYPE.MONSTER_LAIR, data), GetChance(LANDMARK_TYPE.MONSTER_LAIR, data));
		yield return null;
		CreateAbandonedMines(GetLoopCount(LANDMARK_TYPE.ABANDONED_MINE, data), GetChance(LANDMARK_TYPE.ABANDONED_MINE, data));
		yield return null;
		CreateTemples(GetLoopCount(LANDMARK_TYPE.ANCIENT_RUIN, data), GetChance(LANDMARK_TYPE.ANCIENT_RUIN, data));
		yield return null;
		CreateMageTowers(GetLoopCount(LANDMARK_TYPE.MAGE_TOWER, data), GetChance(LANDMARK_TYPE.MAGE_TOWER, data));
		yield return null;
		CreateAncientGraveyard(GetLoopCount(LANDMARK_TYPE.ANCIENT_GRAVEYARD, data), GetChance(LANDMARK_TYPE.ANCIENT_GRAVEYARD, data));
		yield return null;
	}

	private void CreateMonsterLairs(int loopCount, int chance) {
		int createdCount = 0;
		for (int i = 0; i < loopCount; i++) {
			if (Random.Range(0, 100) < chance) {
				List<HexTile> choices;
				if (WorldConfigManager.Instance.isDemoWorld) {
					choices = new List<HexTile>() {
						GridMap.Instance.map[2, 2]
					};
				} else {
					choices = GridMap.Instance.normalHexTiles
						.Where(x => x.elevationType == ELEVATION.PLAIN && x.featureComponent.features.Count == 0 && x.landmarkOnTile == null)
						.ToList();
				}
				if (choices.Count > 0) {
					HexTile chosenTile = CollectionUtilities.GetRandomElement(choices);
					LandmarkManager.Instance.CreateNewLandmarkOnTile(chosenTile, LANDMARK_TYPE.MONSTER_LAIR);
					LandmarkManager.Instance.CreateNewSettlement(chosenTile.region, LOCATION_TYPE.DUNGEON,
						chosenTile);
					if (WorldConfigManager.Instance.isDemoWorld) {
						//make sure that chosen tiles for demo are flat and featureless  
						chosenTile.featureComponent.RemoveAllFeatures(chosenTile);
						chosenTile.SetElevation(ELEVATION.PLAIN);
					}
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
			if (Random.Range(0, 100) < chance) {
				List<HexTile> choices = GridMap.Instance.normalHexTiles
					.Where(x => x.elevationType == ELEVATION.PLAIN && x.featureComponent.features.Count == 0
					            && x.HasNeighbourWithElevation(ELEVATION.MOUNTAIN) && x.landmarkOnTile == null)
					.ToList();
				if (choices.Count > 0) {
					HexTile chosenTile = CollectionUtilities.GetRandomElement(choices);
					LandmarkManager.Instance.CreateNewLandmarkOnTile(chosenTile, LANDMARK_TYPE.ABANDONED_MINE);
					LandmarkManager.Instance.CreateNewSettlement(chosenTile.region, LOCATION_TYPE.DUNGEON,
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
			if (Random.Range(0, 100) < chance) {
				List<HexTile> choices;
				if (WorldConfigManager.Instance.isDemoWorld) {
					choices = new List<HexTile>() {
						GridMap.Instance.map[6, 8]
					};
				} else {
					choices = GridMap.Instance.normalHexTiles
						.Where(x => x.elevationType == ELEVATION.PLAIN && x.featureComponent.features.Count == 0 && x.landmarkOnTile == null)
						.ToList();
				}
				if (choices.Count > 0) {
					HexTile chosenTile = CollectionUtilities.GetRandomElement(choices);
					LandmarkManager.Instance.CreateNewLandmarkOnTile(chosenTile, LANDMARK_TYPE.ANCIENT_RUIN);
					LandmarkManager.Instance.CreateNewSettlement(chosenTile.region, LOCATION_TYPE.DUNGEON,
						chosenTile);
					if (WorldConfigManager.Instance.isDemoWorld) {
						//make sure that chosen tiles for demo are flat and featureless  
						chosenTile.featureComponent.RemoveAllFeatures(chosenTile);
						chosenTile.SetElevation(ELEVATION.PLAIN);
					}
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
			if (Random.Range(0, 100) < chance) {
				List<HexTile> choices = GridMap.Instance.normalHexTiles
					.Where(x => x.elevationType == ELEVATION.PLAIN && x.featureComponent.features.Count == 0 && x.landmarkOnTile == null)
					.ToList();
				if (choices.Count > 0) {
					HexTile chosenTile = CollectionUtilities.GetRandomElement(choices);
					LandmarkManager.Instance.CreateNewLandmarkOnTile(chosenTile, LANDMARK_TYPE.MAGE_TOWER);
					LandmarkManager.Instance.CreateNewSettlement(chosenTile.region, LOCATION_TYPE.DUNGEON,
						chosenTile);
					createdCount++;
				} else {
					break;
				}
			}
		}
		Debug.Log($"Created {createdCount.ToString()} Mage Towers");
	}
	private void CreateAncientGraveyard(int loopCount, int chance) {
		int createdCount = 0;
		for (int i = 0; i < loopCount; i++) {
			if (Random.Range(0, 100) < chance) {
				List<HexTile> choices = GridMap.Instance.normalHexTiles
					.Where(x => x.elevationType == ELEVATION.PLAIN && x.landmarkOnTile == null)
					.ToList();
				if (choices.Count > 0) {
					HexTile chosenTile = CollectionUtilities.GetRandomElement(choices);
					LandmarkManager.Instance.CreateNewLandmarkOnTile(chosenTile, LANDMARK_TYPE.ANCIENT_GRAVEYARD);
					LandmarkManager.Instance.CreateNewSettlement(chosenTile.region, LOCATION_TYPE.DUNGEON,
						chosenTile);
					createdCount++;
				} else {
					break;
				}
			}
		}
		Debug.Log($"Created {createdCount.ToString()} Ancient Graveyards");
	}

	private int GetLoopCount(LANDMARK_TYPE landmarkType, MapGenerationData data) {
		switch (landmarkType) {
			case LANDMARK_TYPE.MONSTER_LAIR:
				if (WorldConfigManager.Instance.isDemoWorld) {
					return 1;
				}
				if (data.regionCount == 1) {
					return 1;
				} else if (data.regionCount == 2 || data.regionCount == 3) {
					return 2;
				} else {
					return 3;
				}
			case LANDMARK_TYPE.ABANDONED_MINE:
				if (WorldConfigManager.Instance.isDemoWorld) {
					return 0;
				}
				bool monsterLairWasBuilt =
					LandmarkManager.Instance.GetLandmarkOfType(LANDMARK_TYPE.MONSTER_LAIR) != null;
				if (data.regionCount == 1) {
					return monsterLairWasBuilt ? 0 : 1;
				} else if (data.regionCount == 2 || data.regionCount == 3) {
					return monsterLairWasBuilt ? 1 : 2;
				} else {
					return monsterLairWasBuilt ? 2 : 3;
				}
			case LANDMARK_TYPE.ANCIENT_RUIN:
				if (WorldConfigManager.Instance.isDemoWorld) {
					return 1;
				}
				if (data.regionCount == 1) {
					return 1;
				} else if (data.regionCount == 2 || data.regionCount == 3) {
					return 2;
				} else {
					return 3;
				}
			case LANDMARK_TYPE.MAGE_TOWER:
				if (WorldConfigManager.Instance.isDemoWorld) {
					return 0;
				}
				bool templeWasBuilt =
					LandmarkManager.Instance.GetLandmarkOfType(LANDMARK_TYPE.ANCIENT_RUIN) != null;
				if (data.regionCount == 1) {
					return templeWasBuilt ? 0 : 1;
				} else if (data.regionCount == 2 || data.regionCount == 3) {
					return templeWasBuilt ? 1 : 2;
				} else {
					return 3;
				}
			case LANDMARK_TYPE.ANCIENT_GRAVEYARD:
				if (WorldConfigManager.Instance.isDemoWorld) {
					return 0;
				}
				bool graveyardWasBuilt =
					LandmarkManager.Instance.GetLandmarkOfType(LANDMARK_TYPE.ANCIENT_GRAVEYARD) != null;
				if (data.regionCount == 1) {
					return graveyardWasBuilt ? 0 : 1;
				} else if (data.regionCount == 2 || data.regionCount == 3) {
					return graveyardWasBuilt ? 1 : 2;
				} else {
					return 3;
				}
			default:
				return 0;
		}
	}
	private int GetChance(LANDMARK_TYPE landmarkType, MapGenerationData data) {
		switch (landmarkType) {
			case LANDMARK_TYPE.MONSTER_LAIR:
				return WorldConfigManager.Instance.isDemoWorld ? 100 : 75;
			case LANDMARK_TYPE.ABANDONED_MINE:
				return WorldConfigManager.Instance.isDemoWorld ? 0 : 50;
			case LANDMARK_TYPE.ANCIENT_RUIN:
				return WorldConfigManager.Instance.isDemoWorld ? 100 : 35;
			case LANDMARK_TYPE.MAGE_TOWER:
				return WorldConfigManager.Instance.isDemoWorld ? 0 : 35;
			case LANDMARK_TYPE.ANCIENT_GRAVEYARD:
				return WorldConfigManager.Instance.isDemoWorld ? 0 : 75;
			default:
				return 0;
		}
	}
}

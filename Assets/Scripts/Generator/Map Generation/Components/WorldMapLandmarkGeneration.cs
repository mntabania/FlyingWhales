// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using Locations.Region_Features;
// using Locations.Area_Features;
// using Scenario_Maps;
// using UnityEngine;
// using UtilityScripts;
//
// public class WorldMapLandmarkGeneration : MapGenerationComponent {
//
// 	#region Random Generation
// 	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
// 		TryCreateMonsterLairs(GetLoopCount(LANDMARK_TYPE.MONSTER_LAIR, data), GetChance(LANDMARK_TYPE.MONSTER_LAIR, data));
// 		yield return null;
// 		TryCreateAbandonedMines(GetLoopCount(LANDMARK_TYPE.ABANDONED_MINE, data), GetChance(LANDMARK_TYPE.ABANDONED_MINE, data));
// 		yield return null;
// 		TryCreateTemples(GetLoopCount(LANDMARK_TYPE.TEMPLE, data), GetChance(LANDMARK_TYPE.TEMPLE, data));
// 		yield return null;
// 		TryCreateMageTowers(GetLoopCount(LANDMARK_TYPE.MAGE_TOWER, data), GetChance(LANDMARK_TYPE.MAGE_TOWER, data));
// 		yield return null;
// 		TryCreateAncientGraveyard(GetLoopCount(LANDMARK_TYPE.ANCIENT_GRAVEYARD, data), GetChance(LANDMARK_TYPE.ANCIENT_GRAVEYARD, data));
// 		yield return null;
// 		TryCreateRuinedZoo(GetLoopCount(LANDMARK_TYPE.RUINED_ZOO, data), GetChance(LANDMARK_TYPE.RUINED_ZOO, data));
// 		yield return null;
// 		TryCreateAncientRuin(GetLoopCount(LANDMARK_TYPE.ANCIENT_RUIN, data), GetChance(LANDMARK_TYPE.ANCIENT_RUIN, data));
// 		yield return null;
// 		LandmarkSecondPass();
// 		yield return null;
// 	}
// 	private void LandmarkSecondPass() {
// 		for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
// 			Region region = GridMap.Instance.allRegions[i];
// 			for (int j = 0; j < region.regionFeatureComponent.features.Count; j++) {
// 				RegionFeature feature = region.regionFeatureComponent.features[j];
// 				feature.LandmarkGenerationSecondPassActions(region);
// 			}
// 		}
// 	}
// 	private void TryCreateMonsterLairs(int loopCount, int chance) {
// 		int createdCount = 0;
// 		for (int i = 0; i < loopCount; i++) {
// 			if (Random.Range(0, 100) < chance) {
// 				List<Area> choices;
// 				if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
// 					if (i == 0) {
// 						choices = new List<Area>() { GridMap.Instance.map[1, 4] };	
// 					} else {
// 						choices = new List<Area>() { GridMap.Instance.map[3, 7] };
// 					}
// 				} else {
// 					choices = GridMap.Instance.allAreas
// 						.Where(x => x.elevationType == ELEVATION.PLAIN && //a random flat tile
// 						            x.featureComponent.features.Count == 0 && x.landmarkOnTile == null && //with no Features yet
// 						            !IsInRangeOfSettlement(x, 3) && !IsAdjacentToSpecialStructure(x) //and not adjacent to player Portal, Settlement or other non-cave landmarks
// 						)
// 						.ToList();
// 				}
// 					
// 				if (choices.Count > 0) {
// 					Area chosenTile = CollectionUtilities.GetRandomElement(choices);
// 					LANDMARK_TYPE landmarkType = LANDMARK_TYPE.MONSTER_LAIR;
// 					if (chosenTile.region.regionFeatureComponent.HasFeature<RuinsFeature>()) {
// 						landmarkType = LANDMARK_TYPE.ANCIENT_RUIN;
// 					} else if (chosenTile.region.regionFeatureComponent.HasFeature<HauntedFeature>()) {
// 						landmarkType = LANDMARK_TYPE.ANCIENT_GRAVEYARD;
// 					}
// 					LandmarkManager.Instance.CreateNewLandmarkOnTile(chosenTile, landmarkType);
// 					LandmarkManager.Instance.CreateNewSettlement(chosenTile.region, LOCATION_TYPE.DUNGEON, chosenTile);
// 					if (WorldConfigManager.Instance.isTutorialWorld) {
// 						//make sure that chosen tiles for demo are flat and featureless  
// 						chosenTile.featureComponent.RemoveAllFeatures(chosenTile);
// 						chosenTile.SetElevation(ELEVATION.PLAIN);
// 					}
// 					createdCount++;
// 				} else {
// 					break;
// 				}
// 			}
// 		}
// 		Debug.Log($"Created {createdCount.ToString()} Monster Lairs");
// 	}
// 	private void TryCreateAbandonedMines(int loopCount, int chance) {
// 		int createdCount = 0;
// 		for (int i = 0; i < loopCount; i++) {
// 			if (Random.Range(0, 100) < chance) {
// 				List<HexTile> choices = GridMap.Instance.allAreas
// 					.Where(x => x.elevationType == ELEVATION.PLAIN && x.featureComponent.features.Count == 0 && 
// 					            x.HasNeighbourWithElevation(ELEVATION.MOUNTAIN) && x.landmarkOnTile == null &&  
// 					            !IsAdjacentToPortal(x) && !IsAdjacentToSettlement(x) && !IsAdjacentToSpecialStructure(x)//and not adjacent to player Portal, Settlement or other non-cave landmarks
// 					).ToList();
// 				if (choices.Count > 0) {
// 					HexTile chosenTile = CollectionUtilities.GetRandomElement(choices);
// 					LANDMARK_TYPE landmarkType = LANDMARK_TYPE.ABANDONED_MINE;
// 					if (chosenTile.region.regionFeatureComponent.HasFeature<RuinsFeature>()) {
// 						landmarkType = LANDMARK_TYPE.ANCIENT_RUIN;
// 					} else if (chosenTile.region.regionFeatureComponent.HasFeature<HauntedFeature>()) {
// 						landmarkType = LANDMARK_TYPE.ANCIENT_GRAVEYARD;
// 					}
// 					LandmarkManager.Instance.CreateNewLandmarkOnTile(chosenTile, landmarkType);
// 					LandmarkManager.Instance.CreateNewSettlement(chosenTile.region, LOCATION_TYPE.DUNGEON,
// 						chosenTile);
// 					createdCount++;
// 				} else {
// 					break;
// 				}
// 			}
// 		}
// 		Debug.Log($"Created {createdCount.ToString()} Mines");
// 	}
// 	private void TryCreateTemples(int loopCount, int chance) {
// 		int createdCount = 0;
// 		for (int i = 0; i < loopCount; i++) {
// 			if (Random.Range(0, 100) < chance) {
// 				List<HexTile> choices;
// 				if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
// 					choices = new List<HexTile>() { GridMap.Instance.map[6, 1] };	
// 				} else {
// 					choices = GridMap.Instance.allAreas
// 						.Where(x => x.elevationType == ELEVATION.PLAIN && x.featureComponent.features.Count == 0 && x.landmarkOnTile == null && 
// 						            !IsAdjacentToPortal(x) && !IsAdjacentToSettlement(x) && !IsAdjacentToSpecialStructure(x)//and not adjacent to player Portal, Settlement or other non-cave landmarks
// 						).ToList();
// 				}
// 					
// 				if (choices.Count > 0) {
// 					HexTile chosenTile = CollectionUtilities.GetRandomElement(choices);
// 					LANDMARK_TYPE landmarkType = LANDMARK_TYPE.TEMPLE;
// 					if (chosenTile.region.regionFeatureComponent.HasFeature<RuinsFeature>()) {
// 						landmarkType = LANDMARK_TYPE.ANCIENT_RUIN;
// 					} else if (chosenTile.region.regionFeatureComponent.HasFeature<HauntedFeature>()) {
// 						landmarkType = LANDMARK_TYPE.ANCIENT_GRAVEYARD;
// 					}
// 					LandmarkManager.Instance.CreateNewLandmarkOnTile(chosenTile, landmarkType);
// 					LandmarkManager.Instance.CreateNewSettlement(chosenTile.region, LOCATION_TYPE.DUNGEON,
// 						chosenTile);
// 					if (WorldConfigManager.Instance.isTutorialWorld) {
// 						//make sure that chosen tiles for demo are flat and featureless  
// 						chosenTile.featureComponent.RemoveAllFeatures(chosenTile);
// 						chosenTile.SetElevation(ELEVATION.PLAIN);
// 					}
// 					createdCount++;
// 				} else {
// 					break;
// 				}
// 			}
// 		}
// 		Debug.Log($"Created {createdCount.ToString()} Temples");
// 	}
// 	private void TryCreateMageTowers(int loopCount, int chance) {
// 		int createdCount = 0;
// 		for (int i = 0; i < loopCount; i++) {
// 			if (Random.Range(0, 100) < chance) {
// 				List<HexTile> choices = GridMap.Instance.allAreas
// 					.Where(x => x.elevationType == ELEVATION.PLAIN && x.featureComponent.features.Count == 0 && x.landmarkOnTile == null &&  
// 					            !IsAdjacentToPortal(x) && !IsAdjacentToSettlement(x) && !IsAdjacentToSpecialStructure(x)//and not adjacent to player Portal, Settlement or other non-cave landmarks
// 					).ToList();
// 				if (choices.Count > 0) {
// 					HexTile chosenTile = CollectionUtilities.GetRandomElement(choices);
// 					LANDMARK_TYPE landmarkType = LANDMARK_TYPE.MAGE_TOWER;
// 					if (chosenTile.region.regionFeatureComponent.HasFeature<RuinsFeature>()) {
// 						landmarkType = LANDMARK_TYPE.ANCIENT_RUIN;
// 					} else if (chosenTile.region.regionFeatureComponent.HasFeature<HauntedFeature>()) {
// 						landmarkType = LANDMARK_TYPE.ANCIENT_GRAVEYARD;
// 					}
// 					LandmarkManager.Instance.CreateNewLandmarkOnTile(chosenTile, landmarkType);
// 					LandmarkManager.Instance.CreateNewSettlement(chosenTile.region, LOCATION_TYPE.DUNGEON,
// 						chosenTile);
// 					createdCount++;
// 				} else {
// 					break;
// 				}
// 			}
// 		}
// 		Debug.Log($"Created {createdCount.ToString()} Mage Towers");
// 	}
// 	private void TryCreateAncientGraveyard(int loopCount, int chance) {
// 		int createdCount = 0;
// 		for (int i = 0; i < loopCount; i++) {
// 			if (Random.Range(0, 100) < chance) {
// 				List<HexTile> choices;
// 				if (WorldConfigManager.Instance.isTutorialWorld) {
// 					if (i == 0) {
// 						choices = new List<HexTile>() {
// 							GridMap.Instance.map[2, 2]
// 						};	
// 					} else {
// 						choices = new List<HexTile>() {
// 							GridMap.Instance.map[6, 8]
// 						};
// 					}
// 				} else {
// 					choices = GridMap.Instance.allAreas
// 						.Where(x => x.elevationType == ELEVATION.PLAIN && x.landmarkOnTile == null &&
// 						            !IsAdjacentToPortal(x) && !IsAdjacentToSettlement(x) && !IsAdjacentToSpecialStructure(x)//and not adjacent to player Portal, Settlement or other non-cave landmarks
// 						).ToList();
// 				}
// 				if (choices.Count > 0) {
// 					HexTile chosenTile = CollectionUtilities.GetRandomElement(choices);
// 					LandmarkManager.Instance.CreateNewLandmarkOnTile(chosenTile, LANDMARK_TYPE.ANCIENT_GRAVEYARD);
// 					LandmarkManager.Instance.CreateNewSettlement(chosenTile.region, LOCATION_TYPE.DUNGEON,
// 						chosenTile);
// 					createdCount++;
// 				} else {
// 					break;
// 				}
// 			}
// 		}
// 		Debug.Log($"Created {createdCount.ToString()} Ancient Graveyards");
// 	}
// 	private void TryCreateRuinedZoo(int loopCount, int chance) {
// 		int createdCount = 0;
// 		for (int i = 0; i < loopCount; i++) {
// 			if (Random.Range(0, 100) < chance) {
// 				List<HexTile> choices;
// 				if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa) {
// 					choices = new List<HexTile>() {
// 						GridMap.Instance.map[2, 3]
// 					};
// 				} else {
// 					choices = GridMap.Instance.allAreas
// 						.Where(x => x.elevationType == ELEVATION.PLAIN && x.landmarkOnTile == null &&
// 						            !IsAdjacentToPortal(x) && !IsAdjacentToSettlement(x) && !IsAdjacentToSpecialStructure(x)//and not adjacent to player Portal, Settlement or other non-cave landmarks
// 						).ToList();
// 				}
// 				if (choices.Count > 0) {
// 					HexTile chosenTile = CollectionUtilities.GetRandomElement(choices);
// 					LandmarkManager.Instance.CreateNewLandmarkOnTile(chosenTile, LANDMARK_TYPE.RUINED_ZOO);
// 					LandmarkManager.Instance.CreateNewSettlement(chosenTile.region, LOCATION_TYPE.DUNGEON, chosenTile);
// 					createdCount++;
// 				} else {
// 					break;
// 				}
// 			}
// 		}
// 		Debug.Log($"Created {createdCount.ToString()} Ruined Zoo");
// 	}
// 	private void TryCreateAncientRuin(int loopCount, int chance) {
// 		int createdCount = 0;
// 		for (int i = 0; i < loopCount; i++) {
// 			if (Random.Range(0, 100) < chance) {
// 				List<HexTile> choices;
// 				if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa) {
// 					choices = new List<HexTile>() {
// 						GridMap.Instance.map[3, 0]
// 					};
// 				} else {
// 					choices = GridMap.Instance.allAreas
// 						.Where(x => x.elevationType == ELEVATION.PLAIN && x.landmarkOnTile == null &&
// 						            !IsAdjacentToPortal(x) && !IsAdjacentToSettlement(x) && !IsAdjacentToSpecialStructure(x)//and not adjacent to player Portal, Settlement or other non-cave landmarks
// 						).ToList();
// 				}
// 				if (choices.Count > 0) {
// 					HexTile chosenTile = CollectionUtilities.GetRandomElement(choices);
// 					LandmarkManager.Instance.CreateNewLandmarkOnTile(chosenTile, LANDMARK_TYPE.ANCIENT_RUIN);
// 					LandmarkManager.Instance.CreateNewSettlement(chosenTile.region, LOCATION_TYPE.DUNGEON, chosenTile);
// 					createdCount++;
// 				} else {
// 					break;
// 				}
// 			}
// 		}
// 		Debug.Log($"Created {createdCount.ToString()} Ruined Zoo");
// 	}
// 	private int GetLoopCount(LANDMARK_TYPE landmarkType, MapGenerationData data) {
// 		switch (landmarkType) {
// 			case LANDMARK_TYPE.MONSTER_LAIR:
// 				if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
// 					return 0;
// 				} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
// 					return 2;
// 				} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa) {
// 					return 0;
// 				} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Affatt) {
// 					return 0;
// 				} else {
// 					if (data.regionCount == 1) {
// 						return 1;
// 					} else if (data.regionCount == 2 || data.regionCount == 3) {
// 						return 2;
// 					} else {
// 						return 3;
// 					}	
// 				}
// 			case LANDMARK_TYPE.ABANDONED_MINE:
// 				if (WorldConfigManager.Instance.isTutorialWorld) {
// 					return 0;
// 				} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa) {
// 					return 0;
// 				}
// 				bool monsterLairWasBuilt = LandmarkManager.Instance.GetSpecialStructureOfType(STRUCTURE_TYPE.MONSTER_LAIR) != null;
// 				if (data.regionCount == 1) {
// 					return monsterLairWasBuilt ? 0 : 1;
// 				} else if (data.regionCount == 2 || data.regionCount == 3) {
// 					return monsterLairWasBuilt ? 1 : 2;
// 				} else {
// 					return monsterLairWasBuilt ? 2 : 3;
// 				}
// 			case LANDMARK_TYPE.TEMPLE:
// 				if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
// 					return 0;
// 				} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
// 					return 1;
// 				} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa) {
// 					return 0;
// 				} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Affatt) {
// 					return 5;
// 				} else {
// 					if (data.regionCount == 1) {
// 						return 1;
// 					} else if (data.regionCount == 2 || data.regionCount == 3) {
// 						return 2;
// 					} else {
// 						return 3;
// 					}
// 				}
// 			case LANDMARK_TYPE.MAGE_TOWER:
// 				if (WorldConfigManager.Instance.isTutorialWorld) {
// 					return 0;
// 				}  if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa) {
// 					return 0;
// 				} 
// 				bool templeWasBuilt = LandmarkManager.Instance.GetSpecialStructureOfType(STRUCTURE_TYPE.ANCIENT_RUIN) != null;
// 				if (data.regionCount == 1) {
// 					return templeWasBuilt ? 0 : 1;
// 				} else if (data.regionCount == 2 || data.regionCount == 3) {
// 					return templeWasBuilt ? 1 : 2;
// 				} else {
// 					return 3;
// 				}
// 			case LANDMARK_TYPE.ANCIENT_GRAVEYARD:
// 				if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
// 					return 2;
// 				} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
// 					//always ensure that an ancient graveyard is spawned in second world
// 					return 1;
// 				} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo) {
// 					return 3;
// 				} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa) {
// 					return 0;
// 				} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Affatt) {
// 					return 0;
// 				} else {
// 					return 0;
// 				}
// 			case LANDMARK_TYPE.RUINED_ZOO:
// 				if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa) {
// 					return 1;
// 				} else {
// 					return 0;
// 				}
// 			case LANDMARK_TYPE.ANCIENT_RUIN:
// 				if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa) {
// 					return 1;
// 				} else {
// 					return 0;
// 				}
// 			default:
// 				return 0;
// 		}
// 	}
// 	private int GetChance(LANDMARK_TYPE landmarkType, MapGenerationData data) {
// 		switch (landmarkType) {
// 			case LANDMARK_TYPE.MONSTER_LAIR:
// 				if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
// 					return 0;
// 				} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
// 					return 100;
// 				} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Affatt) {
// 					return 0;
// 				} else {
// 					return 75;
// 				}
// 			case LANDMARK_TYPE.ABANDONED_MINE:
// 				if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
// 					return 0;
// 				} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
// 					return 0;
// 				} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Affatt) {
// 					return 100;
// 				} else {
// 					return 50;
// 				}
// 			case LANDMARK_TYPE.TEMPLE:
// 				if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
// 					return 0;
// 				} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
// 					return 100;
// 				} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Affatt) {
// 					return 100;
// 				} else {
// 					return 35;
// 				}
// 			case LANDMARK_TYPE.MAGE_TOWER:
// 				if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
// 					return 0;
// 				} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
// 					return 0;
// 				} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Affatt) {
// 					return 100;
// 				} else {
// 					return 35;
// 				}
// 			case LANDMARK_TYPE.ANCIENT_GRAVEYARD:
// 				if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
// 					return 100;
// 				} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
// 					return 0;
// 				} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo) {
// 					return 75;
// 				} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Affatt) {
// 					return 100;
// 				} else {
// 					return 0;
// 				}
// 			case LANDMARK_TYPE.RUINED_ZOO:
// 				if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa) {
// 					return 100;
// 				} else {
// 					return 0;
// 				}
// 			case LANDMARK_TYPE.ANCIENT_RUIN:
// 				if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa) {
// 					return 100;
// 				} else {
// 					return 0;
// 				}
// 			default:
// 				return 0;
// 		}
// 	}
// 	#endregion
//
// 	#region Scenario Maps
// 	public override IEnumerator LoadScenarioData(MapGenerationData data, ScenarioMapData scenarioMapData) {
// 		List<SaveDataHextile> landmarkTiles = scenarioMapData.worldMapSave.GetAllTilesWithLandmarks();
// 		for (int i = 0; i < landmarkTiles.Count; i++) {
// 			SaveDataHextile saveData = landmarkTiles[i];
// 			//do not load player landmarks here! That is handled in PlayerSettlementGeneration.
// 			if (saveData.landmarkType.GetStructureType().IsSpecialStructure()) {
// 				HexTile hexTile = GridMap.Instance.map[saveData.xCoordinate, saveData.yCoordinate];
// 				LandmarkManager.Instance.CreateNewLandmarkOnTile(hexTile, saveData.landmarkType);
// 				LandmarkManager.Instance.CreateNewSettlement(hexTile.region, LOCATION_TYPE.DUNGEON, hexTile);	
// 			}
// 		}
// 		yield return null;
// 	}
// 	#endregion
// 	
// 	#region Saved World
// 	public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData) {
// 		// List<SaveDataHextile> landmarkTiles = saveData.worldMapSave.GetAllTilesWithLandmarks();
// 		// for (int i = 0; i < landmarkTiles.Count; i++) {
// 		// 	SaveDataHextile saveDataHextile = landmarkTiles[i];
// 		// 	HexTile hexTile = GridMap.Instance.map[saveDataHextile.xCoordinate, saveDataHextile.yCoordinate];
// 		// 	LandmarkManager.Instance.CreateNewLandmarkOnTile(hexTile, saveDataHextile.landmarkType);
// 		// 	hexTile.UpdateLandmarkVisuals();
// 		// }
// 		yield return null;
// 	}
// 	#endregion
// 	
// 	#region Utilities
// 	private bool IsAdjacentToPortal(HexTile tile) {
// 		for (int i = 0; i < tile.neighbourComponent.neighbours.Count; i++) {
// 			HexTile neighbour = tile.neighbourComponent.neighbours[i];
// 			if (neighbour.landmarkOnTile != null && neighbour.landmarkOnTile.specificLandmarkType == LANDMARK_TYPE.THE_PORTAL) {
// 				return true;
// 			}
// 		}
// 		return false;
// 	}
// 	private bool IsAdjacentToSettlement(HexTile tile) {
// 		for (int i = 0; i < tile.neighbourComponent.neighbours.Count; i++) {
// 			HexTile neighbour = tile.neighbourComponent.neighbours[i];
// 			if (neighbour.featureComponent.HasFeature(AreaFeatureDB.Inhabited_Feature)) {
// 				return true;
// 			}
// 		}
// 		return false;
// 	}
// 	private bool IsInRangeOfSettlement(Area tile, int range) {
// 		List<Area> tilesInRange = tile.GetTilesInRange(range);
// 		for (int i = 0; i < tilesInRange.Count; i++) {
// 			Area tileInRange = tilesInRange[i];
// 			if (tileInRange.featureComponent.HasFeature(AreaFeatureDB.Inhabited_Feature)) {
// 				return true;
// 			}
// 		}
// 		return false;
// 	}
// 	private bool IsAdjacentToSpecialStructure(Area p_area) {
// 		for (int i = 0; i < p_area.neighbourComponent.neighbours.Count; i++) {
// 			Area neighbour = p_area.neighbourComponent.neighbours[i];
// 			if (neighbour.landmarkOnTile != null && 
// 			    neighbour.landmarkOnTile.specificLandmarkType.GetStructureType().IsSpecialStructure()) {
// 				return true;
// 			}
// 		}
// 		return false;
// 	}
// 	#endregion
// }

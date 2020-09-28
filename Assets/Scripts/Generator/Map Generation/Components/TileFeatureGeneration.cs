using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Locations.Tile_Features;
using Scenario_Maps;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;

public class TileFeatureGeneration : MapGenerationComponent {

	#region Random World
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
		LevelLoaderManager.Instance.UpdateLoadingInfo("Generating tile features...");
		yield return MapGenerator.Instance.StartCoroutine(GenerateFeaturesForAllTiles(data));
		if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
			DetermineSettlementsForTutorial();
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
			DetermineSettlementsForSecondWorld();
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo) {
			DetermineSettlementsForPangatLoo();
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Zenko) {
			DetermineSettlementsForZenko();
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Affatt) {
			DetermineSettlementsForAffatt();
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa) {
			DetermineSettlementsForIcalawa();
		} else {
			yield return MapGenerator.Instance.StartCoroutine(ComputeHabitabilityValues(data));
			succeess = TryCreateSettlements(data);
		}
		
	}
	private IEnumerator GenerateFeaturesForAllTiles(MapGenerationData data) {
		List<HexTile> flatTilesWithNoFeatures = new List<HexTile>();
		int batchCount = 0;
		for (int x = 0; x < GridMap.Instance.width; x++) {
			for (int y = 0; y < GridMap.Instance.height; y++) {
				HexTile tile = GridMap.Instance.map[x, y];
				if (tile.elevationType == ELEVATION.TREES) {
					tile.featureComponent.AddFeature(TileFeatureDB.Wood_Source_Feature, tile);
				} else if (tile.elevationType == ELEVATION.MOUNTAIN) {
					tile.featureComponent.AddFeature(TileFeatureDB.Metal_Source_Feature, tile);	
				} else if (tile.elevationType == ELEVATION.PLAIN && tile.featureComponent.features.Count == 0) {
					flatTilesWithNoFeatures.Add(tile);	
				}
				batchCount++;
				if (batchCount >= MapGenerationData.WorldMapFeatureGenerationBatches) {
					batchCount = 0;
					yield return null;
				}
			}	
		}

		int stoneSourceCount = GetStoneSourceToGenerate(data.regionCount);
		int fertileCount = GetFertileToGenerate(data.regionCount);
		int gameCount = GetGameToGenerate(data.regionCount);

		//stone source
		for (int i = 0; i < stoneSourceCount; i++) {
			if (flatTilesWithNoFeatures.Count <= 0) { break; }
			HexTile tile = CollectionUtilities.GetRandomElement(flatTilesWithNoFeatures);
			tile.featureComponent.AddFeature(TileFeatureDB.Stone_Source_Feature, tile);
			flatTilesWithNoFeatures.Remove(tile);
		}		
		
		yield return null;
		
		//fertile
		for (int i = 0; i < fertileCount; i++) {
			if (flatTilesWithNoFeatures.Count <= 0) { break; }
			HexTile tile = CollectionUtilities.GetRandomElement(flatTilesWithNoFeatures);
			tile.featureComponent.AddFeature(TileFeatureDB.Fertile_Feature, tile);
			flatTilesWithNoFeatures.Remove(tile);
		}
		
		yield return null;
		
		if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
			//pigs
			HexTile pigTile = GridMap.Instance.map[2, 4];
			GameFeature pigGameFeature = LandmarkManager.Instance.CreateTileFeature<GameFeature>(TileFeatureDB.Game_Feature);
			pigGameFeature.SetSpawnType(SUMMON_TYPE.Pig);
			pigTile.featureComponent.AddFeature(pigGameFeature, pigTile);
			
			//sheep
			HexTile sheepTile = GridMap.Instance.map[4, 3];
			GameFeature sheepGameFeature = LandmarkManager.Instance.CreateTileFeature<GameFeature>(TileFeatureDB.Game_Feature);
			sheepGameFeature.SetSpawnType(SUMMON_TYPE.Sheep);
			sheepTile.featureComponent.AddFeature(sheepGameFeature, sheepTile);
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
			//Add 2 pigs 2 tiles away from village 
			HexTile pigTile = GridMap.Instance.map[5, 5];
			GameFeature pigGameFeature = LandmarkManager.Instance.CreateTileFeature<GameFeature>(TileFeatureDB.Game_Feature);
			pigGameFeature.SetSpawnType(SUMMON_TYPE.Pig);
			pigTile.featureComponent.AddFeature(pigGameFeature, pigTile);
			pigTile.SetElevation(ELEVATION.PLAIN);
		} else {
			List<HexTile> gameChoices = GridMap.Instance.normalHexTiles.Where(h =>
				h.elevationType == ELEVATION.PLAIN || h.elevationType == ELEVATION.TREES).ToList();
			for (int i = 0; i < gameCount; i++) {
				if (gameChoices.Count <= 0) { break; }
				HexTile tile = CollectionUtilities.GetRandomElement(gameChoices);
				tile.featureComponent.AddFeature(TileFeatureDB.Game_Feature, tile);
				gameChoices.Remove(tile);
			}	
		}
	}
	private IEnumerator ComputeHabitabilityValues(MapGenerationData data) {
		data.habitabilityValues = new int[data.width, data.height];
		
		int batchCount = 0;
		for (int x = 0; x < data.width; x++) {
			for (int y = 0; y < data.height; y++) {
				HexTile tile = GridMap.Instance.map[x, y];
				int habitability = 0;
				if (tile.elevationType == ELEVATION.WATER || tile.elevationType == ELEVATION.MOUNTAIN) {
					habitability = 0;
				} else {
					int adjacentWaterTiles = 0;
					int adjacentFlatTiles = 0;
					for (int i = 0; i < tile.AllNeighbours.Count; i++) {
						HexTile neighbour = tile.AllNeighbours[i];
						if (neighbour.region != tile.region) {
							continue; //do not include neighbour if part of another region
						}
						if (neighbour.elevationType == ELEVATION.PLAIN) {
							adjacentFlatTiles += 1;
						} else if (neighbour.elevationType == ELEVATION.WATER) {
							adjacentWaterTiles += 1;
						}

						if (tile.biomeType == BIOMES.FOREST || tile.biomeType == BIOMES.SNOW) {
							if (neighbour.featureComponent.HasFeature(TileFeatureDB.Wood_Source_Feature)) {
								habitability += 3;
							}	
							if (neighbour.featureComponent.HasFeature(TileFeatureDB.Metal_Source_Feature)) {
								habitability += 4;
							}
							if (neighbour.featureComponent.HasFeature(TileFeatureDB.Fertile_Feature)) {
								habitability += 5;
							}
						} else if (tile.biomeType == BIOMES.GRASSLAND || tile.biomeType == BIOMES.DESERT) {
							if (neighbour.featureComponent.HasFeature(TileFeatureDB.Stone_Source_Feature)) {
								habitability += 3;
							}
							if (neighbour.featureComponent.HasFeature(TileFeatureDB.Metal_Source_Feature)) {
								habitability += 4;
							}
							if (neighbour.featureComponent.HasFeature(TileFeatureDB.Game_Feature)) {
								habitability += 5;
							}
						}
					}
					if (adjacentWaterTiles == 1) {
						habitability += 5;
					}
					if (adjacentFlatTiles < 2) {
						habitability -= 10;
					}
				}
				data.habitabilityValues[x, y] = habitability;
				batchCount++;
				if (batchCount >= MapGenerationData.WorldMapHabitabilityGenerationBatches) {
					batchCount = 0;
					yield return null;
				}
			}	
		}
	}
	private bool TryCreateSettlements(MapGenerationData data) {
		int createdSettlements = 0;
		int chanceToCreateSettlement = 100;
		for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
			Region region = GridMap.Instance.allRegions[i];
			if (IsSettlementPossibleOnRegion(region)) {
				HexTile highestHabitabilityTile = GetTileWithHighestHabitability(region, data);
				if (highestHabitabilityTile == null) {
					continue;
				}
				List<HexTile> habitableNeighbours = highestHabitabilityTile.AllNeighbours.Where(t => t.region == region && data.GetHabitabilityValue(t) > 0).ToList();
				if (habitableNeighbours.Count >= 2) {
					if (GameUtilities.RollChance(chanceToCreateSettlement)) {
						List<HexTile> villageTiles = new List<HexTile>();
						for (int j = 0; j < 3; j++) {
							if (habitableNeighbours.Count == 0) { break; }
							HexTile habitableNeighbour = CollectionUtilities.GetRandomElement(habitableNeighbours);
							villageTiles.Add(habitableNeighbour);
							habitableNeighbours.Remove(habitableNeighbour);
						}
						villageTiles.Add(highestHabitabilityTile);
				
						for (int j = 0; j < villageTiles.Count; j++) {
							HexTile villageTile = villageTiles[j];
							villageTile.featureComponent.AddFeature(TileFeatureDB.Inhabited_Feature, villageTile);
							//remove game feature from settlement tiles
							villageTile.featureComponent.RemoveFeature(TileFeatureDB.Game_Feature, villageTile);
							// LandmarkManager.Instance.CreateNewLandmarkOnTile(villageTile, LANDMARK_TYPE.VILLAGE);
						}
						createdSettlements++;
						//when a settlement is built, reduce chance by 10% for the next loop
						chanceToCreateSettlement -= 15;
					}
				}	
			}
		}
		return createdSettlements > 0;
	}
	#endregion

	#region Tile Feature Utilities
	private int GetStoneSourceToGenerate(int regionCount) {
		switch (regionCount) {
			case 1:
				return 1;
			case 2:
			case 3:
				return 2;
			case 4:
			case 5:
			case 6:
				return 3;
			default:
				return 3;
		}
	}
	private int GetFertileToGenerate(int regionCount) {
		switch (regionCount) {
			case 1:
				return 1;
			case 2:
			case 3:
				return 2;
			case 4:
			case 5:
			case 6:
				return 4;
			default:
				return 3;
		}
	}
	private int GetGameToGenerate(int regionCount) {
		switch (regionCount) {
			case 1:
				return 1;
			case 2:
			case 3:
				return 2;
			case 4:
			case 5:
			case 6:
				return 3;
			default:
				return 3;
		}
	}
	#endregion

	#region Settlement Generation Utilities
	private bool IsSettlementPossibleOnRegion(Region region) {
		//if region biome type is forest or snow, settlement is possible if world settings allow elves
		//if region biome type is desert or grassland, settlement is possible if world settings allow humans 
		if (region.coreTile.biomeType == BIOMES.FOREST || region.coreTile.biomeType == BIOMES.SNOW) {
			return WorldSettings.Instance.worldSettingsData.races.Contains(RACE.ELVES);
		} else if (region.coreTile.biomeType == BIOMES.DESERT || region.coreTile.biomeType == BIOMES.GRASSLAND) {
			return WorldSettings.Instance.worldSettingsData.races.Contains(RACE.HUMANS);
		}
		return false;
	}
	private List<HexTile> GetNeighbouringTiles(List<HexTile> tiles) {
		List<HexTile> neighbouringTiles = new List<HexTile>();
		for (int i = 0; i < tiles.Count; i++) {
			HexTile tile = tiles[i];
			for (int j = 0; j < tile.AllNeighbours.Count; j++) {
				HexTile neighbour = tile.AllNeighbours[j];
				if (tiles.Contains(neighbour) == false && neighbouringTiles.Contains(neighbour) == false) {
					neighbouringTiles.Add(neighbour);
				}
			}
		}
		return neighbouringTiles;
	}
	private HexTile GetTileWithHighestHabitability(Region region, MapGenerationData data) {
		int highestHabitability = 0;
		HexTile tileWithHighestHabitability = null;
		for (int i = 0; i < region.tiles.Count; i++) {
			HexTile tile = region.tiles[i];
			int habitability = data.GetHabitabilityValue(tile);
			if (habitability > highestHabitability) {
				tileWithHighestHabitability = tile;
				highestHabitability = habitability;
			}
		}
		return tileWithHighestHabitability;
	}
	#endregion

	#region Scenario Maps
	public override IEnumerator LoadScenarioData(MapGenerationData data, ScenarioMapData scenarioMapData) {
		SaveDataHextile[,] savedMap = scenarioMapData.worldMapSave.GetSaveDataMap();
		for (int x = 0; x < data.width; x++) {
			for (int y = 0; y < data.height; y++) {
				SaveDataHextile savedHexTile = savedMap[x, y];
				HexTile hexTile = GridMap.Instance.map[x, y];
				if (savedHexTile.tileFeatureSaveData?.Count > 0) {
					for (int i = 0; i < savedHexTile.tileFeatureSaveData.Count; i++) {
						SaveDataTileFeature saveDataTileFeature = savedHexTile.tileFeatureSaveData[i];
						TileFeature tileFeature = saveDataTileFeature.Load();
						hexTile.featureComponent.AddFeature(tileFeature, hexTile);
					}
				}
				yield return null;
			}
		}
	}
	private void DetermineSettlementsForTutorial() {
		List<HexTile> chosenTiles = new List<HexTile> {
			GridMap.Instance.map[4, 5],
			GridMap.Instance.map[5, 5],
			GridMap.Instance.map[4, 6],
			GridMap.Instance.map[3, 5],
		};
	
		for (int i = 0; i < chosenTiles.Count; i++) {
			HexTile chosenTile = chosenTiles[i];
			chosenTile.SetElevation(ELEVATION.PLAIN);
			chosenTile.featureComponent.RemoveAllFeatures(chosenTile);
			chosenTile.featureComponent.AddFeature(TileFeatureDB.Inhabited_Feature, chosenTile);
			LandmarkManager.Instance.CreateNewLandmarkOnTile(chosenTile, LANDMARK_TYPE.VILLAGE);
		}
		
		List<HexTile> neighbouringTiles = GetNeighbouringTiles(chosenTiles);
		//if settlement is not adjacent to any water hex tile create one
		if (neighbouringTiles.Any(h => h.elevationType == ELEVATION.WATER) == false) {
			HexTile randomTile = CollectionUtilities.GetRandomElement(neighbouringTiles);
			randomTile.SetElevation(ELEVATION.WATER);
			randomTile.featureComponent.RemoveAllFeatures(randomTile);
		}
	}
	private void DetermineSettlementsForSecondWorld() {
		List<HexTile> chosenTiles = new List<HexTile> {
			GridMap.Instance.map[8, 5],
			GridMap.Instance.map[7, 5],
			GridMap.Instance.map[8, 6],
			GridMap.Instance.map[9, 5],
			GridMap.Instance.map[8, 4],
		};
	
		for (int i = 0; i < chosenTiles.Count; i++) {
			HexTile chosenTile = chosenTiles[i];
			chosenTile.SetElevation(ELEVATION.PLAIN);
			chosenTile.featureComponent.RemoveAllFeatures(chosenTile);
			chosenTile.featureComponent.AddFeature(TileFeatureDB.Inhabited_Feature, chosenTile);
		}
		
		// List<HexTile> neighbouringTiles = GetNeighbouringTiles(chosenTiles);
		// for (int i = 0; i < neighbouringTiles.Count; i++) {
		// 	HexTile neighbour = neighbouringTiles[i];
		// 	if (i == 0) {
		// 		neighbour.SetElevation(ELEVATION.PLAIN);
		// 	} else {
		// 		neighbour.SetElevation(ELEVATION.MOUNTAIN);
		// 	}
		// }
	}
	private void DetermineSettlementsForZenko() {
		List<HexTile> chosenTiles = new List<HexTile> {
			//region 1 (snow)
			GridMap.Instance.map[5, 9],
			GridMap.Instance.map[4, 9],
			GridMap.Instance.map[5, 8],
			GridMap.Instance.map[4, 8],
			// GridMap.Instance.map[4, 8],
			//region 2 (grassland)
			GridMap.Instance.map[8, 7],
			GridMap.Instance.map[8, 8],
			GridMap.Instance.map[9, 7],
			GridMap.Instance.map[9, 8],
			// GridMap.Instance.map[7, 7],
			//region 3 (forest)
			GridMap.Instance.map[1, 2],
			GridMap.Instance.map[2, 2],
			GridMap.Instance.map[3, 2],
			GridMap.Instance.map[2, 3],
			// GridMap.Instance.map[3, 3],
			//region 4 (desert)
			GridMap.Instance.map[10, 4],
			GridMap.Instance.map[9, 5],
			GridMap.Instance.map[8, 4],
			GridMap.Instance.map[9, 4],
			// GridMap.Instance.map[9, 3],
		};

		for (int i = 0; i < chosenTiles.Count; i++) {
			HexTile chosenTile = chosenTiles[i];
			chosenTile.SetElevation(ELEVATION.PLAIN);
			chosenTile.featureComponent.RemoveAllFeatures(chosenTile);
			chosenTile.featureComponent.AddFeature(TileFeatureDB.Inhabited_Feature, chosenTile);
		}
		
		// List<HexTile> neighbouringTiles = GetNeighbouringTiles(chosenTiles);
		// for (int i = 0; i < neighbouringTiles.Count; i++) {
		// 	HexTile neighbour = neighbouringTiles[i];
		// 	if (i == 0) {
		// 		neighbour.SetElevation(ELEVATION.PLAIN);
		// 	} else {
		// 		neighbour.SetElevation(ELEVATION.MOUNTAIN);
		// 	}
		// }
	}
	private void DetermineSettlementsForPangatLoo() {
		List<HexTile> chosenTiles = new List<HexTile> {
			//region 1 (grassland)
			GridMap.Instance.map[0, 1],
			GridMap.Instance.map[1, 2],
			GridMap.Instance.map[1, 3],
			GridMap.Instance.map[1, 4],
			GridMap.Instance.map[0, 2],
			GridMap.Instance.map[2, 2],
			GridMap.Instance.map[2, 3],
			GridMap.Instance.map[2, 4],
			GridMap.Instance.map[0, 3],
		};

		for (int i = 0; i < chosenTiles.Count; i++) {
			HexTile chosenTile = chosenTiles[i];
			chosenTile.SetElevation(ELEVATION.PLAIN);
			chosenTile.featureComponent.RemoveAllFeatures(chosenTile);
			chosenTile.featureComponent.AddFeature(TileFeatureDB.Inhabited_Feature, chosenTile);
		}
	}
	private void DetermineSettlementsForAffatt() {
		List<HexTile> chosenTiles = new List<HexTile> {
			//region 1 (big forest region)
			//first settlement
			GridMap.Instance.map[1, 2],
			GridMap.Instance.map[1, 3],
			GridMap.Instance.map[2, 3],
			GridMap.Instance.map[2, 2],
			//second settlement
			GridMap.Instance.map[7, 2],
			GridMap.Instance.map[7, 3],
			GridMap.Instance.map[6, 1],
			GridMap.Instance.map[5, 3],
			GridMap.Instance.map[6, 2],
			//region 4 (snow region)
			GridMap.Instance.map[3, 6],
			GridMap.Instance.map[2, 6],
			GridMap.Instance.map[2, 7],
			GridMap.Instance.map[3, 7],
			GridMap.Instance.map[4, 6],
		};

		for (int i = 0; i < chosenTiles.Count; i++) {
			HexTile chosenTile = chosenTiles[i];
			chosenTile.SetElevation(ELEVATION.PLAIN);
			chosenTile.featureComponent.RemoveAllFeatures(chosenTile);
			chosenTile.featureComponent.AddFeature(TileFeatureDB.Inhabited_Feature, chosenTile);
		}
		
		// List<HexTile> neighbouringTiles = GetNeighbouringTiles(chosenTiles);
		// for (int i = 0; i < neighbouringTiles.Count; i++) {
		// 	HexTile neighbour = neighbouringTiles[i];
		// 	if (i == 0) {
		// 		neighbour.SetElevation(ELEVATION.PLAIN);
		// 	} else {
		// 		neighbour.SetElevation(ELEVATION.MOUNTAIN);
		// 	}
		// }
	}
	private void DetermineSettlementsForIcalawa() {
		List<HexTile> chosenTiles = new List<HexTile> {
			GridMap.Instance.map[11, 1],
			GridMap.Instance.map[11, 2],
			GridMap.Instance.map[11, 3],
			GridMap.Instance.map[10, 2],
		};

		for (int i = 0; i < chosenTiles.Count; i++) {
			HexTile chosenTile = chosenTiles[i];
			chosenTile.SetElevation(ELEVATION.PLAIN);
			chosenTile.featureComponent.RemoveAllFeatures(chosenTile);
			chosenTile.featureComponent.AddFeature(TileFeatureDB.Inhabited_Feature, chosenTile);
		}
	}
	#endregion
	
	#region Saved World
	public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData) {
		SaveDataHextile[,] savedMap = saveData.worldMapSave.GetSaveDataMap();
		for (int x = 0; x < data.width; x++) {
			for (int y = 0; y < data.height; y++) {
				SaveDataHextile savedHexTile = savedMap[x, y];
				HexTile hexTile = GridMap.Instance.map[x, y];
				if (savedHexTile.tileFeatureSaveData?.Count > 0) {
					for (int i = 0; i < savedHexTile.tileFeatureSaveData.Count; i++) {
						SaveDataTileFeature saveDataTileFeature = savedHexTile.tileFeatureSaveData[i];
						TileFeature tileFeature = saveDataTileFeature.Load();
						hexTile.featureComponent.AddFeature(tileFeature, hexTile);
					}
				}
				yield return null;
			}
		}
	}
	#endregion
}

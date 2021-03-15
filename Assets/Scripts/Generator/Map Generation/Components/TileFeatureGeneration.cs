using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Area_Features;
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
			DetermineSettlementsForOona(data);
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo) {
			DetermineSettlementsForPangatLoo(data);
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Zenko) {
			DetermineSettlementsForZenko(data);
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Affatt) {
			DetermineSettlementsForAffatt(data);
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa) {
			DetermineSettlementsForIcalawa(data);
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Aneem) {
			DetermineSettlementsForAneem(data);
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pitto) {
			DetermineSettlementsForPitto(data);
		} else {
			yield return MapGenerator.Instance.StartCoroutine(ComputeHabitabilityValues(data));
			yield return MapGenerator.Instance.StartCoroutine(DetermineVillageSpots(data));
			succeess = TryAssignSettlementTiles(data);
		}
		
	}
	private IEnumerator GenerateFeaturesForAllTiles(MapGenerationData data) {
		List<Area> flatTilesWithNoFeatures = new List<Area>();
		int batchCount = 0;
		for (int x = 0; x < GridMap.Instance.width; x++) {
			for (int y = 0; y < GridMap.Instance.height; y++) {
				Area tile = GridMap.Instance.map[x, y];
				if (tile.elevationType == ELEVATION.TREES) {
					tile.featureComponent.AddFeature(AreaFeatureDB.Wood_Source_Feature, tile);
				} else if (tile.elevationType == ELEVATION.MOUNTAIN) {
					tile.featureComponent.AddFeature(AreaFeatureDB.Metal_Source_Feature, tile);	
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

		int stoneSourceCount = GetStoneSourceToGenerate(data.chosenWorldMapTemplate.regionCount);
		int fertileCount = GetFertileToGenerate(data.chosenWorldMapTemplate.regionCount);
		int gameCount = GetGameToGenerate(data.chosenWorldMapTemplate.regionCount);

		//stone source
		for (int i = 0; i < stoneSourceCount; i++) {
			if (flatTilesWithNoFeatures.Count <= 0) { break; }
			Area tile = CollectionUtilities.GetRandomElement(flatTilesWithNoFeatures);
			tile.featureComponent.AddFeature(AreaFeatureDB.Stone_Source_Feature, tile);
			flatTilesWithNoFeatures.Remove(tile);
			Debug.Log($"Added stone source feature to {tile}");
		}		
		
		yield return null;
		
		//fertile
		for (int i = 0; i < fertileCount; i++) {
			if (flatTilesWithNoFeatures.Count <= 0) { break; }
			Area tile = CollectionUtilities.GetRandomElement(flatTilesWithNoFeatures);
			tile.featureComponent.AddFeature(AreaFeatureDB.Fertile_Feature, tile);
			flatTilesWithNoFeatures.Remove(tile);
		}
		
		yield return null;
		
		if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
			//pigs
			Area pigTile = GridMap.Instance.map[2, 4];
			GameFeature pigGameFeature = LandmarkManager.Instance.CreateAreaFeature<GameFeature>(AreaFeatureDB.Game_Feature);
			pigGameFeature.SetSpawnType(SUMMON_TYPE.Pig);
			pigTile.featureComponent.AddFeature(pigGameFeature, pigTile);
			
			//sheep
			Area sheepTile = GridMap.Instance.map[4, 3];
			GameFeature sheepGameFeature = LandmarkManager.Instance.CreateAreaFeature<GameFeature>(AreaFeatureDB.Game_Feature);
			sheepGameFeature.SetSpawnType(SUMMON_TYPE.Sheep);
			sheepTile.featureComponent.AddFeature(sheepGameFeature, sheepTile);
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
			//Add 2 pigs 2 tiles away from village 
			Area pigTile = GridMap.Instance.map[5, 5];
			GameFeature pigGameFeature = LandmarkManager.Instance.CreateAreaFeature<GameFeature>(AreaFeatureDB.Game_Feature);
			pigGameFeature.SetSpawnType(SUMMON_TYPE.Pig);
			pigTile.featureComponent.AddFeature(pigGameFeature, pigTile);
			pigTile.SetElevation(ELEVATION.PLAIN);
		} else {
			List<Area> gameChoices = GridMap.Instance.allAreas.Where(h =>
				h.elevationType == ELEVATION.PLAIN || h.elevationType == ELEVATION.TREES).ToList();
			for (int i = 0; i < gameCount; i++) {
				if (gameChoices.Count <= 0) { break; }
				Area tile = CollectionUtilities.GetRandomElement(gameChoices);
				tile.featureComponent.AddFeature(AreaFeatureDB.Game_Feature, tile);
				gameChoices.Remove(tile);
			}	
		}
	}
	private IEnumerator ComputeHabitabilityValues(MapGenerationData data) {
		data.habitabilityValues = new int[data.width, data.height];
		
		int batchCount = 0;
		for (int x = 0; x < data.width; x++) {
			for (int y = 0; y < data.height; y++) {
				Area area = GridMap.Instance.map[x, y];
				int habitability = 0;
				if (area.elevationType == ELEVATION.WATER || area.elevationType == ELEVATION.MOUNTAIN || 
				    area.gridTileComponent.gridTiles.Any(t => t.elevationType == ELEVATION.WATER || t.elevationType == ELEVATION.MOUNTAIN)) {
					habitability = 0;
				} else {
					int adjacentWaterTiles = 0;
					int adjacentFlatTiles = 0;
					habitability += 1;
					for (int i = 0; i < area.neighbourComponent.neighbours.Count; i++) {
						Area neighbour = area.neighbourComponent.neighbours[i];
						if (neighbour.region != area.region) {
							continue; //do not include neighbour if part of another region
						}
						if (neighbour.elevationType == ELEVATION.PLAIN) {
							adjacentFlatTiles += 1;
						} else if (neighbour.elevationType == ELEVATION.WATER) {
							adjacentWaterTiles += 1;
						}

						if (neighbour.featureComponent.HasFeature(AreaFeatureDB.Wood_Source_Feature)) {
							habitability += 3;
						}	
						if (neighbour.featureComponent.HasFeature(AreaFeatureDB.Metal_Source_Feature)) {
							habitability += 4;
						}
						if (neighbour.featureComponent.HasFeature(AreaFeatureDB.Fertile_Feature)) {
							habitability += 5;
						}
						if (neighbour.featureComponent.HasFeature(AreaFeatureDB.Stone_Source_Feature)) {
							habitability += 3;
						}
						if (neighbour.featureComponent.HasFeature(AreaFeatureDB.Game_Feature)) {
							habitability += 5;
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
	private IEnumerator DetermineVillageSpots(MapGenerationData p_data) {
		for (int x = 0; x < p_data.width; x++) {
			for (int y = 0; y < p_data.height; y++) {
				Area currentTile = GridMap.Instance.map[x, y];
				int currentTileHabitability = p_data.GetHabitabilityValue(currentTile);
				if (currentTileHabitability >= MapGenerationData.MinimumHabitabilityForVillage) {
					p_data.AddVillageSpot(currentTile);
					// int adjacentHabitable = 0;
					// for (int i = 0; i < currentTile.neighbourComponent.neighbours.Count; i++) {
					// 	Area neighbour = currentTile.neighbourComponent.neighbours[i];
					// 	int habitability = p_data.GetHabitabilityValue(neighbour);
					// 	if (habitability > 0) {
					// 		adjacentHabitable++;
					// 	}
					// }
					// if (adjacentHabitable >= 2) {
					// 	p_data.AddVillageSpot(currentTile);
					// }
				}
			}
		}
		Debug.Log($"Created {p_data.villageSpots.Count.ToString()} Village Spots");
		yield return null;
	}
	private bool TryAssignSettlementTiles(MapGenerationData data) {
		int createdVillages = 0;
		int villagesToCreate = WorldSettings.Instance.worldSettingsData.factionSettings.GetCurrentTotalVillageCountBasedOnFactions();
		if (data.villageSpots.Count < villagesToCreate) {
			//not enough village spots
			return false;
		}
		for (int i = 0; i < WorldSettings.Instance.worldSettingsData.factionSettings.factionTemplates.Count; i++) {
			FactionTemplate factionTemplate = WorldSettings.Instance.worldSettingsData.factionSettings.factionTemplates[i];
			for (int j = 0; j < factionTemplate.villageSettings.Count; j++) {
				if (data.villageSpots.Count == 0) {
					return false; //not enough village spots 
				}
				VillageSetting villageSetting = factionTemplate.villageSettings[j];
				int tilesInRange = villageSetting.GetTileCountReservedForVillage();
				Area chosenTile = null;
				if (j == 0) {
					//if no preferred tiles are available, then just choose at random from available village spots
					chosenTile = CollectionUtilities.GetRandomElement(data.villageSpots);
				} else {
					//if not first village pick a spot nearest to First Village
					float nearestDistance = Mathf.Infinity;
					Vector2 firstVillagePos = data.determinedVillages[factionTemplate][0].areaData.position;
					for (int k = 0; k < data.villageSpots.Count; k++) {
						Area villageSpot = data.villageSpots[k];
						Vector2 directionToTarget = villageSpot.areaData.position - firstVillagePos;
						float distance = directionToTarget.sqrMagnitude;
						if (distance < nearestDistance) {
							nearestDistance = distance;
							chosenTile = villageSpot;
						}
					}
				}
				Assert.IsNotNull(chosenTile, $"Could not find village spot for {factionTemplate.name}'s Village #{j.ToString()}");
				data.AddDeterminedVillage(factionTemplate, chosenTile);
				// for (int k = 0; k < chosenTile.gridTileComponent.gridTiles.Count; k++) {
				// 	LocationGridTile tile = chosenTile.gridTileComponent.gridTiles[k];
				// 	data.RemoveTileFromNonPlainElevationIslands(tile);
				// }
				chosenTile.featureComponent.AddFeature(AreaFeatureDB.Inhabited_Feature, chosenTile);
				//remove game feature from settlement tiles
				chosenTile.featureComponent.RemoveFeature(AreaFeatureDB.Game_Feature, chosenTile);
				//remove chosen tile and neighbours from choices.
				List<Area> neighbours = chosenTile.GetTilesInRange(tilesInRange, false);
				neighbours.Add(chosenTile);
				data.RemoveVillageSpots(neighbours);
				createdVillages++;
			}
		}
		return createdVillages == villagesToCreate;
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
	private List<Area> GetNeighbouringTiles(List<Area> tiles) {
		List<Area> neighbouringTiles = new List<Area>();
		for (int i = 0; i < tiles.Count; i++) {
			Area tile = tiles[i];
			for (int j = 0; j < tile.neighbourComponent.neighbours.Count; j++) {
				Area neighbour = tile.neighbourComponent.neighbours[j];
				if (tiles.Contains(neighbour) == false && neighbouringTiles.Contains(neighbour) == false) {
					neighbouringTiles.Add(neighbour);
				}
			}
		}
		return neighbouringTiles;
	}
	#endregion

	#region Scenario Maps
	public override IEnumerator LoadScenarioData(MapGenerationData data, ScenarioMapData scenarioMapData) {
		SaveDataArea[,] savedMap = scenarioMapData.worldMapSave.GetSaveDataMap();
		for (int x = 0; x < data.width; x++) {
			for (int y = 0; y < data.height; y++) {
				SaveDataArea savedHexTile = savedMap[x, y];
				Area hexTile = GridMap.Instance.map[x, y];
				if (savedHexTile.tileFeatureSaveData?.Count > 0) {
					for (int i = 0; i < savedHexTile.tileFeatureSaveData.Count; i++) {
						SaveDataAreaFeature saveDataTileFeature = savedHexTile.tileFeatureSaveData[i];
						AreaFeature tileFeature = saveDataTileFeature.Load();
						hexTile.featureComponent.AddFeature(tileFeature, hexTile);
					}
				}
				yield return null;
			}
		}
	}
	private void DetermineSettlementsForTutorial() {
		List<Area> chosenTiles = new List<Area> {
			GridMap.Instance.map[6, 5],
			GridMap.Instance.map[7, 5],
			GridMap.Instance.map[6, 6],
			GridMap.Instance.map[5, 5],
		};
	
		for (int i = 0; i < chosenTiles.Count; i++) {
			Area chosenTile = chosenTiles[i];
			chosenTile.SetElevation(ELEVATION.PLAIN);
			chosenTile.featureComponent.RemoveAllFeatures(chosenTile);
			chosenTile.featureComponent.AddFeature(AreaFeatureDB.Inhabited_Feature, chosenTile);
		}
		
		List<Area> neighbouringTiles = GetNeighbouringTiles(chosenTiles);
		//if settlement is not adjacent to any water hex tile create one
		if (neighbouringTiles.Any(h => h.elevationType == ELEVATION.WATER) == false) {
			Area randomTile = CollectionUtilities.GetRandomElement(neighbouringTiles);
			randomTile.SetElevation(ELEVATION.WATER);
			randomTile.featureComponent.RemoveAllFeatures(randomTile);
		}
	}
	private void DetermineSettlementsForOona(MapGenerationData data) {
		List<Area> chosenTiles = new List<Area> {
			GridMap.Instance.map[6, 5],
		};

		FactionTemplate factionTemplate = new FactionTemplate(1);
		factionTemplate.SetFactionEmblem(FactionEmblemRandomizer.GetUnusedFactionEmblem());
		
		for (int i = 0; i < chosenTiles.Count; i++) {
			Area chosenTile = chosenTiles[i];
			chosenTile.SetElevation(ELEVATION.PLAIN);
			chosenTile.featureComponent.RemoveAllFeatures(chosenTile);
			chosenTile.featureComponent.AddFeature(AreaFeatureDB.Inhabited_Feature, chosenTile);
			data.AddDeterminedVillage(factionTemplate, chosenTile);
		}
	}
	private void DetermineSettlementsForIcalawa(MapGenerationData data) {
		List<Area> chosenTiles = new List<Area> {
			GridMap.Instance.map[9, 2],
		};

		FactionTemplate factionTemplate = new FactionTemplate(1);
		factionTemplate.SetFactionEmblem(FactionEmblemRandomizer.GetUnusedFactionEmblem());
		
		for (int i = 0; i < chosenTiles.Count; i++) {
			Area chosenTile = chosenTiles[i];
			chosenTile.SetElevation(ELEVATION.PLAIN);
			chosenTile.featureComponent.RemoveAllFeatures(chosenTile);
			chosenTile.featureComponent.AddFeature(AreaFeatureDB.Inhabited_Feature, chosenTile);
			data.AddDeterminedVillage(factionTemplate, chosenTile);
		}
	}
	private void DetermineSettlementsForPangatLoo(MapGenerationData data) {
		List<Area> chosenTiles = new List<Area> {
			GridMap.Instance.map[2, 3],
		};

		FactionTemplate factionTemplate = new FactionTemplate(1);
		factionTemplate.SetFactionEmblem(FactionEmblemRandomizer.GetUnusedFactionEmblem());
		
		for (int i = 0; i < chosenTiles.Count; i++) {
			Area chosenTile = chosenTiles[i];
			chosenTile.SetElevation(ELEVATION.PLAIN);
			chosenTile.featureComponent.RemoveAllFeatures(chosenTile);
			chosenTile.featureComponent.AddFeature(AreaFeatureDB.Inhabited_Feature, chosenTile);
			data.AddDeterminedVillage(factionTemplate, chosenTile);
		}
	}
	private void DetermineSettlementsForAffatt(MapGenerationData data) {
		List<Area> chosenTiles = new List<Area> {
			GridMap.Instance.map[1, 2],
			GridMap.Instance.map[3, 8],
			GridMap.Instance.map[8, 3],
		};

		FactionTemplate factionTemplate1 = new FactionTemplate(2);
		factionTemplate1.SetFactionEmblem(FactionEmblemRandomizer.GetUnusedFactionEmblem());
		
		FactionTemplate factionTemplate2 = new FactionTemplate(1);
		factionTemplate2.SetFactionEmblem(FactionEmblemRandomizer.GetUnusedFactionEmblem());
		
		for (int i = 0; i < chosenTiles.Count; i++) {
			Area chosenTile = chosenTiles[i];
			chosenTile.SetElevation(ELEVATION.PLAIN);
			chosenTile.featureComponent.RemoveAllFeatures(chosenTile);
			chosenTile.featureComponent.AddFeature(AreaFeatureDB.Inhabited_Feature, chosenTile);
			if (i == 0 || i == 1) {
				data.AddDeterminedVillage(factionTemplate1, chosenTile);
			} else {
				data.AddDeterminedVillage(factionTemplate2, chosenTile);
			}
		}
	}
	private void DetermineSettlementsForZenko(MapGenerationData data) {
		List<Area> chosenTiles = new List<Area> {
			//region 1 (snow)
			GridMap.Instance.map[4, 8],
			//region 2 (grassland)
			GridMap.Instance.map[8, 2],
			// GridMap.Instance.map[7, 7],
			//region 3 (forest)
			GridMap.Instance.map[1, 2],
			// GridMap.Instance.map[3, 3],
			//region 4 (desert)
			GridMap.Instance.map[10, 8],
			// GridMap.Instance.map[9, 3],
		};

		FactionTemplate factionTemplate1 = new FactionTemplate(1);
		factionTemplate1.SetFactionEmblem(FactionEmblemRandomizer.GetUnusedFactionEmblem());
		
		FactionTemplate factionTemplate2 = new FactionTemplate(1);
		factionTemplate2.SetFactionEmblem(FactionEmblemRandomizer.GetUnusedFactionEmblem());
		
		FactionTemplate factionTemplate3 = new FactionTemplate(1);
		factionTemplate3.SetFactionEmblem(FactionEmblemRandomizer.GetUnusedFactionEmblem());
		
		FactionTemplate factionTemplate4 = new FactionTemplate(1);
		factionTemplate4.SetFactionEmblem(FactionEmblemRandomizer.GetUnusedFactionEmblem());
		
		for (int i = 0; i < chosenTiles.Count; i++) {
			Area chosenTile = chosenTiles[i];
			chosenTile.SetElevation(ELEVATION.PLAIN);
			chosenTile.featureComponent.RemoveAllFeatures(chosenTile);
			chosenTile.featureComponent.AddFeature(AreaFeatureDB.Inhabited_Feature, chosenTile);
			if (i == 0) {
				data.AddDeterminedVillage(factionTemplate1, chosenTile);
			} else if (i == 1) {
				data.AddDeterminedVillage(factionTemplate2, chosenTile);
			} else if (i == 2) {
				data.AddDeterminedVillage(factionTemplate3, chosenTile);
			} else {
				data.AddDeterminedVillage(factionTemplate4, chosenTile);
			}
		}
	}
	private void DetermineSettlementsForAneem(MapGenerationData data) {
		List<Area> chosenTiles = new List<Area> {
			GridMap.Instance.map[2, 5],
			GridMap.Instance.map[12, 2],
		};

		FactionTemplate factionTemplate1 = new FactionTemplate(1);
		factionTemplate1.SetFactionEmblem(FactionEmblemRandomizer.GetUnusedFactionEmblem());
		
		FactionTemplate factionTemplate2 = new FactionTemplate(1);
		factionTemplate2.SetFactionEmblem(FactionEmblemRandomizer.GetUnusedFactionEmblem());
		
		for (int i = 0; i < chosenTiles.Count; i++) {
			Area chosenTile = chosenTiles[i];
			chosenTile.SetElevation(ELEVATION.PLAIN);
			chosenTile.featureComponent.RemoveAllFeatures(chosenTile);
			chosenTile.featureComponent.AddFeature(AreaFeatureDB.Inhabited_Feature, chosenTile);
			if (i == 0) {
				data.AddDeterminedVillage(factionTemplate1, chosenTile);
			} else {
				data.AddDeterminedVillage(factionTemplate2, chosenTile);
			}
		}
	}
	private void DetermineSettlementsForPitto(MapGenerationData data) {
		List<Area> chosenTiles = new List<Area> {
			GridMap.Instance.map[4, 5],
			GridMap.Instance.map[8, 5],
		};

		FactionTemplate factionTemplate1 = new FactionTemplate(1);
		factionTemplate1.SetFactionEmblem(FactionEmblemRandomizer.GetUnusedFactionEmblem());
		
		FactionTemplate factionTemplate2 = new FactionTemplate(1);
		factionTemplate2.SetFactionEmblem(FactionEmblemRandomizer.GetUnusedFactionEmblem());
		
		for (int i = 0; i < chosenTiles.Count; i++) {
			Area chosenTile = chosenTiles[i];
			chosenTile.SetElevation(ELEVATION.PLAIN);
			chosenTile.featureComponent.RemoveAllFeatures(chosenTile);
			chosenTile.featureComponent.AddFeature(AreaFeatureDB.Inhabited_Feature, chosenTile);
			if (i == 0) {
				data.AddDeterminedVillage(factionTemplate1, chosenTile);
			} else {
				data.AddDeterminedVillage(factionTemplate2, chosenTile);
			}
		}
	}
	#endregion
	
	#region Saved World
	public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData) {
		SaveDataArea[,] savedMap = saveData.worldMapSave.GetSaveDataMap();
		for (int x = 0; x < data.width; x++) {
			for (int y = 0; y < data.height; y++) {
				SaveDataArea savedHexTile = savedMap[x, y];
				Area hexTile = GridMap.Instance.map[x, y];
				if (savedHexTile.tileFeatureSaveData?.Count > 0) {
					for (int i = 0; i < savedHexTile.tileFeatureSaveData.Count; i++) {
						SaveDataAreaFeature saveDataTileFeature = savedHexTile.tileFeatureSaveData[i];
						AreaFeature tileFeature = saveDataTileFeature.Load();
						hexTile.featureComponent.AddFeature(tileFeature, hexTile);
					}
				}
				yield return null;
			}
		}
	}
	#endregion
}

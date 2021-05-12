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
		LevelLoaderManager.Instance.UpdateLoadingInfo("Generating Tile Features...");
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
			// yield return MapGenerator.Instance.StartCoroutine(ComputeHabitabilityValues(data));
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
				if (tile.elevationType == ELEVATION.PLAIN) {
					if (GameUtilities.RollChance(30)) {
						tile.featureComponent.AddFeature(AreaFeatureDB.Wood_Source_Feature, tile);	
					} else if (tile.featureComponent.features.Count == 0) {
						flatTilesWithNoFeatures.Add(tile);	
					}
				} else if (tile.elevationType == ELEVATION.MOUNTAIN) {
					tile.featureComponent.AddFeature(AreaFeatureDB.Metal_Source_Feature, tile);	
				}
				batchCount++;
				if (batchCount >= MapGenerationData.WorldMapFeatureGenerationBatches) {
					batchCount = 0;
					yield return null;
				}
			}	
		}

		int stoneSourceCount = Random.Range(1, 4);
		int fertileCount = Random.Range(1, 4);
		int gameCount = Random.Range(1, 4);
		int poisonVentsCount = Random.Range(0, 5);
		int vaporVentsCount = Random.Range(0, 5);

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

		List<Area> ventChoices = RuinarchListPool<Area>.Claim();
		ventChoices.AddRange(GridMap.Instance.allAreas);
		//poison vents
		for (int i = 0; i < poisonVentsCount; i++) {
			if (ventChoices.Count == 0) { break; }
			Area tile = CollectionUtilities.GetRandomElement(ventChoices);
			tile.featureComponent.AddFeature(AreaFeatureDB.Poison_Vents, tile);
			ventChoices.Remove(tile);
		}
		//vapor vents
		for (int i = 0; i < vaporVentsCount; i++) {
			if (ventChoices.Count == 0) { break; }
			Area tile = CollectionUtilities.GetRandomElement(ventChoices);
			tile.featureComponent.AddFeature(AreaFeatureDB.Vapor_Vents, tile);
			ventChoices.Remove(tile);
		}
		
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
			List<Area> gameChoices = GridMap.Instance.allAreas.Where(h => h.elevationType == ELEVATION.PLAIN).ToList();
			for (int i = 0; i < gameCount; i++) {
				if (gameChoices.Count <= 0) { break; }
				Area tile = CollectionUtilities.GetRandomElement(gameChoices);
				tile.featureComponent.AddFeature(AreaFeatureDB.Game_Feature, tile);
				gameChoices.Remove(tile);
			}	
		}
	}
	// private bool IsAreaAtCorner(Area p_area) {
	// 	if (p_area.areaData.xCoordinate == 0) {
	// 		if (p_area.areaData.yCoordinate == 0 || p_area.areaData.yCoordinate == GridMap.Instance.height - 1) {
	// 			return true;
	// 		}
	// 	} else if (p_area.areaData.xCoordinate == GridMap.Instance.width - 1) {
	// 		if (p_area.areaData.yCoordinate == 0 || p_area.areaData.yCoordinate == GridMap.Instance.height - 1) {
	// 			return true;
	// 		}
	// 	}
	// 	return false;
	// }
	// private IEnumerator ComputeHabitabilityValues(MapGenerationData data) {
	// 	data.habitabilityValues = new int[data.width, data.height];
	// 	
	// 	int batchCount = 0;
	// 	for (int x = 0; x < data.width; x++) {
	// 		for (int y = 0; y < data.height; y++) {
	// 			Area area = GridMap.Instance.map[x, y];
	// 			int habitability = 0;
	// 			bool isAtCorner = IsAreaAtCorner(area);
	// 			bool isFullyPlain = area.elevationComponent.IsFully(ELEVATION.PLAIN);
	// 			if (isFullyPlain && !isAtCorner) {
	// 				int adjacentWaterTiles = 0;
	// 				int adjacentFlatTiles = 0;
	// 				int adjacentCaveTiles = 0;
	// 				habitability += 1;
	// 				for (int i = 0; i < area.neighbourComponent.neighbours.Count; i++) {
	// 					Area neighbour = area.neighbourComponent.neighbours[i];
	// 					if (neighbour.region != area.region) {
	// 						continue; //do not include neighbour if part of another region
	// 					}
	// 					if (neighbour.elevationType == ELEVATION.PLAIN) {
	// 						adjacentFlatTiles += 1;
	// 					} else if (neighbour.elevationType == ELEVATION.WATER) {
	// 						adjacentWaterTiles += 1;
	// 					} else if (neighbour.elevationType == ELEVATION.MOUNTAIN) {
	// 						adjacentCaveTiles += 1;
	// 					}
	//
	// 					if (neighbour.featureComponent.HasFeature(AreaFeatureDB.Wood_Source_Feature)) {
	// 						habitability += 3;
	// 					}	
	// 					if (neighbour.featureComponent.HasFeature(AreaFeatureDB.Metal_Source_Feature)) {
	// 						habitability += 4;
	// 					}
	// 					if (neighbour.featureComponent.HasFeature(AreaFeatureDB.Fertile_Feature)) {
	// 						habitability += 5;
	// 					}
	// 					if (neighbour.featureComponent.HasFeature(AreaFeatureDB.Stone_Source_Feature)) {
	// 						habitability += 3;
	// 					}
	// 					if (neighbour.featureComponent.HasFeature(AreaFeatureDB.Game_Feature)) {
	// 						habitability += 5;
	// 					}
	// 				}
	// 				if (adjacentWaterTiles == 1 || adjacentCaveTiles == 1) {
	// 					habitability += 5;
	// 				}
	// 				if (adjacentFlatTiles >= 1) {
	// 					habitability += 20;
	// 				} else {
	// 					habitability -= 10;
	// 				}
	// 			}
	// 			data.habitabilityValues[x, y] = habitability;
	// 			batchCount++;
	// 			if (batchCount >= MapGenerationData.WorldMapHabitabilityGenerationBatches) {
	// 				batchCount = 0;
	// 				yield return null;
	// 			}
	// 		}	
	// 	}
	// }
	private IEnumerator DetermineVillageSpots(MapGenerationData p_data) {
		List<Area> villageSpotChoices = RuinarchListPool<Area>.Claim();

		for (int x = 0; x < p_data.width; x++) {
			for (int y = 0; y < p_data.height; y++) {
				Area currentTile = GridMap.Instance.map[x, y];
				if (IsAreaValidVillageSpotCandidate(currentTile) && IsAreaConnectedToNumberOfPlainTiles(currentTile, 6, 12)) {
					p_data.AddVillageSpot(currentTile);
				}
			}
		}
		Debug.Log($"Created {p_data.villageSpots.Count.ToString()} Village Spots");
		yield return null;
	}
	private bool IsAreaValidVillageSpotCandidate(Area p_area) {
		if (p_area.elevationType == ELEVATION.PLAIN) {
			if (p_area.neighbourComponent.HasNeighbourWithElevation(ELEVATION.WATER) || 
			    p_area.neighbourComponent.HasNeighbourWithElevation(ELEVATION.MOUNTAIN)) {
				return true;
			}
		}
		return false;
	}
	private bool IsAreaConnectedToNumberOfPlainTiles(Area p_area, int p_requiredNumber, int p_maxNumber) {
		List<Area> connectedPlainAreas = RuinarchListPool<Area>.Claim();
		connectedPlainAreas.Add(p_area);
		
		List<Area> areasToCheck = RuinarchListPool<Area>.Claim();
		List<Area> checkedAreas = RuinarchListPool<Area>.Claim();
		areasToCheck.AddRange(p_area.neighbourComponent.cardinalNeighbours);

		while (areasToCheck.Count > 0) {
			if (connectedPlainAreas.Count >= p_maxNumber) { break; }
			Area currentArea = areasToCheck[0];
			if (currentArea.elevationComponent.elevationType == ELEVATION.PLAIN) {
				connectedPlainAreas.Add(currentArea);
				//add neighbours as tile to be checked
				for (int i = 0; i < currentArea.neighbourComponent.cardinalNeighbours.Count; i++) {
					Area neighbour = currentArea.neighbourComponent.cardinalNeighbours[i];
					if (!checkedAreas.Contains(neighbour) && !areasToCheck.Contains(neighbour)) {
						areasToCheck.Add(neighbour);
					}
				}
			}
			areasToCheck.Remove(currentArea);
			checkedAreas.Add(currentArea);
		}

		return connectedPlainAreas.Count >= p_requiredNumber;
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
				int tilesInRange = villageSetting.GetTileCountReservedForVillage(WorldSettings.Instance.worldSettingsData.mapSettings.mapSize);
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
				List<Area> neighbours = RuinarchListPool<Area>.Claim();
				chosenTile.PopulateAreasInRange(neighbours, tilesInRange, false);
				neighbours.Add(chosenTile);
				data.RemoveVillageSpots(neighbours);
				RuinarchListPool<Area>.Release(neighbours);
				createdVillages++;
			}
		}
		return createdVillages == villagesToCreate;
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
